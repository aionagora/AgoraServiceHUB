namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class AsientoContableHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/contabilidad/asientos";

    public AsientoContableHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<AsientoContableDto>> GetAllAsync(
        DateTime? desde = null, DateTime? hasta = null,
        string? estado = null, string? origenTipo = null, string? search = null)
    {
        var url = $"{Base}?";
        if (desde.HasValue) url += $"desde={desde.Value:yyyy-MM-dd}&";
        if (hasta.HasValue) url += $"hasta={hasta.Value:yyyy-MM-dd}&";
        if (!string.IsNullOrEmpty(estado)) url += $"estado={Uri.EscapeDataString(estado)}&";
        if (!string.IsNullOrEmpty(origenTipo)) url += $"origenTipo={Uri.EscapeDataString(origenTipo)}&";
        if (!string.IsNullOrEmpty(search)) url += $"search={Uri.EscapeDataString(search)}&";
        var response = await _http.GetFromJsonAsync<ApiResponse<List<AsientoContableDto>>>(url.TrimEnd('&', '?'));
        return response?.Data ?? new();
    }

    public async Task<AsientoContableDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<AsientoContableDto>>($"{Base}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<AsientoContableDto>> CreateAsync(CreateAsientoContableDto dto)
    {
        var response = await _http.PostAsJsonAsync(Base, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<AsientoContableDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<AsientoContableDto>>()
            ?? ApiResponse<AsientoContableDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<AsientoContableDto>> ContabilizarAsync(long id)
    {
        var response = await _http.PostAsync($"{Base}/{id}/contabilizar", null);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<AsientoContableDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<AsientoContableDto>>()
            ?? ApiResponse<AsientoContableDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<AsientoContableDto>> AnularAsync(long id)
    {
        var response = await _http.PostAsync($"{Base}/{id}/anular", null);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<AsientoContableDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<AsientoContableDto>>()
            ?? ApiResponse<AsientoContableDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
    {
        var response = await _http.DeleteAsync($"{Base}/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }
}
