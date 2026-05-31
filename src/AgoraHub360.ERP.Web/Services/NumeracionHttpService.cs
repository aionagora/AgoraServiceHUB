namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Numeracion;

public class NumeracionHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/numeraciones-documento";

    public NumeracionHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<NumeracionDocumentoDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<NumeracionDocumentoDto>>>(BaseUrl);
        return response?.Data ?? new();
    }

    public async Task<ApiResponse<NumeracionDocumentoDto>> CreateAsync(CrearNumeracionDocumentoRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<NumeracionDocumentoDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<NumeracionDocumentoDto>>()
            ?? ApiResponse<NumeracionDocumentoDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<NumeracionDocumentoDto>> UpdateAsync(int id, ActualizarNumeracionDocumentoRequestDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<NumeracionDocumentoDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<NumeracionDocumentoDto>>()
            ?? ApiResponse<NumeracionDocumentoDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> ActivarAsync(int id)
    {
        var response = await _http.PatchAsync($"{BaseUrl}/{id}/activar", null);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> DesactivarAsync(int id)
    {
        var response = await _http.PatchAsync($"{BaseUrl}/{id}/desactivar", null);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }
}
