namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class UnidadMedidaHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/uoms";

    public UnidadMedidaHttpService(HttpClient http) => _http = http;

    public async Task<List<UomDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<UomDto>>>(BaseUrl);
        return response?.Data ?? new();
    }

    public async Task<UomDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<UomDto>>($"{BaseUrl}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<UomDto>> CreateAsync(CreateUomDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<UomDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<UomDto>>()
            ?? ApiResponse<UomDto>.Fail("Communication error.");
    }

    public async Task<ApiResponse<UomDto>> UpdateAsync(int id, UpdateUomDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<UomDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<UomDto>>()
            ?? ApiResponse<UomDto>.Fail("Communication error.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
        if (!response.IsSuccessStatusCode)
            return ApiResponse<bool>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Communication error.");
    }
}
