namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class BrandHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/mdm/brands";

    public BrandHttpService(HttpClient http) => _http = http;

    public async Task<List<BrandDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<BrandDto>>>(BaseUrl);
        return response?.Data ?? new();
    }

    public async Task<BrandDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<BrandDto>>($"{BaseUrl}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<BrandDto>> CreateAsync(CreateBrandDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<BrandDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<BrandDto>>()
            ?? ApiResponse<BrandDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<BrandDto>> UpdateAsync(long id, UpdateBrandDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<BrandDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<BrandDto>>()
            ?? ApiResponse<BrandDto>.Fail("Error de comunicación.");
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
