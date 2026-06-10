namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using System.Text.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;

public class VentaHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/ventas";

    public VentaHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<PaginatedResultDto<VentaResumenDto>>> GetAllAsync(VentaFilterDto? filter = null)
    {
        var url = $"{BaseUrl}?";
        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.NumeroVenta)) url += $"numeroVenta={Uri.EscapeDataString(filter.NumeroVenta)}&";
            if (filter.ClienteId.HasValue) url += $"clienteId={filter.ClienteId}&";
            if (!string.IsNullOrEmpty(filter.EstadoVenta)) url += $"estadoVenta={Uri.EscapeDataString(filter.EstadoVenta)}&";
            if (!string.IsNullOrEmpty(filter.EstadoPago)) url += $"estadoPago={Uri.EscapeDataString(filter.EstadoPago)}&";
            if (filter.FechaDesde.HasValue) url += $"fechaDesde={filter.FechaDesde.Value:yyyy-MM-dd}&";
            if (filter.FechaHasta.HasValue) url += $"fechaHasta={filter.FechaHasta.Value:yyyy-MM-dd}&";
            if (!string.IsNullOrEmpty(filter.Busqueda)) url += $"busqueda={Uri.EscapeDataString(filter.Busqueda)}&";
            url += $"top={filter.Top}&";
            url += $"pagina={filter.Pagina}&";
            url += $"tamanoPagina={filter.TamanoPagina}&";
            if (filter.Page.HasValue) url += $"page={filter.Page.Value}&";
            if (filter.PageSize.HasValue) url += $"pageSize={filter.PageSize.Value}&";
        }
        url = url.TrimEnd('&', '?');

        var response = await _http.GetAsync(url);
        return await ParseResponse<PaginatedResultDto<VentaResumenDto>>(response);
    }

    public async Task<ApiResponse<PaginatedResultDto<VentaPagoDto>>> GetPagosPagedAsync(VentaPagoFilterDto filter)
    {
        var url = $"{BaseUrl}/pagos?";
        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.NumeroVenta)) url += $"numeroVenta={Uri.EscapeDataString(filter.NumeroVenta)}&";
            if (!string.IsNullOrEmpty(filter.NumeroFactura)) url += $"numeroFactura={Uri.EscapeDataString(filter.NumeroFactura)}&";
            if (filter.ClienteId.HasValue) url += $"clienteId={filter.ClienteId}&";
            if (!string.IsNullOrEmpty(filter.TipoPago)) url += $"tipoPago={Uri.EscapeDataString(filter.TipoPago)}&";
            if (!string.IsNullOrEmpty(filter.EstadoPago)) url += $"estadoPago={Uri.EscapeDataString(filter.EstadoPago)}&";
            if (filter.FechaPagoDesde.HasValue) url += $"fechaPagoDesde={filter.FechaPagoDesde.Value:yyyy-MM-dd}&";
            if (filter.FechaPagoHasta.HasValue) url += $"fechaPagoHasta={filter.FechaPagoHasta.Value:yyyy-MM-dd}&";
            url += $"top={filter.Top}&";
            url += $"pagina={filter.Pagina}&";
            url += $"tamanoPagina={filter.TamanoPagina}&";
        }
        url = url.TrimEnd('&', '?');

        var response = await _http.GetAsync(url);
        return await ParseResponse<PaginatedResultDto<VentaPagoDto>>(response);
    }

    public async Task<VentaDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetAsync($"{BaseUrl}/{id}");
        var parsed = await ParseResponse<VentaDto>(response);

        if (!parsed.Success)
            return null;

        return parsed.Data;
    }

    public async Task<ApiResponse<VentaDto>> CreateAsync(CrearVentaRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        return await ParseResponse<VentaDto>(response);
    }

    public async Task<ApiResponse<VentaDto>> UpdateAsync(long id, ActualizarVentaRequestDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        return await ParseResponse<VentaDto>(response);
    }

    public async Task<ApiResponse<VentaDto>> ConfirmarAsync(long id, ConfirmarVentaRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/{id}/confirmar", dto);
        return await ParseResponse<VentaDto>(response);
    }

    public async Task<ApiResponse<VentaDto>> CrearDesdePedidoAsync(GenerarVentaDesdePedidoRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/desde-pedido", dto);
        return await ParseResponse<VentaDto>(response);
    }

    public async Task<ApiResponse<VentaDto>> RegistrarPagoAsync(long id, RegistrarPagoVentaRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/{id}/pagos", dto);
        return await ParseResponse<VentaDto>(response);
    }

    public async Task<ApiResponse<VentaDto>> AnularAsync(long id, AnularVentaRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/{id}/anular", dto);
        return await ParseResponse<VentaDto>(response);
    }

    public async Task<ApiResponse<VentaDto>> AnularPagoAsync(long id, AnularPagoVentaRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/pagos/{id}/anular", dto);
        return await ParseResponse<VentaDto>(response);
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
