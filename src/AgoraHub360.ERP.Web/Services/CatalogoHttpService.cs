namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class CatalogoHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/mdm/catalogs";

    public CatalogoHttpService(HttpClient http) => _http = http;

    public async Task<List<CatalogDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<CatalogDto>>>(BaseUrl);
        return response?.Data ?? new();
    }

    public async Task<CatalogDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<CatalogDto>>($"{BaseUrl}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<CatalogDto>> CreateAsync(CreateCatalogDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<CatalogDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<CatalogDto>>()
            ?? ApiResponse<CatalogDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<CatalogDto>> UpdateAsync(long id, UpdateCatalogDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<CatalogDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<CatalogDto>>()
            ?? ApiResponse<CatalogDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
        if (!response.IsSuccessStatusCode)
            return ApiResponse<bool>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }
}
