namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class CentroCostoHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/contabilidad/centros-costo";

    public CentroCostoHttpService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>Obtiene todos los centros de costo activos de la empresa.</summary>
    public async Task<List<CentroCostoDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<CentroCostoDto>>>(Base);
        return response?.Data ?? new();
    }

    public async Task<ApiResponse<CentroCostoDto>> CreateAsync(CreateCentroCostoDto dto)
    {
        var response = await _http.PostAsJsonAsync(Base, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<CentroCostoDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<CentroCostoDto>>()
            ?? ApiResponse<CentroCostoDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<CentroCostoDto>> UpdateAsync(int id, UpdateCentroCostoDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{Base}/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<CentroCostoDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<CentroCostoDto>>()
            ?? ApiResponse<CentroCostoDto>.Fail("Error de comunicación.");
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
}
