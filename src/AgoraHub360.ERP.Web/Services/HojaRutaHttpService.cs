namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Logistica;

public class HojaRutaHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/logistica/hojas-ruta";

    public HojaRutaHttpService(HttpClient http) => _http = http;

    public async Task<List<HojaRutaDto>> GetAllAsync(
        string? estado = null, string? subEstado = null, string? tipoOP = null,
        DateTime? desde = null, DateTime? hasta = null, string? search = null)
    {
        var url = $"{Base}?";
        if (!string.IsNullOrEmpty(estado)) url += $"estado={Uri.EscapeDataString(estado)}&";
        if (!string.IsNullOrEmpty(subEstado)) url += $"subEstado={Uri.EscapeDataString(subEstado)}&";
        if (!string.IsNullOrEmpty(tipoOP)) url += $"tipoOP={Uri.EscapeDataString(tipoOP)}&";
        if (desde.HasValue) url += $"desde={desde.Value:yyyy-MM-dd}&";
        if (hasta.HasValue) url += $"hasta={hasta.Value:yyyy-MM-dd}&";
        if (!string.IsNullOrEmpty(search)) url += $"search={Uri.EscapeDataString(search)}&";

        var response = await _http.GetFromJsonAsync<ApiResponse<List<HojaRutaDto>>>(url.TrimEnd('&', '?'));
        return response?.Data ?? new List<HojaRutaDto>();
    }

    public async Task<HojaRutaDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<HojaRutaDto>>($"{Base}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<HojaRutaDto>> CreateAsync(CreateHojaRutaDto dto)
    {
        var r = await _http.PostAsJsonAsync(Base, dto);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<HojaRutaDto>.Fail($"Error HTTP {(int)r.StatusCode}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<HojaRutaDto>>()
            ?? ApiResponse<HojaRutaDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<HojaRutaDto>> UpdateAsync(long id, UpdateHojaRutaDto dto)
    {
        var r = await _http.PutAsJsonAsync($"{Base}/{id}", dto);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<HojaRutaDto>.Fail($"Error HTTP {(int)r.StatusCode}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<HojaRutaDto>>()
            ?? ApiResponse<HojaRutaDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<HojaRutaDto>> CambiarEstadoAsync(long id, CambiarEstadoHojaRutaDto dto)
    {
        var r = await _http.PostAsJsonAsync($"{Base}/{id}/cambiar-estado", dto);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<HojaRutaDto>.Fail($"Error HTTP {(int)r.StatusCode}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<HojaRutaDto>>()
            ?? ApiResponse<HojaRutaDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
    {
        var r = await _http.DeleteAsync($"{Base}/{id}");
        if (!r.IsSuccessStatusCode)
            return ApiResponse<bool>.Fail($"Error HTTP {(int)r.StatusCode}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }

    public async Task<List<HojaRutaDto>> GetByOrdenPedidoAsync(long ordenPedidoId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<HojaRutaDto>>>(
            $"{Base}/by-orden-pedido/{ordenPedidoId}");
        return response?.Data ?? new();
    }
}
