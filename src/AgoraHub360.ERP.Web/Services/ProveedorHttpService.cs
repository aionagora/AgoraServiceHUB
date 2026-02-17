namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ProveedorHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/proveedores";

    public ProveedorHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ProveedorDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ProveedorDto>>>(BaseUrl);
        return response?.Data ?? new();
    }

    public async Task<ApiResponse<ProveedorDto>> CreateAsync(CreateProveedorDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<ProveedorDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProveedorDto>>()
            ?? ApiResponse<ProveedorDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<ProveedorDto>> UpdateAsync(int id, UpdateProveedorDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<ProveedorDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProveedorDto>>()
            ?? ApiResponse<ProveedorDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }
}
