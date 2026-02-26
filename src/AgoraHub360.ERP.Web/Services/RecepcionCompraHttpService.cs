namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Compras;

public class RecepcionCompraHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/compras/recepciones";

    public RecepcionCompraHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<RecepcionCompraDto>> GetAllAsync(
        long? ordenCompraId = null,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        var url = $"{Base}?";
        if (ordenCompraId.HasValue) url += $"ordenCompraId={ordenCompraId}&";
        if (desde.HasValue) url += $"desde={desde.Value:yyyy-MM-dd}&";
        if (hasta.HasValue) url += $"hasta={hasta.Value:yyyy-MM-dd}&";

        var response = await _http.GetFromJsonAsync<ApiResponse<List<RecepcionCompraDto>>>(url.TrimEnd('&', '?'));
        return response?.Data ?? new List<RecepcionCompraDto>();
    }

    public async Task<RecepcionCompraDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<RecepcionCompraDto>>($"{Base}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<RecepcionCompraDto>> CreateAsync(CreateRecepcionCompraDto dto)
    {
        var response = await _http.PostAsJsonAsync(Base, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<RecepcionCompraDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<RecepcionCompraDto>>()
            ?? ApiResponse<RecepcionCompraDto>.Fail("Error de comunicación.");
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
