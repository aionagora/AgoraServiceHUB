namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Sucursal;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class SucursalesController : ControllerBase
{
    private readonly ISucursalService _sucursalService;
    private readonly ICurrentUserService _currentUserService;

    public SucursalesController(ISucursalService sucursalService, ICurrentUserService currentUserService)
    {
        _sucursalService = sucursalService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [Authorize(Policy = PolicyNames.RequireTenantSelected)]
    [Authorize(Policy = PolicyNames.RequireTenantMembership)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        if (!HasTenantSelected())
            return Forbid();

        var result = await _sucursalService.GetAllAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<SucursalListadoDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<SucursalListadoDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PolicyNames.RequireTenantSelected)]
    [Authorize(Policy = PolicyNames.RequireTenantMembership)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        if (!HasTenantSelected())
            return Forbid();

        var result = await _sucursalService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<SucursalDto>.Fail(result.Error!));

        return Ok(ApiResponse<SucursalDto>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.RequireTenantAdmin)]
    public async Task<IActionResult> Create([FromBody] CrearSucursalDto dto, CancellationToken ct)
    {
        if (!IsTenantAdmin() || !HasTenantSelected())
            return Forbid();

        var result = await _sucursalService.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<SucursalDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<SucursalDto>.Ok(result.Value!, "Sucursal creada exitosamente."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PolicyNames.RequireTenantAdmin)]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarSucursalDto dto, CancellationToken ct)
    {
        if (!IsTenantAdmin() || !HasTenantSelected())
            return Forbid();

        var result = await _sucursalService.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<SucursalDto>.Fail(result.Error!));

        return Ok(ApiResponse<SucursalDto>.Ok(result.Value!, "Sucursal actualizada de manera exitosa."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PolicyNames.RequireTenantAdmin)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!IsTenantAdmin() || !HasTenantSelected())
            return Forbid();

        var result = await _sucursalService.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Sucursal eliminada lógicamente."));
    }

    [HttpPatch("{id:int}/estado")]
    [Authorize(Policy = PolicyNames.RequireTenantAdmin)]
    public async Task<IActionResult> PatchEstado(int id, [FromBody] bool activo, CancellationToken ct)
    {
        if (!IsTenantAdmin() || !HasTenantSelected())
            return Forbid();

        var result = await _sucursalService.CambiarEstadoAsync(id, activo, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        var accion = activo ? "activada" : "desactivada";
        return Ok(ApiResponse<bool>.Ok(true, $"Sucursal {accion} exitosamente."));
    }

    [HttpPatch("{id:int}/principal")]
    [Authorize(Policy = PolicyNames.RequireTenantAdmin)]
    public async Task<IActionResult> SetPrincipal(int id, CancellationToken ct)
    {
        if (!IsTenantAdmin() || !HasTenantSelected())
            return Forbid();

        var result = await _sucursalService.EstablecerCentralAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Sucursal establecida como central (principal) de la empresa."));
    }

    private bool HasTenantSelected()
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
