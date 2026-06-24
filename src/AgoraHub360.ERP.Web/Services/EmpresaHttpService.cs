namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Empresa;
using Microsoft.JSInterop;

public class EmpresaHttpService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private const string BaseUrl = "api/v1/empresas";

    public EmpresaHttpService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    private async Task LogRequestDiagnostic(string endpoint)
    {
        try
        {
            var token = await _js.InvokeAsync<string?>("localStorage.getItem", "agorahub360_auth_token");
            var hasToken = !string.IsNullOrWhiteSpace(token);
            var authHeader = _http.DefaultRequestHeaders.Authorization;
            var hasHeader = authHeader is not null;
            var tokenLenOrStart = "N/A";
            if (hasToken && token is not null)
            {
                tokenLenOrStart = token.Length > 20 ? token.Substring(0, 20) + "..." : $"length={token.Length}";
            }
            Console.WriteLine($"[DIAGNOSTIC-LOG] Endpoint: {endpoint} | localStorageToken: {hasToken} | AuthHeaderPresent: {hasHeader} | TokenStart: {tokenLenOrStart}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DIAGNOSTIC-LOG] Error during diagnostic logging: {ex.Message}");
        }
    }

    public async Task<List<EmpresaDto>> GetAllAsync()
    {
        await LogRequestDiagnostic(BaseUrl);
        using var response = await _http.GetAsync(BaseUrl);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Endpoint: GET {BaseUrl} | Status: {(int)response.StatusCode} {response.ReasonPhrase} | Response Body: {body}", null, response.StatusCode);
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<EmpresaDto>>>();
        return result?.Data ?? new List<EmpresaDto>();
    }

    /// <summary>
    /// Obtiene solo las empresas asignadas al usuario autenticado.
    /// </summary>
    public async Task<List<EmpresaDto>> GetMisEmpresasAsync()
    {
        var url = $"{BaseUrl}/mis-empresas";
        await LogRequestDiagnostic(url);
        using var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Endpoint: GET {url} | Status: {(int)response.StatusCode} {response.ReasonPhrase} | Response Body: {body}", null, response.StatusCode);
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<EmpresaDto>>>();
        return result?.Data ?? new List<EmpresaDto>();
    }

    public async Task<EmpresaDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<EmpresaDto>>($"{BaseUrl}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<EmpresaDto>> CreateAsync(CreateEmpresaDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        return await ReadApiResponseAsync<EmpresaDto>(response, "Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<EmpresaDto>> UpdateAsync(int id, UpdateEmpresaDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        return await ReadApiResponseAsync<EmpresaDto>(response, "Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
        return await ReadApiResponseAsync<bool>(response, "Error de comunicación con el servidor.");
    }

    /// <summary>
    /// Seeds default MDM data (catalog, UoMs, statuses) for the current user's company.
    /// Idempotent — safe to call multiple times.
    /// </summary>
    public async Task<ApiResponse<bool>> SeedMyCompanyAsync()
    {
        var response = await _http.PostAsync($"{BaseUrl}/mi-empresa/seed", null);
        return await ReadApiResponseAsync<bool>(response, "Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<ConfiguracionInicialEmpresaResultadoDto>> GenerarConfiguracionBasicaAsync(long empresaId)
    {
        var response = await _http.PostAsync($"{BaseUrl}/{empresaId}/generar-configuracion-basica", null);
        return await ReadApiResponseAsync<ConfiguracionInicialEmpresaResultadoDto>(response, "Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<CrearEmpresaDemoCompletaResponseDto>> CrearDemoCompletaAsync(CrearEmpresaDemoCompletaRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/demo/crear-completa", dto);
        return await ReadApiResponseAsync<CrearEmpresaDemoCompletaResponseDto>(response, "Error de comunicación con el servidor.");
    }

    private static async Task<ApiResponse<T>> ReadApiResponseAsync<T>(HttpResponseMessage response, string fallback)
    {
        var body = await response.Content.ReadAsStringAsync();

        try
        {
            var parsed = string.IsNullOrWhiteSpace(body)
                ? null
                : System.Text.Json.JsonSerializer.Deserialize<ApiResponse<T>>(body, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (parsed is not null)
            {
                if (!response.IsSuccessStatusCode && string.IsNullOrWhiteSpace(parsed.Message))
                {
                    parsed.Message = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
                }

                return parsed;
            }
        }
        catch
        {
            // Ignorar parseo y devolver diagnóstico bruto.
        }

        var detail = string.IsNullOrWhiteSpace(body)
            ? fallback
            : body;

        return ApiResponse<T>.Fail(
            $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}. Detalle: {detail}",
            new List<string> { detail });
    }
}
