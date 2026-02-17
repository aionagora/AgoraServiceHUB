namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Rol;

public class RolHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/roles";

    public RolHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<RolDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<RolDto>>>(BaseUrl);
        return response?.Data ?? new List<RolDto>();
    }

    public async Task<ApiResponse<RolDto>> CreateAsync(CreateRolDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        return await response.Content.ReadFromJsonAsync<ApiResponse<RolDto>>()
            ?? ApiResponse<RolDto>.Fail("Error de comunicacion con el servidor.");
    }

    public async Task<ApiResponse<RolDto>> UpdateAsync(int id, UpdateRolDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        return await response.Content.ReadFromJsonAsync<ApiResponse<RolDto>>()
            ?? ApiResponse<RolDto>.Fail("Error de comunicacion con el servidor.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicacion con el servidor.");
    }
}
