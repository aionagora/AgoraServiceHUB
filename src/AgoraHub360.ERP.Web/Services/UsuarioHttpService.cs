namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Usuario;

public class UsuarioHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/usuarios";

    public UsuarioHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<UsuarioDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<UsuarioDto>>>(BaseUrl);
        return response?.Data ?? new List<UsuarioDto>();
    }

    public async Task<UsuarioDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<UsuarioDto>>($"{BaseUrl}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<UsuarioDto>> CreateAsync(CreateUsuarioDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<UsuarioDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<UsuarioDto>>()
            ?? ApiResponse<UsuarioDto>.Fail("Error de comunicacion con el servidor.");
    }

    public async Task<ApiResponse<UsuarioDto>> UpdateAsync(int id, UpdateUsuarioDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<UsuarioDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<UsuarioDto>>()
            ?? ApiResponse<UsuarioDto>.Fail("Error de comunicacion con el servidor.");
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

    public async Task<ApiResponse<bool>> AsignarRolAsync(int usuarioId, AsignarRolDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/{usuarioId}/roles", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicacion con el servidor.");
    }

    public async Task<ApiResponse<bool>> RemoverDeEmpresaAsync(int usuarioId, int empresaId)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{usuarioId}/empresas/{empresaId}");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicacion con el servidor.");
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(int usuarioId, ResetPasswordDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{usuarioId}/reset-password", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicacion con el servidor.");
    }
}
