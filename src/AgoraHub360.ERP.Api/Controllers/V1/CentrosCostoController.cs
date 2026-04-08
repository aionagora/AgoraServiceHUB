namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contabilidad/centros-costo")]
[Authorize]
public class CentrosCostoController : ControllerBase
{
    private readonly ICentroCostoService _service;
    private readonly ICurrentUserService _currentUser;

    public CentrosCostoController(ICentroCostoService service, ICurrentUserService currentUser)
    {
        _service     = service;
        _currentUser = currentUser;
    }

    /// <summary>Lista todos los centros de costo activos de la empresa del JWT.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Unauthorized(ApiResponse<string>.Fail("EmpresaId no encontrado en el token."));

        var result = await _service.GetAllByEmpresaAsync(empresaId.Value, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<CentroCostoDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<CentroCostoDto>>.Ok(result.Value!));
    }

    /// <summary>Obtiene un centro de costo por Id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<CentroCostoDto>.Fail(result.Error!));

        return Ok(ApiResponse<CentroCostoDto>.Ok(result.Value!));
    }

    /// <summary>Crea un nuevo centro de costo para la empresa del JWT.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCentroCostoDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CentroCostoDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<CentroCostoDto>.Ok(result.Value!, "Centro de costo creado."));
    }

    /// <summary>Actualiza nombre, descripción o padre de un centro de costo.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCentroCostoDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<CentroCostoDto>.Fail(result.Error!));

        return Ok(ApiResponse<CentroCostoDto>.Ok(result.Value!, "Centro de costo actualizado."));
    }

    /// <summary>Elimina (soft-delete) un centro de costo sin hijos activos.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Centro de costo eliminado."));
    }
}
