namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contabilidad/cuentas")]
[Authorize]
public class CuentasContablesController : ControllerBase
{
    private readonly ICuentaContableService _service;

    public CuentasContablesController(ICuentaContableService service)
    {
        _service = service;
    }

    /// <summary>Lista todas las cuentas (flat) con filtros opcionales.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] byte? tipo,
        [FromQuery] bool? permiteMovimientos,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(tipo, permiteMovimientos, search, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<CuentaContableDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<CuentaContableDto>>.Ok(result.Value!));
    }

    /// <summary>Obtiene el plan de cuentas en estructura de árbol.</summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetTree(CancellationToken ct)
    {
        var result = await _service.GetTreeAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<CuentaContableDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<CuentaContableDto>>.Ok(result.Value!));
    }

    /// <summary>Obtiene una cuenta por Id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<CuentaContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<CuentaContableDto>.Ok(result.Value!));
    }

    /// <summary>Crea una nueva cuenta contable.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCuentaContableDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CuentaContableDto>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById),
            new { id = result.Value!.CuentaContableId },
            ApiResponse<CuentaContableDto>.Ok(result.Value!, "Cuenta creada."));
    }

    /// <summary>Actualiza una cuenta existente.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCuentaContableDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CuentaContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<CuentaContableDto>.Ok(result.Value!, "Cuenta actualizada."));
    }

    /// <summary>Elimina una cuenta sin movimientos ni subcuentas.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Cuenta eliminada."));
    }

    /// <summary>Genera el plan de cuentas estándar Bolivia/NIIF.</summary>
    [HttpPost("seed")]
    public async Task<IActionResult> Seed(CancellationToken ct)
    {
        var result = await _service.SeedPlanCuentasAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<int>.Fail(result.Error!));
        return Ok(ApiResponse<int>.Ok(result.Value!, $"Plan de cuentas generado: {result.Value} cuentas creadas."));
    }
}
