namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using System.Text.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class CierreContableHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/contabilidad/cierre-contable";

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public CierreContableHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<CierreContableDto?> ObtenerCierreAsync(int gestion)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CierreContableDto>>($"{Base}/{gestion}");
            return response?.Data;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<CierreContableDto>> EjecutarCierreAsync(EjecutarCierreDto dto)
    {
        var response = await _http.PostAsJsonAsync(Base, dto);
        return await ParseResponse<CierreContableDto>(response);
    }

    public async Task<ApiResponse<CierreContableDto>> EjecutarCierreAnualAsync(EjecutarCierreAnualDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/anual", dto);
        return await ParseResponse<CierreContableDto>(response);
    }

    public async Task<ApiResponse<bool>> EjecutarAperturaAsync(int gestionNueva)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/apertura/{gestionNueva}", new { });
        return await ParseResponse<bool>(response);
    }

    private static async Task<ApiResponse<T>> ParseResponse<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        try
        {
            var parsed = JsonSerializer.Deserialize<ApiResponse<T>>(body, JsonOpts);
            if (parsed is not null) return parsed;
        }
        catch { }

        return response.IsSuccessStatusCode
            ? ApiResponse<T>.Fail("Respuesta inesperada del servidor.")
            : ApiResponse<T>.Fail($"Error HTTP {(int)response.StatusCode}.");
    }
}
