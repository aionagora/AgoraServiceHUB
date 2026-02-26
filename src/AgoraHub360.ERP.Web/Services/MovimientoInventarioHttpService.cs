namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Inventario;

public class MovimientoInventarioHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/inventario";

    public MovimientoInventarioHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<MovimientoInventarioDto>> GetMovimientosAsync(
        int? productoId = null,
        int? almacenId = null,
        string? tipo = null,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        var url = $"{Base}/movimientos?";
        if (productoId.HasValue) url += $"productoId={productoId}&";
        if (almacenId.HasValue) url += $"almacenId={almacenId}&";
        if (!string.IsNullOrEmpty(tipo)) url += $"tipo={Uri.EscapeDataString(tipo)}&";
        if (desde.HasValue) url += $"desde={desde.Value:yyyy-MM-dd}&";
        if (hasta.HasValue) url += $"hasta={hasta.Value:yyyy-MM-dd}&";

        var response = await _http.GetFromJsonAsync<ApiResponse<List<MovimientoInventarioDto>>>(url.TrimEnd('&', '?'));
        return response?.Data ?? new List<MovimientoInventarioDto>();
    }

    public async Task<ApiResponse<MovimientoInventarioDto>> CreateAsync(CreateMovimientoInventarioDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/movimientos", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<MovimientoInventarioDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<MovimientoInventarioDto>>()
            ?? ApiResponse<MovimientoInventarioDto>.Fail("Error de comunicación con el servidor.");
    }

    public async Task<KardexDto?> GetKardexAsync(
        int productoId,
        int? almacenId = null,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        var url = $"{Base}/kardex/{productoId}?";
        if (almacenId.HasValue) url += $"almacenId={almacenId}&";
        if (desde.HasValue) url += $"desde={desde.Value:yyyy-MM-dd}&";
        if (hasta.HasValue) url += $"hasta={hasta.Value:yyyy-MM-dd}&";

        var response = await _http.GetFromJsonAsync<ApiResponse<KardexDto>>(url.TrimEnd('&', '?'));
        return response?.Data;
    }

    public async Task<List<StockProductoDto>> GetStockAsync(int? almacenId = null)
    {
        var url = $"{Base}/stock";
        if (almacenId.HasValue) url += $"?almacenId={almacenId}";

        var response = await _http.GetFromJsonAsync<ApiResponse<List<StockProductoDto>>>(url);
        return response?.Data ?? new List<StockProductoDto>();
    }
}
