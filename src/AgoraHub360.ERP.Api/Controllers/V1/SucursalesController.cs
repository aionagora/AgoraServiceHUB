namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
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

    public SucursalesController(ISucursalService sucursalService)
    {
        _sucursalService = sucursalService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sucursalService.GetAllAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<SucursalListadoDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<SucursalListadoDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _sucursalService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<SucursalDto>.Fail(result.Error!));

        return Ok(ApiResponse<SucursalDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearSucursalDto dto, CancellationToken ct)
    {
        var result = await _sucursalService.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<SucursalDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<SucursalDto>.Ok(result.Value!, "Sucursal creada exitosamente."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarSucursalDto dto, CancellationToken ct)
    {
        var result = await _sucursalService.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<SucursalDto>.Fail(result.Error!));

        return Ok(ApiResponse<SucursalDto>.Ok(result.Value!, "Sucursal actualizada de manera exitosa."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _sucursalService.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Sucursal eliminada lógicamente."));
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> PatchEstado(int id, [FromBody] bool activo, CancellationToken ct)
    {
        var result = await _sucursalService.CambiarEstadoAsync(id, activo, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        var accion = activo ? "activada" : "desactivada";
        return Ok(ApiResponse<bool>.Ok(true, $"Sucursal {accion} exitosamente."));
    }

    [HttpPatch("{id:int}/principal")]
    public async Task<IActionResult> SetPrincipal(int id, CancellationToken ct)
    {
        var result = await _sucursalService.EstablecerCentralAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Sucursal establecida como central (principal) de la empresa."));
    }
}
