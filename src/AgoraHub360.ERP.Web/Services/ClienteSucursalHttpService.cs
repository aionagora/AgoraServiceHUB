namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ClienteSucursalHttpService
{
    private readonly HttpClient _http;

    public ClienteSucursalHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ClienteSucursalDto>> GetByClienteAsync(int clienteId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ClienteSucursalDto>>>($"api/v1/clientes/{clienteId}/sucursales");
        return response?.Data ?? new List<ClienteSucursalDto>();
    }

    public async Task<ClienteSucursalDto?> GetByIdAsync(int clienteId, int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<ClienteSucursalDto>>($"api/v1/clientes/{clienteId}/sucursales/{id}");
        return response?.Data;
    }
}
