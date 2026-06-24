namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.CxC;

public class ClienteCreditoConfiguracionHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/clientes";

    public ClienteCreditoConfiguracionHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ClienteCreditoConfiguracionDto?> GetByClienteAsync(int clienteId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<ClienteCreditoConfiguracionDto>>($"{BaseUrl}/{clienteId}/credito-configuracion");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener la configuración de crédito: {ex.Message}");
            return null;
        }
    }

    public async Task<ApiResponse<ClienteCreditoConfiguracionDto>> GuardarAsync(
        int clienteId,
        GuardarClienteCreditoConfiguracionRequestDto dto)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"{BaseUrl}/{clienteId}/credito-configuracion", dto);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return ApiResponse<ClienteCreditoConfiguracionDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
            }
            return await response.Content.ReadFromJsonAsync<ApiResponse<ClienteCreditoConfiguracionDto>>()
                ?? ApiResponse<ClienteCreditoConfiguracionDto>.Fail("Error de comunicación con el servidor.");
        }
        catch (Exception ex)
        {
            return ApiResponse<ClienteCreditoConfiguracionDto>.Fail($"Excepción: {ex.Message}");
        }
    }
}
