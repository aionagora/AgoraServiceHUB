namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class PlantillaContableHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/contabilidad/plantillas";

    public PlantillaContableHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<PlantillaContableDto>> GetAllAsync(string? tipoDocumento = null)
    {
        var url = string.IsNullOrEmpty(tipoDocumento) ? Base : $"{Base}?tipoDocumento={Uri.EscapeDataString(tipoDocumento)}";
        var response = await _http.GetFromJsonAsync<ApiResponse<List<PlantillaContableDto>>>(url);
        return response?.Data ?? new();
    }

    public async Task<PlantillaContableDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<PlantillaContableDto>>($"{Base}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<PlantillaContableDto>> CreateAsync(CreatePlantillaContableDto dto)
    {
        var response = await _http.PostAsJsonAsync(Base, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<PlantillaContableDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<PlantillaContableDto>>()
            ?? ApiResponse<PlantillaContableDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<PlantillaContableDto>> UpdateAsync(int id, UpdatePlantillaContableDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{Base}/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<PlantillaContableDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<PlantillaContableDto>>()
            ?? ApiResponse<PlantillaContableDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"{Base}/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<int>> SeedPlantillasAsync()
    {
        var response = await _http.PostAsync($"{Base}/seed", null);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<int>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<int>>()
            ?? ApiResponse<int>.Fail("Error de comunicación.");
    }
}
