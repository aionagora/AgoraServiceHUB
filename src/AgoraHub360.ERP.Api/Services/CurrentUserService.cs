namespace AgoraHub360.ERP.Api.Services;

using System.Security.Claims;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;

/// <summary>
/// Implementación de ICurrentUserService que extrae información
/// del usuario autenticado desde HttpContext (Claims JWT).
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public int? UserIdInt
    {
        get
        {
            var userId = UserId;
            return int.TryParse(userId, out var id) ? id : null;
        }
    }

    public string? UserName =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

    public int? EmpresaId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypesCustom.EmpresaId);
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public int? TenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypesCustom.TenantId)
                        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypesCustom.EmpresaId);
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public string PlatformRole
        => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypesCustom.PlatformRole)
           ?? Roles.None;

    public string TenantRole
        => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypesCustom.TenantRole)
           ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role)
           ?? Roles.NoAccess;

    public string TenantStatus
        => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypesCustom.TenantStatus)
           ?? (TenantId.HasValue ? AgoraHub360.ERP.Shared.Constants.TenantStatus.Selected : AgoraHub360.ERP.Shared.Constants.TenantStatus.NotSelected);

    public bool IsInRole(string role)
        => _httpContextAccessor.HttpContext?.User?.IsInRole(role) == true;
}
