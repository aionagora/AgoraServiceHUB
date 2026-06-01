namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Rol;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRolService _rolService;
    private readonly ICurrentUserService _currentUserService;

    public RolesController(IRolService rolService, ICurrentUserService currentUserService)
    {
        _rolService = rolService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [Authorize(Policy = PolicyNames.RequireAuthenticated)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        if (!CanReadRoles())
            return Forbid();

        var result = await _rolService.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<RolDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PolicyNames.RequireAuthenticated)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        if (!CanReadRoles())
            return Forbid();

        var result = await _rolService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<RolDto>.Fail(result.Error!));

        return Ok(ApiResponse<RolDto>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.RequirePlatformAdmin)]
    public async Task<IActionResult> Create([FromBody] CreateRolDto dto, CancellationToken ct)
    {
        if (!IsPlatformAdmin())
            return Forbid();

        var result = await _rolService.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<RolDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<RolDto>.Ok(result.Value!, "Rol creado exitosamente."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PolicyNames.RequirePlatformAdmin)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRolDto dto, CancellationToken ct)
    {
        if (!IsPlatformAdmin())
            return Forbid();

        var result = await _rolService.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrado"))
                return NotFound(ApiResponse<RolDto>.Fail(result.Error!));
            return BadRequest(ApiResponse<RolDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<RolDto>.Ok(result.Value!, "Rol actualizado exitosamente."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PolicyNames.RequirePlatformAdmin)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!IsPlatformAdmin())
            return Forbid();

        var result = await _rolService.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Rol eliminado exitosamente."));
    }

    private bool IsPlatformAdmin()
        => string.Equals(_currentUserService.PlatformRole, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase)
           || string.Equals(_currentUserService.PlatformRole, Roles.SystemAdmin, StringComparison.OrdinalIgnoreCase);

    private bool IsTenantAdmin()
        => string.Equals(_currentUserService.TenantRole, Roles.TenantOwner, StringComparison.OrdinalIgnoreCase)
           || string.Equals(_currentUserService.TenantRole, Roles.AdminEmpresa, StringComparison.OrdinalIgnoreCase);

    private bool CanReadRoles()
        => IsPlatformAdmin() || IsTenantAdmin();
}
