namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using System.Text.Json;
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

    public async Task<AuthMeDto?> GetMeAsync()
    {
        try
        {
            using var httpResponse = await _http.GetAsync($"{BaseUrl}/me");
            if (!httpResponse.IsSuccessStatusCode)
                return null;

            var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>();
            if (response is null || !response.Success || response.Data.ValueKind == JsonValueKind.Undefined)
                return null;

            var data = response.Data;

            static string? ReadString(JsonElement root, string propertyName)
            {
                if (!root.TryGetProperty(propertyName, out var prop))
                    return null;

                return prop.ValueKind == JsonValueKind.String
                    ? prop.GetString()
                    : prop.ToString();
            }

            var tenantIdRaw = ReadString(data, nameof(AuthMeDto.TenantId));
            int? tenantId = int.TryParse(tenantIdRaw, out var parsedTenantId) ? parsedTenantId : null;

            return new AuthMeDto
            {
                UserId = ReadString(data, nameof(AuthMeDto.UserId)),
                UserName = ReadString(data, nameof(AuthMeDto.UserName)),
                Email = ReadString(data, nameof(AuthMeDto.Email)),
                PlatformRole = ReadString(data, nameof(AuthMeDto.PlatformRole)),
                TenantRole = ReadString(data, nameof(AuthMeDto.TenantRole)),
                TenantStatus = ReadString(data, nameof(AuthMeDto.TenantStatus)),
                TenantId = tenantId,
            };
        }
        catch
        {
            return null;
        }
    }
}

public sealed class AuthMeDto
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PlatformRole { get; set; }
    public string? TenantRole { get; set; }
    public int? TenantId { get; set; }
    public string? TenantStatus { get; set; }
}
