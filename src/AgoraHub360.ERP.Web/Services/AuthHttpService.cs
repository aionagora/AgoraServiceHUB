namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Auth;

/// <summary>
/// Servicio HTTP para autenticación contra la API.
/// </summary>
public class AuthHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/auth";

    public AuthHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/login", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>()
            ?? ApiResponse<AuthResponseDto>.Fail("Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<CambiarEmpresaResponseDto>> SeleccionarEmpresaAsync(SeleccionarEmpresaRequestDto request)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/seleccionar-empresa", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CambiarEmpresaResponseDto>>()
            ?? ApiResponse<CambiarEmpresaResponseDto>.Fail("Error de comunicación con el servidor.");
    }
}
