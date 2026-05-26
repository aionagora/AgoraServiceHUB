namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using System.Text.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;

public class VentaHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/ventas";

    public VentaHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<VentaResumenDto>> GetAllAsync()
    {
        var response = await _http.GetAsync(BaseUrl);
        var parsed = await ParseResponse<IReadOnlyList<VentaResumenDto>>(response);

        if (!parsed.Success)
            throw new InvalidOperationException(parsed.Message ?? "Error al listar ventas.");

        return parsed.Data?.ToList() ?? new List<VentaResumenDto>();
    }

    public async Task<VentaDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetAsync($"{BaseUrl}/{id}");
        var parsed = await ParseResponse<VentaDto>(response);

        if (!parsed.Success)
            return null;

        return parsed.Data;
    }

    public async Task<ApiResponse<VentaDto>> CreateAsync(CrearVentaRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        return await ParseResponse<VentaDto>(response);
    }

    public async Task<ApiResponse<VentaDto>> UpdateAsync(long id, ActualizarVentaRequestDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        return await ParseResponse<VentaDto>(response);
    }

    public async Task<ApiResponse<VentaDto>> ConfirmarAsync(long id, ConfirmarVentaRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/{id}/confirmar", dto);
        return await ParseResponse<VentaDto>(response);
    }

    public async Task<ApiResponse<VentaDto>> CrearDesdePedidoAsync(GenerarVentaDesdePedidoRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/desde-pedido", dto);
        return await ParseResponse<VentaDto>(response);
    }

    public async Task<ApiResponse<VentaDto>> RegistrarPagoAsync(long id, RegistrarPagoVentaRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/{id}/pagos", dto);
        return await ParseResponse<VentaDto>(response);
    }

    public async Task<ApiResponse<VentaDto>> AnularAsync(long id, AnularVentaRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/{id}/anular", dto);
        return await ParseResponse<VentaDto>(response);
    }

    private static async Task<ApiResponse<T>> ParseResponse<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            try
            {
                var parsedResponse = JsonSerializer.Deserialize<ApiResponse<T>>(
                    body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (parsedResponse != null && !string.IsNullOrWhiteSpace(parsedResponse.Message))
                    return parsedResponse;
            }
            catch
            {
                // Ignorar errores de parseo y devolver mensaje HTTP crudo.
            }

            return ApiResponse<T>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }

        var ok = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
        return ok ?? ApiResponse<T>.Fail("Error de comunicación con el servidor.");
    }
}
