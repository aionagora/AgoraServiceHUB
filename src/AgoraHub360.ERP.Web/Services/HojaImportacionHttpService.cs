namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Compras;

public class HojaImportacionHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/compras/importaciones";

    public HojaImportacionHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<HojaImportacionDto>> GetAllAsync(
        long? ordenCompraId = null,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        var url = $"{Base}?";
        if (ordenCompraId.HasValue) url += $"ordenCompraId={ordenCompraId}&";
        if (desde.HasValue) url += $"desde={desde.Value:yyyy-MM-dd}&";
        if (hasta.HasValue) url += $"hasta={hasta.Value:yyyy-MM-dd}&";

        var response = await _http.GetFromJsonAsync<ApiResponse<List<HojaImportacionDto>>>(url.TrimEnd('&', '?'));
        return response?.Data ?? new List<HojaImportacionDto>();
    }

    public async Task<HojaImportacionDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<HojaImportacionDto>>($"{Base}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<HojaImportacionDto>> CreateAsync(CreateHojaImportacionDto dto)
    {
        var response = await _http.PostAsJsonAsync(Base, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<HojaImportacionDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<HojaImportacionDto>>()
            ?? ApiResponse<HojaImportacionDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<HojaImportacionDto>> AddGastoAsync(long hojaId, AddGastoImportacionDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/{hojaId}/gastos", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<HojaImportacionDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<HojaImportacionDto>>()
            ?? ApiResponse<HojaImportacionDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<HojaImportacionDto>> RemoveGastoAsync(long hojaId, long gastoId)
    {
        var response = await _http.DeleteAsync($"{Base}/{hojaId}/gastos/{gastoId}");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<HojaImportacionDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<HojaImportacionDto>>()
            ?? ApiResponse<HojaImportacionDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<HojaImportacionDto>> LiquidarAsync(long hojaId)
    {
        var response = await _http.PostAsync($"{Base}/{hojaId}/liquidar", null);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<HojaImportacionDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<HojaImportacionDto>>()
            ?? ApiResponse<HojaImportacionDto>.Fail("Error de comunicación.");
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
