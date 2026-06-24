using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.FE;
using AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;
using Microsoft.Extensions.Logging;

namespace AgoraHub360.ERP.Infrastructure.FacturacionElectronica;

/// <summary>
/// Provider de facturación electrónica para Cirrus.
/// Implementa IFacturacionElectronicaProvider con OAuth2 client_credentials.
/// Toda la configuración (URLs, credenciales, NIT, ActivityCode) viene desde BD.
/// </summary>
public class CirrusFacturacionProvider : IFacturacionElectronicaProvider
{
    public string CodigoProveedor => "CIRRUS";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICifradoService _cifradoService;
    private readonly ILogger<CirrusFacturacionProvider> _logger;

    // Cache de tokens: key = config.Id, value = (AccessToken, ExpiresUtc)
    private static readonly ConcurrentDictionary<int, (string AccessToken, DateTime ExpiresUtc)> _tokenCache = new();
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> _tokenLocks = new();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public CirrusFacturacionProvider(
        IHttpClientFactory httpClientFactory,
        ICifradoService cifradoService,
        ILogger<CirrusFacturacionProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cifradoService = cifradoService;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  EmitirFacturaAsync
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<EmisionFacturaResultDto> EmitirFacturaAsync(
        ConfiguracionFacturacionElectronica configuracion,
        EmitirFacturaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var accessToken = await ObtenerTokenAsync(configuracion, cancellationToken);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return new EmisionFacturaResultDto
                {
                    Exitoso = false,
                    EstadoSiat = "NoEnviada",
                    Mensaje = "No se pudo obtener token de autenticación del proveedor."
                };
            }

            var posToken = DescifrarPosToken(configuracion);
            var billRequest = MapToCirrusRequest(request);

            var client = _httpClientFactory.CreateClient("CirrusFacturacion");
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, configuracion.TimeoutSegundos));

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{configuracion.ApiBillingUrl}/bill/sales")
            {
                Content = JsonContent.Create(billRequest, options: _jsonOptions)
            };
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            if (!string.IsNullOrWhiteSpace(posToken))
            {
                httpRequest.Headers.Add("Authorization-Pos-Token", posToken);
            }

            using var response = await client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Cirrus emisión responded HTTP {StatusCode} para BillUuid={BillUuid}",
                    (int)response.StatusCode, request.BillUuid);
                return new EmisionFacturaResultDto
                {
                    Exitoso = false,
                    EstadoSiat = "Rechazada",
                    Mensaje = $"HTTP {(int)response.StatusCode}: error en comunicación con Cirrus.",
                    CodigoRespuestaProveedor = ((int)response.StatusCode).ToString()
                };
            }

            var cirrusResponse = JsonSerializer.Deserialize<CirrusApiResponse<CirrusBillResponse>>(body, _jsonOptions);
            if (cirrusResponse?.Response == null)
            {
                // Intentar deserializar como lista (casos donde response es array con un elemento)
                var cirrusList = JsonSerializer.Deserialize<CirrusApiResponse<List<CirrusBillResponse>>>(body, _jsonOptions);
                var billResponse = cirrusList?.Response?.FirstOrDefault();

                if (billResponse == null)
                {
                    return new EmisionFacturaResultDto
                    {
                        Exitoso = false,
                        EstadoSiat = "Rechazada",
                        Mensaje = cirrusResponse?.ErrorDetail ?? "Respuesta inválida del proveedor.",
                        CodigoRespuestaProveedor = cirrusResponse?.StatusCode ?? cirrusResponse?.ValidStatusCode
                    };
                }

                return MapEmisionResult(billResponse, request.BillUuid);
            }

            return MapEmisionResult(cirrusResponse.Response, request.BillUuid);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Timeout al emitir factura BillUuid={BillUuid}", request.BillUuid);
            return new EmisionFacturaResultDto
            {
                Exitoso = false,
                EstadoSiat = "NoEnviada",
                Mensaje = $"Timeout después de {configuracion.TimeoutSegundos} segundos."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al emitir factura BillUuid={BillUuid}", request.BillUuid);
            return new EmisionFacturaResultDto
            {
                Exitoso = false,
                EstadoSiat = "NoEnviada",
                Mensaje = $"Error interno: {ex.Message}"
            };
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  AnularFacturaAsync
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<AnulacionFacturaResultDto> AnularFacturaAsync(
        ConfiguracionFacturacionElectronica configuracion,
        string cuf,
        string motivoAnulacion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var accessToken = await ObtenerTokenAsync(configuracion, cancellationToken);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return new AnulacionFacturaResultDto
                {
                    Exitoso = false,
                    Mensaje = "No se pudo obtener token de autenticación del proveedor."
                };
            }

            var cufEncoded = Uri.EscapeDataString(cuf);
            var motivoEncoded = Uri.EscapeDataString(motivoAnulacion);
            var url = $"{configuracion.ApiManagementUrl}/api/v1/bill/void?cuf={cufEncoded}&motivoAnulacion={motivoEncoded}";

            var client = _httpClientFactory.CreateClient("CirrusFacturacion");
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, configuracion.TimeoutSegundos));

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Cirrus anulación responded HTTP {StatusCode}", (int)response.StatusCode);
                return new AnulacionFacturaResultDto
                {
                    Exitoso = false,
                    Mensaje = $"HTTP {(int)response.StatusCode}: error en comunicación con Cirrus.",
                    CodigoRespuestaProveedor = ((int)response.StatusCode).ToString()
                };
            }

            var cirrusResponse = JsonSerializer.Deserialize<CirrusApiResponse<CirrusVoidResponse>>(body, _jsonOptions);
            if (cirrusResponse?.Response == null)
            {
                return new AnulacionFacturaResultDto
                {
                    Exitoso = false,
                    Mensaje = cirrusResponse?.ErrorDetail ?? "Respuesta inválida del proveedor.",
                    CodigoRespuestaProveedor = cirrusResponse?.StatusCode ?? cirrusResponse?.ValidStatusCode
                };
            }

            return new AnulacionFacturaResultDto
            {
                Exitoso = cirrusResponse.Response.Status == "VALIDATED",
                EstadoSiat = cirrusResponse.Response.Status ?? "Anulada",
                Cuf = cuf,
                Mensaje = cirrusResponse.Response.Description,
                CodigoRespuestaProveedor = cirrusResponse.StatusCode ?? cirrusResponse.ValidStatusCode,
                DescripcionRespuestaProveedor = cirrusResponse.Response.Status
            };
        }
        catch (TaskCanceledException)
        {
            return new AnulacionFacturaResultDto
            {
                Exitoso = false,
                Mensaje = $"Timeout después de {configuracion.TimeoutSegundos} segundos."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al anular factura CUF={CufLast6}", cuf.Length > 6 ? cuf[^6..] : cuf);
            return new AnulacionFacturaResultDto
            {
                Exitoso = false,
                Mensaje = $"Error interno: {ex.Message}"
            };
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  VerificarEstadoAsync
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<EstadoFacturaResultDto> VerificarEstadoAsync(
        ConfiguracionFacturacionElectronica configuracion,
        string cuf,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var accessToken = await ObtenerTokenAsync(configuracion, cancellationToken);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return new EstadoFacturaResultDto
                {
                    Exitoso = false,
                    Mensaje = "No se pudo obtener token de autenticación del proveedor."
                };
            }

            var cufEncoded = Uri.EscapeDataString(cuf);
            var url = $"{configuracion.ApiManagementUrl}/api/v1/bill/status?cuf={cufEncoded}";

            var client = _httpClientFactory.CreateClient("CirrusFacturacion");
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, configuracion.TimeoutSegundos));

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new EstadoFacturaResultDto
                {
                    Exitoso = false,
                    Mensaje = $"HTTP {(int)response.StatusCode}: error en comunicación con Cirrus.",
                    CodigoRespuestaProveedor = ((int)response.StatusCode).ToString()
                };
            }

            var cirrusResponse = JsonSerializer.Deserialize<CirrusApiResponse<CirrusStatusResponse>>(body, _jsonOptions);

            return new EstadoFacturaResultDto
            {
                Exitoso = cirrusResponse?.Response != null,
                EstadoSiat = cirrusResponse?.Response?.Status ?? "NoEnviada",
                Cuf = cuf,
                Mensaje = cirrusResponse?.Response?.Description,
                CodigoRespuestaProveedor = cirrusResponse?.StatusCode ?? cirrusResponse?.ValidStatusCode,
                DescripcionRespuestaProveedor = cirrusResponse?.Response?.Status,
                FechaConsultaUtc = DateTime.UtcNow
            };
        }
        catch (TaskCanceledException)
        {
            return new EstadoFacturaResultDto
            {
                Exitoso = false,
                Mensaje = $"Timeout después de {configuracion.TimeoutSegundos} segundos."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar estado CUF={CufLast6}", cuf.Length > 6 ? cuf[^6..] : cuf);
            return new EstadoFacturaResultDto
            {
                Exitoso = false,
                Mensaje = $"Error interno: {ex.Message}"
            };
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  TestConexionAsync
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<TestConexionResultDto> TestConexionAsync(
        ConfiguracionFacturacionElectronica configuracion,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var accessToken = await ObtenerTokenAsync(configuracion, cancellationToken);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                sw.Stop();
                return new TestConexionResultDto
                {
                    Exitoso = false,
                    ProveedorCodigo = CodigoProveedor,
                    AmbienteCodigo = configuracion.AmbienteFacturacionElectronica?.Codigo,
                    Mensaje = "No se pudo obtener token de autenticación del proveedor.",
                    TiempoRespuestaMs = sw.ElapsedMilliseconds,
                    DetalleTecnico = "ObtenerTokenAsync devolvió null. Verifique ClientId, ClientSecret cifrado y TokenUrl."
                };
            }

            // Intentar obtener CUFD como prueba adicional de conectividad
            var cufd = await ObtenerCufdAsync(configuracion, cancellationToken);
            sw.Stop();

            if (string.IsNullOrWhiteSpace(cufd))
            {
                return new TestConexionResultDto
                {
                    Exitoso = true,
                    ProveedorCodigo = CodigoProveedor,
                    AmbienteCodigo = configuracion.AmbienteFacturacionElectronica?.Codigo,
                    Mensaje = "Token obtenido correctamente, pero no se pudo obtener CUFD. La conexión básica funciona.",
                    TiempoRespuestaMs = sw.ElapsedMilliseconds
                };
            }

            return new TestConexionResultDto
            {
                Exitoso = true,
                ProveedorCodigo = CodigoProveedor,
                AmbienteCodigo = configuracion.AmbienteFacturacionElectronica?.Codigo,
                Mensaje = $"Conexión exitosa. CUFD obtenido: {cufd[..Math.Min(cufd.Length, 20)]}...",
                TiempoRespuestaMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Error en TestConexionAsync para proveedor {Codigo}", CodigoProveedor);
            return new TestConexionResultDto
            {
                Exitoso = false,
                ProveedorCodigo = CodigoProveedor,
                AmbienteCodigo = configuracion.AmbienteFacturacionElectronica?.Codigo,
                Mensaje = $"Error de conexión: {ex.Message}",
                TiempoRespuestaMs = sw.ElapsedMilliseconds,
                DetalleTecnico = $"{ex.GetType().Name}: {ex.Message}"
            };
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ObtenerCufdAsync
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<string> ObtenerCufdAsync(
        ConfiguracionFacturacionElectronica configuracion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var accessToken = await ObtenerTokenAsync(configuracion, cancellationToken);
            if (string.IsNullOrWhiteSpace(accessToken))
                return string.Empty;

            var posToken = DescifrarPosToken(configuracion);

            var client = _httpClientFactory.CreateClient("CirrusFacturacion");
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, configuracion.TimeoutSegundos));

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{configuracion.ApiManagementUrl}/api/v1/bill/cufd");
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            if (!string.IsNullOrWhiteSpace(posToken))
            {
                httpRequest.Headers.Add("Authorization-Pos-Token", posToken);
            }

            using var response = await client.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                return string.Empty;

            var cirrusResponse = JsonSerializer.Deserialize<CirrusApiResponse<CirrusCufdResponse>>(body, _jsonOptions);
            return cirrusResponse?.Response?.Cufd ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener CUFD");
            return string.Empty;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  OAuth2 Token
    // ─────────────────────────────────────────────────────────────────────────
    private async Task<string?> ObtenerTokenAsync(ConfiguracionFacturacionElectronica config, CancellationToken ct)
    {
        // Verificar cache
        if (_tokenCache.TryGetValue(config.Id, out var cached))
        {
            if (cached.ExpiresUtc > DateTime.UtcNow.AddMinutes(5))
                return cached.AccessToken;
        }

        var semaphore = _tokenLocks.GetOrAdd(config.Id, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);

        try
        {
            // Double-check después del lock
            if (_tokenCache.TryGetValue(config.Id, out var recheck))
            {
                if (recheck.ExpiresUtc > DateTime.UtcNow.AddMinutes(5))
                    return recheck.AccessToken;
            }

            var clientSecret = _cifradoService.Descifrar(config.ClientSecretEncrypted);

            var client = _httpClientFactory.CreateClient("CirrusFacturacion");
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, config.TimeoutSegundos));

            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = config.ClientId,
                ["client_secret"] = clientSecret,
                ["response_type"] = "code"
            };

            using var response = await client.PostAsync(config.TokenUrl, new FormUrlEncodedContent(formData), ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Fallo OAuth2 para config {ConfigId}: HTTP {StatusCode}",
                    config.Id, (int)response.StatusCode);
                return null;
            }

            var tokenResponse = JsonSerializer.Deserialize<CirrusTokenResponse>(body, _jsonOptions);
            if (tokenResponse?.AccessToken == null)
            {
                _logger.LogWarning("Respuesta OAuth2 sin access_token para config {ConfigId}", config.Id);
                return null;
            }

            var expiresUtc = DateTime.UtcNow.AddSeconds(Math.Max(tokenResponse.ExpiresIn - 300, 60));
            _tokenCache[config.Id] = (tokenResponse.AccessToken, expiresUtc);

            return tokenResponse.AccessToken;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private string? DescifrarPosToken(ConfiguracionFacturacionElectronica config)
    {
        if (string.IsNullOrWhiteSpace(config.PosTokenEncrypted))
            return null;

        try
        {
            return _cifradoService.Descifrar(config.PosTokenEncrypted);
        }
        catch
        {
            _logger.LogWarning("No se pudo descifrar PosToken para config {ConfigId}", config.Id);
            return null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Mapping
    // ─────────────────────────────────────────────────────────────────────────
    private static CirrusBillRequest MapToCirrusRequest(EmitirFacturaRequestDto request)
    {
        var inv = CultureInfo.InvariantCulture;

        return new CirrusBillRequest
        {
            BillUuid = request.BillUuid,
            ActivityCode = request.ActivityCode,
            BeneficiaryDocNumber = request.BeneficiaryDocNumber,
            DocNumberComplement = request.DocNumberComplement,
            IdentityDocTypeCode = request.IdentityDocTypeCode ?? "NIT",
            BillingName = request.BillingName,
            BeneficiaryName = request.BeneficiaryName,
            BeneficiaryEmail = request.BeneficiaryEmail,
            PaymentMethodCode = request.PaymentMethodCode ?? "1",
            CardNumber = request.CardNumber,
            AdditionalDiscount = request.AdditionalDiscount.ToString("F2", inv),
            GiftCardAmount = request.GiftCardAmount.ToString("F2", inv),
            ItemList = request.Items.Select(i => new CirrusItemRequest
            {
                ItemCode = i.ItemCode,
                DetailQuantity = i.Cantidad.ToString("F2", inv),
                UnitPrice = i.PrecioUnitario.ToString("F2", inv),
                DiscountAmount = i.DescuentoMonto.ToString("F2", inv),
                ItemNewDescription = i.Descripcion
            }).ToList()
        };
    }

    private static EmisionFacturaResultDto MapEmisionResult(CirrusBillResponse billResponse, string billUuid)
    {
        var status = billResponse.Status?.Trim().ToUpperInvariant() ?? "";
        var exitoso = status is "VALIDATED" or "OFFLINE";
        var esOffline = status == "OFFLINE";

        return new EmisionFacturaResultDto
        {
            Exitoso = exitoso,
            EstadoSiat = status switch
            {
                "VALIDATED" => "Validada",
                "OFFLINE" => "Pendiente",
                "REJECTED" => "Rechazada",
                _ => "Pendiente"
            },
            BillUuid = billUuid,
            Cuf = billResponse.Cuf,
            Cufd = billResponse.Cufd,
            SiatQr = billResponse.QrCode,
            EnlacePdf = billResponse.PdfUrl,
            EnlaceXml = billResponse.XmlUrl,
            Mensaje = billResponse.Description,
            CodigoRespuestaProveedor = status,
            DescripcionRespuestaProveedor = billResponse.Description,
            EsOffline = esOffline
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Modelos internos Cirrus
    // ─────────────────────────────────────────────────────────────────────────

    private class CirrusTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }

    private class CirrusApiResponse<T>
    {
        [JsonPropertyName("statusCode")]
        public string? StatusCode { get; set; }
        [JsonPropertyName("response")]
        public T? Response { get; set; }
        [JsonPropertyName("errorDetail")]
        public string? ErrorDetail { get; set; }
        [JsonPropertyName("httpStatus")]
        public int? HttpStatus { get; set; }
        [JsonPropertyName("validStatusCode")]
        public string? ValidStatusCode { get; set; }
    }

    private class CirrusBillRequest
    {
        [JsonPropertyName("billUuid")]
        public string BillUuid { get; set; } = string.Empty;
        [JsonPropertyName("activityCode")]
        public string ActivityCode { get; set; } = string.Empty;
        [JsonPropertyName("beneficiaryDocNumber")]
        public string BeneficiaryDocNumber { get; set; } = string.Empty;
        [JsonPropertyName("docNumberComplement")]
        public string? DocNumberComplement { get; set; }
        [JsonPropertyName("identityDocTypeCode")]
        public string IdentityDocTypeCode { get; set; } = "NIT";
        [JsonPropertyName("billingName")]
        public string BillingName { get; set; } = string.Empty;
        [JsonPropertyName("beneficiaryName")]
        public string BeneficiaryName { get; set; } = string.Empty;
        [JsonPropertyName("beneficiaryEmail")]
        public string? BeneficiaryEmail { get; set; }
        [JsonPropertyName("paymentMethodCode")]
        public string PaymentMethodCode { get; set; } = "1";
        [JsonPropertyName("cardNumber")]
        public string? CardNumber { get; set; }
        [JsonPropertyName("additionalDiscount")]
        public string AdditionalDiscount { get; set; } = "0.00";
        [JsonPropertyName("giftCardAmount")]
        public string GiftCardAmount { get; set; } = "0.00";
        [JsonPropertyName("itemList")]
        public List<CirrusItemRequest> ItemList { get; set; } = new();
    }

    private class CirrusItemRequest
    {
        [JsonPropertyName("itemCode")]
        public string ItemCode { get; set; } = string.Empty;
        [JsonPropertyName("detailQuantity")]
        public string DetailQuantity { get; set; } = "0.00";
        [JsonPropertyName("unitPrice")]
        public string UnitPrice { get; set; } = "0.00";
        [JsonPropertyName("discountAmount")]
        public string DiscountAmount { get; set; } = "0.00";
        [JsonPropertyName("itemNewDescription")]
        public string? ItemNewDescription { get; set; }
    }

    private class CirrusBillResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        [JsonPropertyName("cuf")]
        public string? Cuf { get; set; }
        [JsonPropertyName("cufd")]
        public string? Cufd { get; set; }
        [JsonPropertyName("authorization_number")]
        public string? AuthorizationNumber { get; set; }
        [JsonPropertyName("qr_code")]
        public string? QrCode { get; set; }
        [JsonPropertyName("xml_url")]
        public string? XmlUrl { get; set; }
        [JsonPropertyName("pdf_url")]
        public string? PdfUrl { get; set; }
        [JsonPropertyName("legend")]
        public string? Legend { get; set; }
        [JsonPropertyName("legal_footer")]
        public string? LegalFooter { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    private class CirrusVoidResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    private class CirrusStatusResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    private class CirrusCufdResponse
    {
        [JsonPropertyName("cufd")]
        public string? Cufd { get; set; }
    }
}
