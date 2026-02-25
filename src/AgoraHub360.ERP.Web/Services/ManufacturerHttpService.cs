namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ManufacturerHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/mdm/manufacturers";

    public ManufacturerHttpService(HttpClient http) => _http = http;

    public async Task<List<ManufacturerDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ManufacturerDto>>>(BaseUrl);
        return response?.Data ?? new();
    }

    public async Task<ManufacturerDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<ManufacturerDto>>($"{BaseUrl}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<ManufacturerDto>> CreateAsync(CreateManufacturerDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<ManufacturerDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<ManufacturerDto>>()
            ?? ApiResponse<ManufacturerDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<ManufacturerDto>> UpdateAsync(long id, UpdateManufacturerDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<ManufacturerDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<ManufacturerDto>>()
            ?? ApiResponse<ManufacturerDto>.Fail("Error de comunicación.");
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
