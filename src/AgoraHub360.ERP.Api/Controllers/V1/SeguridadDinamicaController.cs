namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Seguridad;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/seguridad-dinamica")]
[Authorize]
public class SeguridadDinamicaController : ControllerBase
{
    private readonly ISeguridadDinamicaService _service;
    private readonly ICurrentUserService _currentUserService;

    public SeguridadDinamicaController(ISeguridadDinamicaService service, ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }

    [HttpGet("contexto-sesion")]
    [Authorize(Policy = PolicyNames.RequireTenantSelected)]
    [Authorize(Policy = PolicyNames.RequireTenantMembership)]
    public async Task<IActionResult> GetContextoSesion(CancellationToken ct)
    {
        if (!HasTenantContext())
            return Forbid();

        var result = await _service.GetSesionContextAsync(ct: ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<SesionContextoDto>.Fail(result.Error!));

        return Ok(ApiResponse<SesionContextoDto>.Ok(result.Value!));
    }

    [HttpGet("menu")]
    [Authorize(Policy = PolicyNames.RequireTenantSelected)]
    [Authorize(Policy = PolicyNames.RequireTenantMembership)]
    public async Task<IActionResult> GetMenu(CancellationToken ct)
    {
        if (!HasTenantContext())
            return Forbid();

        var result = await _service.GetMenuUsuarioAsync(ct: ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<ModuloSistemaDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<ModuloSistemaDto>>.Ok(result.Value!));
    }

    [HttpGet("permiso")]
    [Authorize(Policy = PolicyNames.RequireTenantSelected)]
    [Authorize(Policy = PolicyNames.RequireTenantMembership)]
    public async Task<IActionResult> HasPermission([FromQuery] string formularioCodigo, [FromQuery] string? accionCodigo, CancellationToken ct)
    {
        if (!HasTenantContext())
            return Forbid();

        var result = await _service.HasPermissionAsync(formularioCodigo, accionCodigo, ct: ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(result.Value));
    }

    [HttpPost("perfiles")]
    [Authorize(Policy = PolicyNames.RequireTenantAdmin)]
    public async Task<IActionResult> CreatePerfil([FromBody] UpsertPerfilAccesoDto dto, CancellationToken ct)
    {
        if (!IsTenantAdmin() || !HasTenantContext())
            return Forbid();

        var result = await _service.CreatePerfilAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<PerfilAccesoDto>.Fail(result.Error!));

        return Ok(ApiResponse<PerfilAccesoDto>.Ok(result.Value!, "Perfil creado exitosamente."));
    }

    [HttpPost("permisos")]
    [Authorize(Policy = PolicyNames.RequireTenantAdmin)]
    public async Task<IActionResult> SetPermiso([FromBody] UpsertPerfilPermisoDto dto, CancellationToken ct)
    {
        if (!IsTenantAdmin() || !HasTenantContext())
            return Forbid();

        var result = await _service.SetPermisoPerfilAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(result.Value, "Permiso actualizado exitosamente."));
    }

    [HttpPost("asignaciones")]
    [Authorize(Policy = PolicyNames.RequireTenantAdmin)]
    public async Task<IActionResult> AsignarPerfil([FromBody] AsignarUsuarioPerfilDto dto, CancellationToken ct)
    {
        if (!IsTenantAdmin() || !HasTenantContext())
            return Forbid();

        var result = await _service.AsignarPerfilUsuarioAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(result.Value, "Perfil asignado exitosamente."));
    }

    private bool HasTenantContext()
    {
        var tenantId = _currentUserService.TenantId ?? _currentUserService.EmpresaId;
        return tenantId.HasValue
               && tenantId.Value > 0
               && string.Equals(_currentUserService.TenantStatus, TenantStatus.Selected, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsTenantAdmin()
        => string.Equals(_currentUserService.TenantRole, Roles.TenantOwner, StringComparison.OrdinalIgnoreCase)
           || string.Equals(_currentUserService.TenantRole, Roles.AdminEmpresa, StringComparison.OrdinalIgnoreCase);
}
