using System.Net;
using System.Net.Http;
using System.Text.Json;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.FE;
using AgoraHub360.ERP.Infrastructure.FacturacionElectronica;
using AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AgoraHub360.ERP.Tests.Infrastructure;

public class CirrusFacturacionProviderTests
{
    [Fact]
    public void CodigoProveedor_DeberiaSerCIRRUS()
    {
        var provider = CrearProviderConHandler(null);
        Assert.Equal("CIRRUS", provider.CodigoProveedor);
    }

    [Fact]
    public async Task EmitirFacturaAsync_CuandoTokenFalla_DeberiaRetornarExitosoFalse()
    {
        // Token endpoint returns 401
        var handler = new FakeHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath.Contains("token") == true ||
                request.RequestUri?.AbsolutePath.Contains("oauth") == true)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("{\"error\":\"invalid_client\"}")
                });
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"statusCode\":\"VALIDATED\"}")
            });
        });

        var provider = CrearProviderConHandler(handler);
        // Usar Id único para evitar colisión con el caché estático de tokens
        var config = CrearConfiguracion(Id: 9998);
        var request = CrearRequest();

        var result = await provider.EmitirFacturaAsync(config, request);

        Assert.False(result.Exitoso);
        Assert.Contains("token", result.Mensaje ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EmitirFacturaAsync_CuandoRespuestaValidated_DeberiaRetornarExitosoTrue()
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath.Contains("token") == true ||
                request.RequestUri?.AbsolutePath.Contains("oauth") == true)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"access_token\":\"test-token\",\"expires_in\":3600}")
                });
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"statusCode\":\"VALIDATED\",\"response\":{\"status\":\"VALIDATED\",\"cuf\":\"CUF-TEST-123\",\"cufd\":\"CUFD-TEST\",\"qr_code\":\"qr-url\",\"pdf_url\":\"pdf-url\",\"xml_url\":\"xml-url\",\"description\":\"Factura emitida correctamente\"}}")
            });
        });

        var provider = CrearProviderConHandler(handler);
        var config = CrearConfiguracion();
        var request = CrearRequest();

        var result = await provider.EmitirFacturaAsync(config, request);

        Assert.True(result.Exitoso);
        Assert.Equal("Validada", result.EstadoSiat);
        Assert.Equal("CUF-TEST-123", result.Cuf);
        Assert.False(result.EsOffline);
    }

    [Fact]
    public async Task EmitirFacturaAsync_CuandoRespuestaOffline_DeberiaRetornarExitosoTrueYEsOfflineTrue()
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath.Contains("token") == true ||
                request.RequestUri?.AbsolutePath.Contains("oauth") == true)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"access_token\":\"test-token\",\"expires_in\":3600}")
                });
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"statusCode\":\"OFFLINE\",\"response\":{\"status\":\"OFFLINE\",\"cuf\":\"CUF-OFFLINE\",\"description\":\"Factura registrada en modo offline\"}}")
            });
        });

        var provider = CrearProviderConHandler(handler);
        var config = CrearConfiguracion();
        var request = CrearRequest();

        var result = await provider.EmitirFacturaAsync(config, request);

        Assert.True(result.Exitoso);
        Assert.Equal("Pendiente", result.EstadoSiat);
        Assert.True(result.EsOffline);
    }

    [Fact]
    public async Task EmitirFacturaAsync_CuandoRespuestaRejected_DeberiaRetornarExitosoFalse()
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath.Contains("token") == true ||
                request.RequestUri?.AbsolutePath.Contains("oauth") == true)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"access_token\":\"test-token\",\"expires_in\":3600}")
                });
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"statusCode\":\"REJECTED\",\"response\":{\"status\":\"REJECTED\",\"description\":\"NIT del emisor no coincide\"}}")
            });
        });

        var provider = CrearProviderConHandler(handler);
        var config = CrearConfiguracion();
        var request = CrearRequest();

        var result = await provider.EmitirFacturaAsync(config, request);

        Assert.False(result.Exitoso);
        Assert.Equal("Rechazada", result.EstadoSiat);
    }

    [Fact]
    public async Task EmitirFacturaAsync_CuandoTimeout_DeberiaRetornarExitosoFalse()
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath.Contains("token") == true ||
                request.RequestUri?.AbsolutePath.Contains("oauth") == true)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"access_token\":\"test-token\",\"expires_in\":3600}")
                });
            }
            throw new OperationCanceledException("Simulated timeout");
        });

        var provider = CrearProviderConHandler(handler);
        var config = CrearConfiguracion();
        var request = CrearRequest();

        var result = await provider.EmitirFacturaAsync(config, request);

        Assert.False(result.Exitoso);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static CirrusFacturacionProvider CrearProviderConHandler(FakeHttpMessageHandler? handler)
    {
        var httpClient = handler != null
            ? new HttpClient(handler) { BaseAddress = new Uri("https://test.cirrus.com") }
            : new HttpClient();

        var httpClientFactory = new FakeHttpClientFactory(httpClient);
        var cifradoService = new FakeCifradoService();
        var logger = NullLogger<CirrusFacturacionProvider>.Instance;

        return new CirrusFacturacionProvider(httpClientFactory, cifradoService, logger);
    }

    private static ConfiguracionFacturacionElectronica CrearConfiguracion(int Id = 1)
    {
        return new ConfiguracionFacturacionElectronica
        {
            Id = Id,
            EmpresaId = 1,
            NombreConfiguracion = "Test Config",
            ProveedorFacturacionElectronicaId = 1,
            AmbienteFacturacionElectronicaId = 1,
            ClientId = "test-client",
            ClientSecretEncrypted = "v1:dGVzdC1jaXBoZXJ0ZXh0",
            TokenUrl = "https://test.cirrus.com/oauth/token",
            ApiManagementUrl = "https://test.cirrus.com/api/v1",
            ApiBillingUrl = "https://test.cirrus.com",
            PosTokenEncrypted = "v1:dGVzdC1wb3MtdG9rZW4=",
            ActivityCode = "123456",
            NitEmisor = "1234567890",
            TimeoutSegundos = 30,
            EsConfiguracionActiva = true
        };
    }

    private static EmitirFacturaRequestDto CrearRequest()
    {
        return new EmitirFacturaRequestDto
        {
            FacturaVentaId = 1,
            VentaId = 1,
            EmpresaId = 1,
            BillUuid = Guid.NewGuid().ToString(),
            ActivityCode = "123456",
            NitEmisor = "1234567890",
            BeneficiaryDocNumber = "123456789",
            IdentityDocTypeCode = "NIT",
            BillingName = "Test Client",
            BeneficiaryName = "Test Client",
            PaymentMethodCode = "1",
            Items = new List<FacturacionFEItemDto>
            {
                new()
                {
                    ItemCode = "ITEM-001",
                    Descripcion = "Test Product",
                    Cantidad = 2,
                    PrecioUnitario = 100.50m,
                    DescuentoMonto = 0
                }
            }
        };
    }

    /// <summary>
    /// Fake HttpMessageHandler que ejecuta un callback.
    /// </summary>
    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await _handler(request);
        }
    }

    /// <summary>
    /// Fake IHttpClientFactory que siempre retorna el mismo HttpClient.
    /// </summary>
    private class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _httpClient;

        public FakeHttpClientFactory(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpClient CreateClient(string name) => _httpClient;
    }

    /// <summary>
    /// Fake ICifradoService que devuelve el texto "cifrado" y "descifrado" sin cifrar realmente.
    /// </summary>
    private class FakeCifradoService : ICifradoService
    {
        public string Cifrar(string texto) => texto;
        public string Descifrar(string textoCifrado)
        {
            // El formato "v1:..." se usa para detectar valores cifrados
            if (textoCifrado.StartsWith("v1:"))
                return textoCifrado[3..]; // Retorna el "cifrado" como "descifrado" para test
            return textoCifrado;
        }
    }
}
