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

    public async Task<ApiResponse<PaginatedResultDto<FacturaVentaResumenDto>>> GetAllAsync(FacturaVentaFilterDto? filter = null)
    {
        var url = $"{BaseUrl}?";
        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.NumeroFactura)) url += $"numeroFactura={Uri.EscapeDataString(filter.NumeroFactura)}&";
            if (filter.ClienteId.HasValue) url += $"clienteId={filter.ClienteId}&";
            if (!string.IsNullOrEmpty(filter.EstadoFactura)) url += $"estadoFactura={Uri.EscapeDataString(filter.EstadoFactura)}&";
            if (filter.FechaEmisionDesde.HasValue) url += $"fechaEmisionDesde={filter.FechaEmisionDesde.Value:yyyy-MM-dd}&";
            if (filter.FechaEmisionHasta.HasValue) url += $"fechaEmisionHasta={filter.FechaEmisionHasta.Value:yyyy-MM-dd}&";
            if (!string.IsNullOrEmpty(filter.Busqueda)) url += $"busqueda={Uri.EscapeDataString(filter.Busqueda)}&";
            if (!string.IsNullOrEmpty(filter.Moneda)) url += $"moneda={Uri.EscapeDataString(filter.Moneda)}&";
            url += $"top={filter.Top}&";
            url += $"pagina={filter.Pagina}&";
            url += $"tamanoPagina={filter.TamanoPagina}&";
            if (filter.Page.HasValue) url += $"page={filter.Page.Value}&";
            if (filter.PageSize.HasValue) url += $"pageSize={filter.PageSize.Value}&";
        }
        url = url.TrimEnd('&', '?');

        var response = await _http.GetAsync(url);
        return await ParseResponse<PaginatedResultDto<FacturaVentaResumenDto>>(response);
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
