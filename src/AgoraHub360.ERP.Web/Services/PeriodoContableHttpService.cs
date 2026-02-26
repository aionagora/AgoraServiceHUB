namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class PeriodoContableHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/contabilidad/periodos";

    public PeriodoContableHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<PeriodoContableDto>> GetAllAsync(int? anio = null)
    {
        var url = anio.HasValue ? $"{Base}?anio={anio}" : Base;
        var response = await _http.GetFromJsonAsync<ApiResponse<List<PeriodoContableDto>>>(url);
        return response?.Data ?? new();
    }

    public async Task<ApiResponse<int>> GenerarPeriodosAsync(int anio)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/generar", new GenerarPeriodosDto { Anio = anio });
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<int>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<int>>()
            ?? ApiResponse<int>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<PeriodoContableDto>> CerrarAsync(int id)
    {
        var response = await _http.PostAsync($"{Base}/{id}/cerrar", null);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<PeriodoContableDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<PeriodoContableDto>>()
            ?? ApiResponse<PeriodoContableDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<PeriodoContableDto>> ReabrirAsync(int id)
    {
        var response = await _http.PostAsync($"{Base}/{id}/reabrir", null);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<PeriodoContableDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<PeriodoContableDto>>()
            ?? ApiResponse<PeriodoContableDto>.Fail("Error de comunicación.");
    }
}
