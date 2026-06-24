namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ClientePerfilFiscalHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/clientes";

    public ClientePerfilFiscalHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ClientePerfilFiscalDto>> GetByClienteAsync(int clienteId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ClientePerfilFiscalDto>>>($"{BaseUrl}/{clienteId}/perfiles-fiscales");
        return response?.Data ?? new List<ClientePerfilFiscalDto>();
    }

    public async Task<ClientePerfilFiscalDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<ClientePerfilFiscalDto>>($"{BaseUrl}/perfiles-fiscales/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<ClientePerfilFiscalDto>> CreateAsync(int clienteId, CreateClientePerfilFiscalDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/{clienteId}/perfiles-fiscales", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<ClientePerfilFiscalDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<ApiResponse<ClientePerfilFiscalDto>>()
            ?? ApiResponse<ClientePerfilFiscalDto>.Fail("Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<ClientePerfilFiscalDto>> UpdateAsync(long id, UpdateClientePerfilFiscalDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/perfiles-fiscales/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<ClientePerfilFiscalDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<ApiResponse<ClientePerfilFiscalDto>>()
            ?? ApiResponse<ClientePerfilFiscalDto>.Fail("Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/perfiles-fiscales/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<bool>> SetPredeterminadoAsync(long id, SetClientePerfilFiscalPredeterminadoDto? dto = null)
    {
        var payload = dto ?? new SetClientePerfilFiscalPredeterminadoDto();
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/perfiles-fiscales/{id}/predeterminado", payload);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación con el servidor.");
    }
}
