using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;

namespace AgoraHub360.ERP.Web.Services;

public class PedidoVentaHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/ventas/pedidos";

    public PedidoVentaHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<PedidoVentaDto>> GetAllAsync()
    {
        var response = await _http.GetAsync(BaseUrl);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Error HTTP {(int)response.StatusCode}: {body}");
        }

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<List<PedidoVentaDto>>>();
        if (payload is null)
            throw new InvalidOperationException("Respuesta vacía del endpoint de pedidos.");

        if (!payload.Success)
            throw new InvalidOperationException(payload.Message ?? "La API devolvió un error al listar pedidos.");

        return payload.Data ?? new List<PedidoVentaDto>();
    }

    public async Task<PedidoVentaDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<PedidoVentaDto>>($"{BaseUrl}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<PedidoVentaDto>> CreateAsync(CreatePedidoVentaDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<PedidoVentaDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<PedidoVentaDto>>() ?? ApiResponse<PedidoVentaDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<PedidoVentaDto>> UpdateAsync(long id, UpdatePedidoVentaDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<PedidoVentaDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<PedidoVentaDto>>() ?? ApiResponse<PedidoVentaDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }
}
