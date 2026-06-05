namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Usuario;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly ICurrentUserService _currentUserService;

    public UsuariosController(IUsuarioService usuarioService, ICurrentUserService currentUserService)
    {
        _usuarioService = usuarioService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var currentUserId = _currentUserService.UserIdInt;
        if (!currentUserId.HasValue)
            return Unauthorized(ApiResponse<IReadOnlyList<UsuarioDto>>.Fail("Usuario no autenticado."));

        var result = await _usuarioService.GetAllAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<UsuarioDto>>.Fail(result.Error!));

        if (IsPlatformAdmin())
            return Ok(ApiResponse<IReadOnlyList<UsuarioDto>>.Ok(result.Value!));

        if (!IsTenantAdmin())
            return Forbid();

        var tenantId = GetSelectedTenantId();
        if (!tenantId.HasValue)
            return Forbid();

        var usuariosTenant = result.Value!
            .Where(u => u.EmpresasAsignadas.Any(e => e.EmpresaId == tenantId.Value))
            .ToList()
            .AsReadOnly();

        return Ok(ApiResponse<IReadOnlyList<UsuarioDto>>.Ok(usuariosTenant));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var currentUserId = _currentUserService.UserIdInt;
        if (!currentUserId.HasValue)
            return Unauthorized(ApiResponse<UsuarioDto>.Fail("Usuario no autenticado."));

        var result = await _usuarioService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<UsuarioDto>.Fail(result.Error!));

        if (IsPlatformAdmin())
            return Ok(ApiResponse<UsuarioDto>.Ok(result.Value!));

        if (currentUserId.Value == id)
            return Ok(ApiResponse<UsuarioDto>.Ok(result.Value!));

        if (!IsTenantAdmin())
            return Forbid();

        var tenantId = GetSelectedTenantId();
        if (!tenantId.HasValue)
            return Forbid();

        var sameTenant = result.Value!.EmpresasAsignadas.Any(e => e.EmpresaId == tenantId.Value);
        if (!sameTenant)
            return Forbid();

        return Ok(ApiResponse<UsuarioDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioDto dto, CancellationToken ct)
    {
        if (!IsPlatformAdmin() && !IsTenantAdmin())
            return Forbid();

        if (IsTenantAdmin())
        {
            var tenantId = GetSelectedTenantId();
            if (!tenantId.HasValue)
                return Forbid();

            if (dto.EmpresaId.HasValue && dto.EmpresaId.Value != tenantId.Value)
                return Forbid();

            if (IsPlatformRoleName(dto.Rol))
                return Forbid();

            dto.EmpresaId = tenantId.Value;
        }

        var result = await _usuarioService.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<UsuarioDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<UsuarioDto>.Ok(result.Value!, "Usuario creado exitosamente."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioDto dto, CancellationToken ct)
    {
        if (!IsPlatformAdmin())
        {
            if (!IsTenantAdmin())
                return Forbid();

            var tenantId = GetSelectedTenantId();
            if (!tenantId.HasValue)
                return Forbid();

            var target = await _usuarioService.GetByIdAsync(id, ct);
            if (!target.IsSuccess)
                return NotFound(ApiResponse<UsuarioDto>.Fail(target.Error!));

            var sameTenant = target.Value!.EmpresasAsignadas.Any(e => e.EmpresaId == tenantId.Value);
            if (!sameTenant)
                return Forbid();
        }

        var result = await _usuarioService.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado"))
                return NotFound(ApiResponse<UsuarioDto>.Fail(result.Error!));
            return BadRequest(ApiResponse<UsuarioDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<UsuarioDto>.Ok(result.Value!, "Usuario actualizado exitosamente."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!IsPlatformAdmin())
        {
            if (!IsTenantAdmin())
                return Forbid();

            var tenantId = GetSelectedTenantId();
            if (!tenantId.HasValue)
                return Forbid();

            var target = await _usuarioService.GetByIdAsync(id, ct);
            if (!target.IsSuccess)
                return NotFound(ApiResponse<bool>.Fail(target.Error!));

            var sameTenant = target.Value!.EmpresasAsignadas.Any(e => e.EmpresaId == tenantId.Value);
            if (!sameTenant)
                return Forbid();
        }

        var result = await _usuarioService.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Usuario eliminado exitosamente."));
    }

    /// <summary>
    /// Asigna o actualiza el rol de un usuario en una empresa.
    /// </summary>
    [HttpPost("{id:int}/roles")]
    public async Task<IActionResult> AsignarRol(int id, [FromBody] AsignarRolDto dto, CancellationToken ct)
    {
        if (!IsPlatformAdmin())
        {
            if (!IsTenantAdmin())
                return Forbid();

            var tenantId = GetSelectedTenantId();
            if (!tenantId.HasValue)
                return Forbid();

            if (dto.EmpresaId != tenantId.Value)
                return Forbid();

            if (IsPlatformRoleName(dto.Rol))
                return Forbid();
        }

        var result = await _usuarioService.AsignarRolAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Rol asignado exitosamente."));
    }

    /// <summary>
    /// Remueve un usuario de una empresa.
    /// </summary>
    [HttpDelete("{id:int}/empresas/{empresaId:int}")]
    public async Task<IActionResult> RemoverDeEmpresa(int id, int empresaId, CancellationToken ct)
    {
        if (!IsPlatformAdmin())
        {
            if (!IsTenantAdmin())
                return Forbid();

            var tenantId = GetSelectedTenantId();
            if (!tenantId.HasValue || empresaId != tenantId.Value)
                return Forbid();
        }

        var result = await _usuarioService.RemoverDeEmpresaAsync(id, empresaId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Usuario removido de la empresa."));
    }

    /// <summary>
    /// Resetea la contraseña de cualquier usuario. Solo Admin.
    /// </summary>
    [HttpPut("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto, CancellationToken ct)
    {
        if (!IsPlatformAdmin())
        {
            if (!IsTenantAdmin())
                return Forbid();

            var tenantId = GetSelectedTenantId();
            if (!tenantId.HasValue)
                return Forbid();

            var target = await _usuarioService.GetByIdAsync(id, ct);
            if (!target.IsSuccess)
                return NotFound(ApiResponse<bool>.Fail(target.Error!));

            var sameTenant = target.Value!.EmpresasAsignadas.Any(e => e.EmpresaId == tenantId.Value);
            if (!sameTenant)
                return Forbid();
        }

        var result = await _usuarioService.ResetPasswordAsync(id, dto, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Contraseña reseteada exitosamente."));
    }

    private bool IsPlatformAdmin()
        => string.Equals(_currentUserService.PlatformRole, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase)
           || string.Equals(_currentUserService.PlatformRole, Roles.SystemAdmin, StringComparison.OrdinalIgnoreCase);

    private bool IsTenantAdmin()
        => string.Equals(_currentUserService.TenantRole, Roles.TenantOwner, StringComparison.OrdinalIgnoreCase)
           || string.Equals(_currentUserService.TenantRole, Roles.AdminEmpresa, StringComparison.OrdinalIgnoreCase);

    private int? GetSelectedTenantId()
    {
        if (!string.Equals(_currentUserService.TenantStatus, TenantStatus.Selected, StringComparison.OrdinalIgnoreCase))
            return null;

        return _currentUserService.TenantId ?? _currentUserService.EmpresaId;
    }

    private static bool IsPlatformRoleName(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return false;

        return string.Equals(role, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase)
               || string.Equals(role, Roles.SystemAdmin, StringComparison.OrdinalIgnoreCase)
               || string.Equals(role, Roles.SecurityAuditor, StringComparison.OrdinalIgnoreCase)
               || string.Equals(role, Roles.PlatformSupport, StringComparison.OrdinalIgnoreCase);
    }
}
