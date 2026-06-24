namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Numeracion;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/numeraciones-documento")]
[Authorize]
public class NumeracionesController : ControllerBase
{
    private readonly INumeracionDocumentoService _service;
    private readonly ICurrentUserService _currentUserService;

    public NumeracionesController(INumeracionDocumentoService service, ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [Authorize(Policy = PolicyNames.RequireTenantSelected)]
    [Authorize(Policy = PolicyNames.RequireTenantMembership)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        if (!HasTenantContext())
            return Forbid();

        var result = await _service.GetAllAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<NumeracionDocumentoDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<NumeracionDocumentoDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PolicyNames.RequireTenantSelected)]
    [Authorize(Policy = PolicyNames.RequireTenantMembership)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        if (!HasTenantContext())
            return Forbid();

        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<NumeracionDocumentoDto>.Fail(result.Error!));

        return Ok(ApiResponse<NumeracionDocumentoDto>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.RequireTenantAdmin)]
    public async Task<IActionResult> Create([FromBody] CrearNumeracionDocumentoRequestDto dto, CancellationToken ct)
    {
        if (!IsTenantAdmin() || !HasTenantContext())
            return Forbid();

        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<NumeracionDocumentoDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<NumeracionDocumentoDto>.Ok(result.Value!, "Numeración creada exitosamente."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PolicyNames.RequireTenantAdmin)]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarNumeracionDocumentoRequestDto dto, CancellationToken ct)
    {
        if (!IsTenantAdmin() || !HasTenantContext())
            return Forbid();

        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada"))
                return NotFound(ApiResponse<NumeracionDocumentoDto>.Fail(result.Error!));
            return BadRequest(ApiResponse<NumeracionDocumentoDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<NumeracionDocumentoDto>.Ok(result.Value!, "Numeración actualizada exitosamente."));
    }

    [HttpPatch("{id:int}/activar")]
    [Authorize(Policy = PolicyNames.RequireTenantAdmin)]
    public async Task<IActionResult> Activar(int id, CancellationToken ct)
    {
        if (!IsTenantAdmin() || !HasTenantContext())
            return Forbid();

        var result = await _service.ActivarAsync(id, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada"))
                return NotFound(ApiResponse<bool>.Fail(result.Error!));

            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        }

        return Ok(ApiResponse<bool>.Ok(true, "Numeración activada exitosamente."));
    }

    [HttpPatch("{id:int}/desactivar")]
    [Authorize(Policy = PolicyNames.RequireTenantAdmin)]
    public async Task<IActionResult> Desactivar(int id, CancellationToken ct)
    {
        if (!IsTenantAdmin() || !HasTenantContext())
            return Forbid();

        var result = await _service.DesactivarAsync(id, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada"))
                return NotFound(ApiResponse<bool>.Fail(result.Error!));

            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        }

        return Ok(ApiResponse<bool>.Ok(true, "Numeración desactivada exitosamente."));
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
