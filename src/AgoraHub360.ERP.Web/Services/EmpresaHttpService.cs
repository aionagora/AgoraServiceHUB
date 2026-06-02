namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Empresa;

public class EmpresaHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/empresas";

    public EmpresaHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<EmpresaDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<EmpresaDto>>>(BaseUrl);
        return response?.Data ?? new List<EmpresaDto>();
    }

    /// <summary>
    /// Obtiene solo las empresas asignadas al usuario autenticado.
    /// </summary>
    public async Task<List<EmpresaDto>> GetMisEmpresasAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<EmpresaDto>>>($"{BaseUrl}/mis-empresas");
        return response?.Data ?? new List<EmpresaDto>();
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
