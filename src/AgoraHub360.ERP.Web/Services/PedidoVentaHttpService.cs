using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;

namespace AgoraHub360.ERP.Web.Services;

public class PedidoVentaHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/ventas/pedidos";

    public PedidoVentaHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<PedidoVentaDto>> GetAllAsync()
    {
        var response = await _http.GetAsync(BaseUrl);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Error HTTP {(int)response.StatusCode}: {body}");
        }

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<List<PedidoVentaDto>>>();
        if (payload is null)
            throw new InvalidOperationException("Respuesta vacía del endpoint de pedidos.");

        if (!payload.Success)
            throw new InvalidOperationException(payload.Message ?? "La API devolvió un error al listar pedidos.");

        return payload.Data ?? new List<PedidoVentaDto>();
    }

    /// <summary>
    /// Obtiene pedidos paginados con filtros.
    /// </summary>
    public async Task<ApiResponse<PaginatedResultDto<PedidoVentaDto>>> GetPagedAsync(PedidoVentaFilterDto filter)
    {
        var url = $"{BaseUrl}/paged?";
        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.Busqueda)) url += $"busqueda={Uri.EscapeDataString(filter.Busqueda)}&";
            if (!string.IsNullOrEmpty(filter.NumeroPedido)) url += $"numeroPedido={Uri.EscapeDataString(filter.NumeroPedido)}&";
            if (filter.ClienteId.HasValue) url += $"clienteId={filter.ClienteId}&";
            if (!string.IsNullOrEmpty(filter.EstadoPedido)) url += $"estadoPedido={Uri.EscapeDataString(filter.EstadoPedido)}&";
            if (!string.IsNullOrEmpty(filter.Prioridad)) url += $"prioridad={Uri.EscapeDataString(filter.Prioridad)}&";
            if (filter.FechaDesde.HasValue) url += $"fechaDesde={filter.FechaDesde.Value:yyyy-MM-dd}&";
            if (filter.FechaHasta.HasValue) url += $"fechaHasta={filter.FechaHasta.Value:yyyy-MM-dd}&";
            url += $"top={filter.Top}&";
            if (filter.Page.HasValue) url += $"page={filter.Page}&";
            url += $"pagina={filter.Pagina}&";
            if (filter.PageSize.HasValue) url += $"pageSize={filter.PageSize}&";
            url += $"tamanoPagina={filter.TamanoPagina}&";
        }
        url = url.TrimEnd('&', '?');

        var response = await _http.GetAsync(url);
        return await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResultDto<PedidoVentaDto>>>()
               ?? ApiResponse<PaginatedResultDto<PedidoVentaDto>>.Fail("Error de comunicación.");
    }

    public async Task<PedidoVentaDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<PedidoVentaDto>>($"{BaseUrl}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<PedidoVentaDto>> CreateAsync(CreatePedidoVentaDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<PedidoVentaDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<PedidoVentaDto>>() ?? ApiResponse<PedidoVentaDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<PedidoVentaDto>> UpdateAsync(long id, UpdatePedidoVentaDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<PedidoVentaDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<PedidoVentaDto>>() ?? ApiResponse<PedidoVentaDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }

    /// <summary>
    /// Llama a POST /api/v1/ventas/pedidos/{id}/confirmar.
    /// Cambia el estado del pedido de Borrador a Confirmado y reserva stock.
    /// </summary>
    public async Task<ApiResponse<bool>> ConfirmarAsync(long id)
    {
        var response = await _http.PostAsync($"{BaseUrl}/{id}/confirmar", null);

        if (!response.IsSuccessStatusCode)
        {
            string body;
            try { body = await response.Content.ReadAsStringAsync(); }
            catch { body = string.Empty; }

            // Intentar leer mensaje estructurado del API
            try
            {
                var apiErr = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<bool>>(
                    body,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (apiErr is not null)
                    return apiErr;
            }
            catch { /* ignorar — retornar error crudo */ }

            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }

        // 200 OK: el controller devuelve DomainResult serializado directamente
        // Intentamos leer ApiResponse<bool> o construirlo desde el body
        try
        {
            var ok = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (ok is not null)
                return ok;
        }
        catch { /* ignorar */ }

        // Fallback: construir respuesta exitosa
        return ApiResponse<bool>.Ok(true, "Pedido confirmado correctamente.");
    }
}

