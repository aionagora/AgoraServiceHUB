namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using System.Text.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.CxC;

public class CuentasPorCobrarHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/cuentas-por-cobrar";

    public CuentasPorCobrarHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<PaginatedResultDto<CuentaPorCobrarResumenDto>>> GetAllAsync(CuentaPorCobrarFilterDto? filter = null)
    {
        var url = $"{BaseUrl}?";
        if (filter != null)
        {
            if (filter.ClienteId.HasValue) url += $"clienteId={filter.ClienteId}&";
            if (!string.IsNullOrEmpty(filter.Estado)) url += $"estado={Uri.EscapeDataString(filter.Estado)}&";
            if (filter.FechaDesde.HasValue) url += $"fechaDesde={filter.FechaDesde.Value:yyyy-MM-dd}&";
            if (filter.FechaHasta.HasValue) url += $"fechaHasta={filter.FechaHasta.Value:yyyy-MM-dd}&";
            if (filter.FechaVencimientoDesde.HasValue) url += $"fechaVencimientoDesde={filter.FechaVencimientoDesde.Value:yyyy-MM-dd}&";
            if (filter.FechaVencimientoHasta.HasValue) url += $"fechaVencimientoHasta={filter.FechaVencimientoHasta.Value:yyyy-MM-dd}&";
            if (!string.IsNullOrEmpty(filter.NumeroFactura)) url += $"numeroFactura={Uri.EscapeDataString(filter.NumeroFactura)}&";
            if (!string.IsNullOrEmpty(filter.NumeroVenta)) url += $"numeroVenta={Uri.EscapeDataString(filter.NumeroVenta)}&";
            if (!string.IsNullOrEmpty(filter.Busqueda)) url += $"busqueda={Uri.EscapeDataString(filter.Busqueda)}&";
            url += $"top={filter.Top}&";
            url += $"pagina={filter.Pagina}&";
            url += $"tamanoPagina={filter.TamanoPagina}&";
            if (filter.Page.HasValue) url += $"page={filter.Page.Value}&";
            if (filter.PageSize.HasValue) url += $"pageSize={filter.PageSize.Value}&";
        }
        url = url.TrimEnd('&', '?');

        var response = await _http.GetAsync(url);
        return await ParseResult<PaginatedResultDto<CuentaPorCobrarResumenDto>>(response);
    }

    public async Task<ApiResponse<CuentaPorCobrarDetalleDto>> GetByIdAsync(long id)
    {
        var response = await _http.GetAsync($"{BaseUrl}/{id}");
        return await ParseResult<CuentaPorCobrarDetalleDto>(response);
    }

    public async Task<ApiResponse<decimal>> GetSaldoPendienteAsync()
    {
        var response = await _http.GetAsync($"{BaseUrl}/saldo-pendiente");
        return await ParseResult<decimal>(response);
    }

    public async Task<ApiResponse<AntiguedadSaldosResumenDto>> GetAntiguedadSaldosAsync()
    {
        var response = await _http.GetAsync($"{BaseUrl}/antiguedad-saldos");
        return await ParseResult<AntiguedadSaldosResumenDto>(response);
    }

    public async Task<ApiResponse<bool>> AnularAsync(long id, string? motivo = null)
    {
        var response = await _http.PutAsync($"{BaseUrl}/{id}/anular", null);
        return await ParseResult<bool>(response);
    }

    public async Task<ApiResponse<CuentaPorCobrarResumenDto>> ActualizarPorPagoAsync(long facturaVentaId, decimal montoPagado = 0m)
    {
        var requestDto = new ActualizarCxcPorPagoRequestDto { MontoPagado = montoPagado };
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/actualizar-por-pago/{facturaVentaId}", requestDto);
        return await ParseResult<CuentaPorCobrarResumenDto>(response);
    }

    private class ActualizarCxcPorPagoRequestDto
    {
        public decimal MontoPagado { get; set; }
    }

    private static async Task<ApiResponse<T>> ParseResult<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        try
        {
            var parsed = JsonSerializer.Deserialize<ResultPayload<T>>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed != null)
            {
                if (parsed.IsSuccess)
                {
                    return ApiResponse<T>.Ok(parsed.Value!, "Operación exitosa.");
                }
                else
                {
                    return ApiResponse<T>.Fail(parsed.Error ?? "Error en el servidor.");
                }
            }
        }
        catch
        {
            // Ignorar y caer al fallback
        }

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<T>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }

        return ApiResponse<T>.Fail("Error al procesar la respuesta del servidor.");
    }

    private class ResultPayload<T>
    {
        public bool IsSuccess { get; set; }
        public T? Value { get; set; }
        public string? Error { get; set; }
    }
}
