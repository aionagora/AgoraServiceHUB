namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ClienteSucursalHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/clientes";

    public ClienteSucursalHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ClienteSucursalDto>> GetByClienteAsync(int clienteId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ClienteSucursalDto>>>($"{BaseUrl}/{clienteId}/sucursales");
        return response?.Data ?? new List<ClienteSucursalDto>();
    }

    public async Task<ClienteSucursalDto?> GetByIdAsync(int clienteId, int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<ClienteSucursalDto>>($"{BaseUrl}/{clienteId}/sucursales/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<ClienteSucursalDto>> CreateAsync(int clienteId, CreateClienteSucursalDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/{clienteId}/sucursales", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<ClienteSucursalDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<ApiResponse<ClienteSucursalDto>>()
            ?? ApiResponse<ClienteSucursalDto>.Fail("Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<ClienteSucursalDto>> UpdateAsync(int clienteId, int id, UpdateClienteSucursalDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{clienteId}/sucursales/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<ClienteSucursalDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<ApiResponse<ClienteSucursalDto>>()
            ?? ApiResponse<ClienteSucursalDto>.Fail("Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int clienteId, int id)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{clienteId}/sucursales/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<bool>> SetPrincipalAsync(int clienteId, int id)
    {
        var response = await _http.PatchAsync($"{BaseUrl}/{clienteId}/sucursales/{id}/principal", null);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación con el servidor.");
    }
}
