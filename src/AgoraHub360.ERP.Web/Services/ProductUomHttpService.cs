namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ProductUomHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/mdm/product-uoms";

    public ProductUomHttpService(HttpClient http) => _http = http;

    public async Task<List<ProductUomDto>> GetByProductAsync(long productId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ProductUomDto>>>(
            $"{BaseUrl}/by-product/{productId}");
        return response?.Data ?? new();
    }

    public async Task<ApiResponse<ProductUomDto>> CreateAsync(CreateProductUomDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<ProductUomDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductUomDto>>()
            ?? ApiResponse<ProductUomDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<ProductUomDto>> UpdateAsync(long id, UpdateProductUomDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<ProductUomDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductUomDto>>()
            ?? ApiResponse<ProductUomDto>.Fail("Error de comunicación.");
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
