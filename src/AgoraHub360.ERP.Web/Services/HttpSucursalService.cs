namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Sucursal;

public class HttpSucursalService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/sucursales";

    public HttpSucursalService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<SucursalListadoDto>> GetAllByEmpresaAsync(int empresaId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<SucursalListadoDto>>>($"{BaseUrl}/empresa/{empresaId}");
        return response?.Data ?? new List<SucursalListadoDto>();
    }

    public async Task<SucursalDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<SucursalDto>>($"{BaseUrl}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<SucursalDto>> CreateAsync(CrearSucursalDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<SucursalDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<SucursalDto>>()
            ?? ApiResponse<SucursalDto>.Fail("Error de comunicacion con el servidor.");
    }

    public async Task<ApiResponse<SucursalDto>> UpdateAsync(int id, ActualizarSucursalDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<SucursalDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<SucursalDto>>()
            ?? ApiResponse<SucursalDto>.Fail("Error de comunicacion con el servidor.");
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

    public async Task<ApiResponse<bool>> PatchEstadoAsync(int id, bool activo)
    {
        var response = await _http.PatchAsJsonAsync($"{BaseUrl}/{id}/estado", activo);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicacion con el servidor.");
    }

    public async Task<ApiResponse<bool>> PatchPrincipalAsync(int id)
    {
        var response = await _http.PatchAsync($"{BaseUrl}/{id}/principal", null);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicacion con el servidor.");
    }
}
