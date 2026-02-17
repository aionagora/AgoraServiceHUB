namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Parametro;

public class ParametroHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/parametros";

    public ParametroHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ParametroSistemaDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ParametroSistemaDto>>>(BaseUrl);
        return response?.Data ?? new();
    }

    public async Task<List<ParametroSistemaDto>> GetByCategoriaAsync(string categoria)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ParametroSistemaDto>>>(
            $"{BaseUrl}/categoria/{Uri.EscapeDataString(categoria)}");
        return response?.Data ?? new();
    }

    public async Task<ApiResponse<ParametroSistemaDto>> UpsertAsync(UpsertParametroDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ParametroSistemaDto>>()
            ?? ApiResponse<ParametroSistemaDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }
}
