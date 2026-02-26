namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class CategoriaProductoHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/categories";

    public CategoriaProductoHttpService(HttpClient http) => _http = http;

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<CategoryDto>>>(BaseUrl);
        return response?.Data ?? new();
    }

    public async Task<CategoryDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<CategoryDto>>($"{BaseUrl}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<CategoryDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>()
            ?? ApiResponse<CategoryDto>.Fail("Communication error.");
    }

    public async Task<ApiResponse<CategoryDto>> UpdateAsync(long id, UpdateCategoryDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<CategoryDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>()
            ?? ApiResponse<CategoryDto>.Fail("Communication error.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
        if (!response.IsSuccessStatusCode)
            return ApiResponse<bool>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Communication error.");
    }
}
