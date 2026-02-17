namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class UnidadMedidaHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/unidades-medida";

    public UnidadMedidaHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<UnidadMedidaDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<UnidadMedidaDto>>>(BaseUrl);
        return response?.Data ?? new();
    }

    public async Task<ApiResponse<UnidadMedidaDto>> CreateAsync(CreateUnidadMedidaDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<UnidadMedidaDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<UnidadMedidaDto>>()
            ?? ApiResponse<UnidadMedidaDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<UnidadMedidaDto>> UpdateAsync(int id, UpdateUnidadMedidaDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<UnidadMedidaDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<UnidadMedidaDto>>()
            ?? ApiResponse<UnidadMedidaDto>.Fail("Error de comunicación.");
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
