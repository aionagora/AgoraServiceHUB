namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class CierreContableHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/contabilidad/cierre-contable";

    public CierreContableHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<CierreContableDto?> ObtenerCierreAsync(int gestion)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<CierreContableDto>>($"{Base}/{gestion}");
        return response?.Data;
    }

    public async Task<ApiResponse<CierreContableDto>> EjecutarCierreAsync(EjecutarCierreDto dto)
    {
        var response = await _http.PostAsJsonAsync(Base, dto);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CierreContableDto>>() ?? new ApiResponse<CierreContableDto>();
    }

    public async Task<ApiResponse<bool>> EjecutarAperturaAsync(int gestionNueva)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/apertura/{gestionNueva}", new { });
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() ?? new ApiResponse<bool>();
    }
}
