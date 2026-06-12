using System.Net.Http.Json;
using System.Text.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

namespace AgoraHub360.ERP.Web.Services;

public class FacturacionFEHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/facturacion-electronica";

    public FacturacionFEHttpService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Parsea la respuesta HTTP a ApiResponse&lt;T&gt;.
    /// Si la respuesta no es exitosa, extrae el mensaje de error del body
    /// sin depender del formato exacto de la propiedad "errors".
    /// </summary>
    private async Task<ApiResponse<T>> ParseResponseAsync<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<T>>()
                       ?? ApiResponse<T>.Fail("Error de comunicación. No se recibió respuesta válida del servidor.");
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.Fail($"Error al procesar la respuesta del servidor: {ex.Message}");
            }
        }

        // Leer el body para extraer el mensaje de error
        var body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
            return ApiResponse<T>.Fail($"Error HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // Intentar extraer "message" del body
            if (root.TryGetProperty("message", out var msgEl) && msgEl.ValueKind == JsonValueKind.String)
            {
                var msg = msgEl.GetString();
                if (!string.IsNullOrWhiteSpace(msg))
                    return ApiResponse<T>.Fail(msg);
            }

            // Intentar extraer "title" (usado por ASP.NET Core en errores 400)
            if (root.TryGetProperty("title", out var titleEl) && titleEl.ValueKind == JsonValueKind.String)
            {
                var title = titleEl.GetString();
                if (!string.IsNullOrWhiteSpace(title))
                    return ApiResponse<T>.Fail(title);
            }

            return ApiResponse<T>.Fail($"Error HTTP {(int)response.StatusCode}: {body[..Math.Min(body.Length, 200)]}");
        }
        catch
        {
            return ApiResponse<T>.Fail($"Error HTTP {(int)response.StatusCode}: {body[..Math.Min(body.Length, 200)]}");
        }
    }

    /// <summary>
    /// Parsea una respuesta GET que retorna una lista, retornando lista vacía en caso de error.
    /// </summary>
    private async Task<List<T>> ParseListAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            return new List<T>();

        try
        {
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<T>>>();
            return apiResponse?.Data ?? new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }

    /// <summary>
    /// Parsea una respuesta GET que retorna un objeto nullable.
    /// </summary>
    private async Task<T?> ParseNullableAsync<T>(HttpResponseMessage response) where T : class
    {
        if (!response.IsSuccessStatusCode)
            return null;

        try
        {
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
            return apiResponse?.Data;
        }
        catch
        {
            return null;
        }
    }

    // ── Proveedores ──

    public async Task<List<ProveedorFEDto>> GetProveedoresAsync()
    {
        var response = await _http.GetAsync($"{BaseUrl}/proveedores");
        return await ParseListAsync<ProveedorFEDto>(response);
    }

    // ── Ambientes ──

    public async Task<List<AmbienteFEDto>> GetAmbientesAsync()
    {
        var response = await _http.GetAsync($"{BaseUrl}/ambientes");
        return await ParseListAsync<AmbienteFEDto>(response);
    }

    // ── Configuraciones ──

    public async Task<List<ConfiguracionFEDto>> GetConfiguracionesAsync()
    {
        var response = await _http.GetAsync($"{BaseUrl}/configuraciones");
        return await ParseListAsync<ConfiguracionFEDto>(response);
    }

    public async Task<ConfiguracionFEDto?> GetConfiguracionByIdAsync(int id)
    {
        var response = await _http.GetAsync($"{BaseUrl}/configuraciones/{id}");
        return await ParseNullableAsync<ConfiguracionFEDto>(response);
    }

    public async Task<ApiResponse<ConfiguracionFEDto>> CrearConfiguracionAsync(CrearConfiguracionFERequestDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/configuraciones", dto);
        return await ParseResponseAsync<ConfiguracionFEDto>(response);
    }

    public async Task<ApiResponse<ConfiguracionFEDto>> ActualizarConfiguracionAsync(int id, ActualizarConfiguracionFERequestDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/configuraciones/{id}", dto);
        return await ParseResponseAsync<ConfiguracionFEDto>(response);
    }

    public async Task<ApiResponse<ConfiguracionFEDto>> ActivarConfiguracionAsync(int id)
    {
        var response = await _http.PostAsync($"{BaseUrl}/configuraciones/{id}/activar", null);
        return await ParseResponseAsync<ConfiguracionFEDto>(response);
    }

    public async Task<ApiResponse<bool>> DesactivarConfiguracionAsync(int id)
    {
        var response = await _http.PostAsync($"{BaseUrl}/configuraciones/{id}/desactivar", null);
        return await ParseResponseAsync<bool>(response);
    }

    // ── Operaciones FE ──

    public async Task<ApiResponse<EmisionFacturaResultDto>> EmitirAsync(long facturaVentaId)
    {
        var response = await _http.PostAsync($"{BaseUrl}/emitir/{facturaVentaId}", null);
        return await ParseResponseAsync<EmisionFacturaResultDto>(response);
    }

    public async Task<ApiResponse<AnulacionFacturaResultDto>> AnularAsync(long facturaVentaId, string motivo)
    {
        var dto = new AnularFacturaFERequestDto { MotivoAnulacion = motivo };
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/anular/{facturaVentaId}", dto);
        return await ParseResponseAsync<AnulacionFacturaResultDto>(response);
    }

    public async Task<ApiResponse<EstadoFacturaResultDto>> VerificarEstadoAsync(long facturaVentaId)
    {
        var response = await _http.GetAsync($"{BaseUrl}/estado/{facturaVentaId}");
        return await ParseResponseAsync<EstadoFacturaResultDto>(response);
    }

    // ── Auditoría ──

    public async Task<List<AuditoriaFEDto>> ListarAuditoriaAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        string? estadoSiat = null,
        string? proveedorCodigo = null,
        long? facturaVentaId = null)
    {
        var query = new List<string>();
        if (desde.HasValue) query.Add($"desde={desde.Value:yyyy-MM-dd}");
        if (hasta.HasValue) query.Add($"hasta={hasta.Value:yyyy-MM-dd}");
        if (!string.IsNullOrEmpty(estadoSiat)) query.Add($"estadoSiat={Uri.EscapeDataString(estadoSiat)}");
        if (!string.IsNullOrEmpty(proveedorCodigo)) query.Add($"proveedorCodigo={Uri.EscapeDataString(proveedorCodigo)}");
        if (facturaVentaId.HasValue) query.Add($"facturaVentaId={facturaVentaId.Value}");

        var url = $"{BaseUrl}/auditoria";
        if (query.Count > 0) url += "?" + string.Join("&", query);

        var response = await _http.GetAsync(url);
        return await ParseListAsync<AuditoriaFEDto>(response);
    }

    public async Task<List<AuditoriaFEDto>> ListarAuditoriaPorFacturaAsync(long facturaVentaId)
    {
        var response = await _http.GetAsync($"{BaseUrl}/auditoria/factura/{facturaVentaId}");
        return await ParseListAsync<AuditoriaFEDto>(response);
    }
}
