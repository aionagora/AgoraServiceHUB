namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using AgoraHub360.ERP.Shared.Constants;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

/// <summary>
/// AuthenticationStateProvider basado en JWT almacenado en localStorage.
/// Parsea los claims del token para determinar el estado de autenticación.
/// </summary>
public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private readonly HttpClient _http;
    private const string TokenKey = "agorahub360_auth_token";

    public JwtAuthStateProvider(IJSRuntime js, HttpClient http)
    {
        _js = js;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        // Verificar si el token expiró
        var claims = ParseClaimsFromJwt(token);
        var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
        if (expClaim is not null && long.TryParse(expClaim.Value, out var exp))
        {
            var expDate = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
            if (expDate < DateTime.UtcNow)
            {
                await RemoveTokenAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task LoginAsync(string token)
    {
        await SetTokenAsync(token);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task ReplaceTokenAsync(string token)
    {
        await SetTokenAsync(token);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task LogoutAsync()
    {
        await RemoveTokenAsync();
        _http.DefaultRequestHeaders.Authorization = null;

        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }

    public async Task<JwtSessionContext> GetSessionContextAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            _http.DefaultRequestHeaders.Authorization = null;
            return JwtSessionContext.Empty;
        }

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var claims = ParseClaimsFromJwt(token).ToList();

        string? GetClaim(string type)
            => claims.FirstOrDefault(c => c.Type == type)?.Value;

        var tenantIdRaw = GetClaim(ClaimTypesCustom.TenantId) ?? GetClaim(ClaimTypesCustom.EmpresaId);
        int? tenantId = int.TryParse(tenantIdRaw, out var parsedTenant) ? parsedTenant : null;

        var platformRole = GetClaim(ClaimTypesCustom.PlatformRole) ?? Roles.None;
        var tenantRole = GetClaim(ClaimTypesCustom.TenantRole)
                         ?? GetClaim(ClaimTypes.Role)
                         ?? Roles.NoAccess;

        var tenantStatus = GetClaim(ClaimTypesCustom.TenantStatus)
                           ?? (tenantId.HasValue
                               ? TenantStatus.Selected
                               : TenantStatus.NotSelected);

        return new JwtSessionContext(
            IsAuthenticated: true,
            UserId: GetClaim(ClaimTypes.NameIdentifier),
            UserName: GetClaim(ClaimTypes.Name),
            Email: GetClaim(ClaimTypes.Email),
            PlatformRole: platformRole,
            TenantRole: tenantRole,
            TenantId: tenantId,
            TenantStatus: tenantStatus);
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        }
        catch
        {
            return null; // SSR/prerender
        }
    }

    private async Task SetTokenAsync(string token)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
    }

    private async Task RemoveTokenAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];

        // Base64 URL decode
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var kvPairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

        if (kvPairs is null) return claims;

        foreach (var kvp in kvPairs)
        {
            var claimType = kvp.Key switch
            {
                "sub" or "nameid" => ClaimTypes.NameIdentifier,
                "unique_name" => ClaimTypes.Name,
                "email" => ClaimTypes.Email,
                "role" => ClaimTypes.Role,
                _ => kvp.Key
            };

            if (kvp.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in kvp.Value.EnumerateArray())
                    claims.Add(new Claim(claimType, element.GetString() ?? ""));
            }
            else
            {
                claims.Add(new Claim(claimType, kvp.Value.ToString()));
            }
        }

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}

public sealed record JwtSessionContext(
    bool IsAuthenticated,
    string? UserId,
    string? UserName,
    string? Email,
    string PlatformRole,
    string TenantRole,
    int? TenantId,
    string TenantStatus)
{
    public static JwtSessionContext Empty { get; } = new(
        IsAuthenticated: false,
        UserId: null,
        UserName: null,
        Email: null,
        PlatformRole: Roles.None,
        TenantRole: Roles.NoAccess,
        TenantId: null,
        TenantStatus: AgoraHub360.ERP.Shared.Constants.TenantStatus.NotSelected);

    public bool IsPlatformAdmin =>
        string.Equals(PlatformRole, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase)
        || string.Equals(PlatformRole, Roles.SystemAdmin, StringComparison.OrdinalIgnoreCase);

    public bool IsTenantAdmin =>
        string.Equals(TenantRole, Roles.TenantOwner, StringComparison.OrdinalIgnoreCase)
        || string.Equals(TenantRole, Roles.AdminEmpresa, StringComparison.OrdinalIgnoreCase);

    public bool HasTenantSelected =>
        TenantId.HasValue
        && TenantId.Value > 0
        && string.Equals(TenantStatus, AgoraHub360.ERP.Shared.Constants.TenantStatus.Selected, StringComparison.OrdinalIgnoreCase);
}
