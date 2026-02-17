namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Empresa;

public class EmpresaHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/empresas";

    public EmpresaHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<EmpresaDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<EmpresaDto>>>(BaseUrl);
        return response?.Data ?? new List<EmpresaDto>();
    }

    /// <summary>
    /// Obtiene solo las empresas asignadas al usuario autenticado.
    /// </summary>
    public async Task<List<EmpresaDto>> GetMisEmpresasAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<EmpresaDto>>>($"{BaseUrl}/mis-empresas");
        return response?.Data ?? new List<EmpresaDto>();
    }

    public async Task<EmpresaDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<EmpresaDto>>($"{BaseUrl}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<EmpresaDto>> CreateAsync(CreateEmpresaDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<EmpresaDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<EmpresaDto>>()
            ?? ApiResponse<EmpresaDto>.Fail("Error de comunicacion con el servidor.");
    }

    public async Task<ApiResponse<EmpresaDto>> UpdateAsync(int id, UpdateEmpresaDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<EmpresaDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<EmpresaDto>>()
            ?? ApiResponse<EmpresaDto>.Fail("Error de comunicacion con el servidor.");
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
            ?? ApiResponse<bool>.Fail("Error de comunicacion con el servidor.");
    }
}
