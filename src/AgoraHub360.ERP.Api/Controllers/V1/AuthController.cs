namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Auth;
using AgoraHub360.ERP.Shared.DTOs.Empresa;
using AgoraHub360.ERP.Shared.Constants;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUsuarioService _usuarioService;
    private readonly IEmpresaService _empresaService;

    public AuthController(
        IAuthService authService,
        IUsuarioService usuarioService,
        IEmpresaService empresaService)
    {
        _authService = authService;
        _usuarioService = usuarioService;
        _empresaService = empresaService;
    }

    /// <summary>
    /// Autentica un usuario y devuelve un token JWT.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);

        if (result is null)
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Credenciales inválidas."));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login exitoso."));
    }

    /// <summary>
    /// Devuelve la información del usuario autenticado.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var rol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var empresaId = User.FindFirst(ClaimTypesCustom.EmpresaId)?.Value;
        var tenantId = User.FindFirst(ClaimTypesCustom.TenantId)?.Value;
        var platformRole = User.FindFirst(ClaimTypesCustom.PlatformRole)?.Value;
        var tenantRole = User.FindFirst(ClaimTypesCustom.TenantRole)?.Value;
        var tenantStatus = User.FindFirst(ClaimTypesCustom.TenantStatus)?.Value;

        var info = new
        {
            UserId = userId,
            UserName = userName,
            Email = email,
            Rol = rol,
            EmpresaId = empresaId,
            TenantId = tenantId,
            PlatformRole = platformRole,
            TenantRole = tenantRole,
            TenantStatus = tenantStatus
        };

        return Ok(ApiResponse<object>.Ok(info));
    }

    /// <summary>
    /// Devuelve las empresas asignadas al usuario autenticado.
    /// SuperAdmin/SystemAdmin pueden ver todas las empresas activas.
    /// </summary>
    [HttpGet("mis-empresas")]
    [Authorize]
    public async Task<IActionResult> MisEmpresas(CancellationToken ct)
    {
        var platformRole = User.FindFirst(ClaimTypesCustom.PlatformRole)?.Value;
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Solo roles globales de plataforma pueden ver todas
        if (string.Equals(platformRole, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase)
            || string.Equals(platformRole, Roles.SystemAdmin, StringComparison.OrdinalIgnoreCase))
        {
            var allResult = await _empresaService.GetAllAsync(ct);
            var activas = allResult.Value!
                .Where(e => e.Activo)
                .ToList() as IReadOnlyList<EmpresaDto>;
            return Ok(ApiResponse<IReadOnlyList<EmpresaDto>>.Ok(activas));
        }

        // Usuarios normales solo ven sus empresas asignadas
        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized(ApiResponse<IReadOnlyList<EmpresaDto>>.Fail("Usuario no identificado."));

        var asignaciones = await _usuarioService.GetEmpresasAsignadasAsync(userId, ct);
        if (!asignaciones.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<EmpresaDto>>.Fail(asignaciones.Error!));

        // Convertir UsuarioEmpresaRolDto a EmpresaDto obteniendo los datos completos
        var empresaIds = asignaciones.Value!.Select(a => a.EmpresaId).ToHashSet();
        var todasEmpresas = await _empresaService.GetAllAsync(ct);
        var misEmpresas = todasEmpresas.Value!
            .Where(e => empresaIds.Contains(e.Id) && e.Activo)
            .ToList() as IReadOnlyList<EmpresaDto>;

        return Ok(ApiResponse<IReadOnlyList<EmpresaDto>>.Ok(misEmpresas));
    }

    [HttpPost("seleccionar-empresa")]
    [Authorize]
    public async Task<IActionResult> SeleccionarEmpresa([FromBody] SeleccionarEmpresaRequestDto request, CancellationToken ct)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized(ApiResponse<CambiarEmpresaResponseDto>.Fail("Usuario no identificado."));

        var result = await _authService.CambiarEmpresaActivaAsync(userId, request.EmpresaId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CambiarEmpresaResponseDto>.Fail(result.Error!));

        return Ok(ApiResponse<CambiarEmpresaResponseDto>.Ok(result.Value!));
    }
}
