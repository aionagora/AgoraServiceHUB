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

    public async Task<List<SucursalListadoDto>> GetAllAsync()
    {
        var response = await _http.GetAsync(BaseUrl);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"HTTP {(int)response.StatusCode}: {error}");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<SucursalListadoDto>>>();
        return result?.Data ?? new List<SucursalListadoDto>();
    }

    public async Task<List<SucursalListadoDto>> GetActivasAsync()
    {
        var todas = await GetAllAsync();
        return todas.Where(s => s.Activo).ToList();
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
            try
            {
                var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<SucursalDto>>();
                if (errorResult != null && !string.IsNullOrEmpty(errorResult.Message)) 
                    return errorResult;
            }
            catch { }
            return ApiResponse<SucursalDto>.Fail(FormatearErrorValidacion("Error al crear sucursal", (int)response.StatusCode, body));
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<SucursalDto>>()
            ?? ApiResponse<SucursalDto>.Fail("Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<SucursalDto>> UpdateAsync(int id, ActualizarSucursalDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            try
            {
                var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<SucursalDto>>();
                if (errorResult != null && !string.IsNullOrEmpty(errorResult.Message)) 
                    return errorResult;
            }
            catch { }
            return ApiResponse<SucursalDto>.Fail(FormatearErrorValidacion("Error al actualizar sucursal", (int)response.StatusCode, body));
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<SucursalDto>>()
            ?? ApiResponse<SucursalDto>.Fail("Error de comunicación con el servidor.");
    }

    private string FormatearErrorValidacion(string defaultMsg, int statusCode, string body)
    {
        if (statusCode == 400 && body.Contains("\"errors\":"))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("title", out var titleProp) &&
                    doc.RootElement.TryGetProperty("errors", out var errorsProp))
                {
                    var msg = $"{titleProp.GetString()}: ";
                    foreach (var err in errorsProp.EnumerateObject())
                    {
                        var values = err.Value.EnumerateArray().Select(x => x.GetString());
                        msg += string.Join(", ", values) + " ";
                    }
                    return msg.Trim();
                }
            }
            catch { }
        }
        return $"HTTP {statusCode}: {body}";
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
