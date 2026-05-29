namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using System.Text.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;

public class FacturaVentaHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/facturas-venta";

    public FacturaVentaHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<FacturaVentaResumenDto>> GetAllAsync()
    {
        var response = await _http.GetAsync(BaseUrl);
        var parsed = await ParseResponse<IReadOnlyList<FacturaVentaResumenDto>>(response);

        if (!parsed.Success)
            throw new InvalidOperationException(parsed.Message ?? "Error al listar facturas de venta.");

        return parsed.Data?.ToList() ?? new List<FacturaVentaResumenDto>();
    }

    public async Task<FacturaVentaDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetAsync($"{BaseUrl}/{id}");
        var parsed = await ParseResponse<FacturaVentaDto>(response);

        if (!parsed.Success)
            return null;

        return parsed.Data;
    }

    public async Task<FacturaVentaDto?> GetByVentaIdAsync(long ventaId)
    {
        var response = await _http.GetAsync($"{BaseUrl}/por-venta/{ventaId}");
        var parsed = await ParseResponse<FacturaVentaDto>(response);

        if (!parsed.Success)
            return null;

        return parsed.Data;
    }

    public async Task<ApiResponse<FacturaVentaDto>> GenerarAsync(GenerarFacturaVentaRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/generar", dto);
        return await ParseResponse<FacturaVentaDto>(response);
    }

    public async Task<ApiResponse<FacturaVentaDto>> AnularAsync(long id, AnularFacturaVentaRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/{id}/anular", dto);
        return await ParseResponse<FacturaVentaDto>(response);
    }

    private static async Task<ApiResponse<T>> ParseResponse<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            try
            {
                var parsedResponse = JsonSerializer.Deserialize<ApiResponse<T>>(
                    body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (parsedResponse != null && !string.IsNullOrWhiteSpace(parsedResponse.Message))
                    return parsedResponse;
            }
            catch
            {
                // Ignorar errores de parseo y devolver mensaje HTTP crudo.
            }

            return ApiResponse<T>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }

        var ok = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
        return ok ?? ApiResponse<T>.Fail("Error de comunicación con el servidor.");
    }
}
