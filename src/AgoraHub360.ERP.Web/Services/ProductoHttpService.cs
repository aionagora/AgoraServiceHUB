namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ProductoHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/products";

    public ProductoHttpService(HttpClient http) => _http = http;

    public async Task<List<ProductDto2>> GetAllAsync(long? catalogId = null)
    {
        var url = catalogId.HasValue ? $"{BaseUrl}?catalogId={catalogId}" : BaseUrl;
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ProductDto2>>>(url);
        return response?.Data ?? new();
    }

    public async Task<ProductDto2?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<ProductDto2>>($"{BaseUrl}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<ProductDto2>> CreateAsync(CreateProductDto2 dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<ProductDto2>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto2>>()
            ?? ApiResponse<ProductDto2>.Fail("Communication error.");
    }

    public async Task<ApiResponse<ProductDto2>> UpdateAsync(long id, UpdateProductDto2 dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<ProductDto2>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto2>>()
            ?? ApiResponse<ProductDto2>.Fail("Communication error.");
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
