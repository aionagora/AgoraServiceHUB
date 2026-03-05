namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Compras;

public class OrdenPedidoHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/compras/pedidos";

    public OrdenPedidoHttpService(HttpClient http) => _http = http;

    public async Task<List<OrdenPedidoDto>> GetAllAsync(
        string? estado = null,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        var url = $"{Base}?";
        if (!string.IsNullOrEmpty(estado)) url += $"estado={Uri.EscapeDataString(estado)}&";
        if (desde.HasValue) url += $"desde={desde.Value:yyyy-MM-dd}&";
        if (hasta.HasValue) url += $"hasta={hasta.Value:yyyy-MM-dd}&";
        var response = await _http.GetFromJsonAsync<ApiResponse<List<OrdenPedidoDto>>>(url.TrimEnd('&', '?'));
        return response?.Data ?? new List<OrdenPedidoDto>();
    }

    public async Task<OrdenPedidoDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<OrdenPedidoDto>>($"{Base}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<OrdenPedidoDto>> CreateAsync(CreateOrdenPedidoDto dto)
    {
        var r = await _http.PostAsJsonAsync(Base, dto);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<OrdenPedidoDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<OrdenPedidoDto>>()
            ?? ApiResponse<OrdenPedidoDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<OrdenPedidoDto>> UpdateAsync(long id, UpdateOrdenPedidoDto dto)
    {
        var r = await _http.PutAsJsonAsync($"{Base}/{id}", dto);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<OrdenPedidoDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<OrdenPedidoDto>>()
            ?? ApiResponse<OrdenPedidoDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<OrdenPedidoDto>> EnviarRevisionAsync(long id)
    {
        var r = await _http.PostAsync($"{Base}/{id}/enviar-revision", null);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<OrdenPedidoDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<OrdenPedidoDto>>()
            ?? ApiResponse<OrdenPedidoDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<OrdenPedidoDto>> RevisarStockAsync(long id, RevisarStockOrdenPedidoDto dto)
    {
        var r = await _http.PostAsJsonAsync($"{Base}/{id}/revisar-stock", dto);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<OrdenPedidoDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<OrdenPedidoDto>>()
            ?? ApiResponse<OrdenPedidoDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<OrdenPedidoDto>> AprobarRechazarAsync(long id, AprobarRechazarOrdenPedidoDto dto)
    {
        var r = await _http.PostAsJsonAsync($"{Base}/{id}/aprobar", dto);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<OrdenPedidoDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<OrdenPedidoDto>>()
            ?? ApiResponse<OrdenPedidoDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<OrdenPedidoDto>> AnularAsync(long id, string? motivo = null)
    {
        var url = $"{Base}/{id}/anular";
        if (!string.IsNullOrEmpty(motivo)) url += $"?motivo={Uri.EscapeDataString(motivo)}";
        var r = await _http.PostAsync(url, null);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<OrdenPedidoDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<OrdenPedidoDto>>()
            ?? ApiResponse<OrdenPedidoDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
    {
        var r = await _http.DeleteAsync($"{Base}/{id}");
        if (!r.IsSuccessStatusCode)
            return ApiResponse<bool>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }
}
