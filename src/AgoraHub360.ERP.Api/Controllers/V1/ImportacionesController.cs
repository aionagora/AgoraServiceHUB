namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Compras;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compras/importaciones")]
[Authorize]
public class ImportacionesController : ControllerBase
{
    private readonly IHojaImportacionService _service;

    public ImportacionesController(IHojaImportacionService service)
    {
        _service = service;
    }

    /// <summary>Lista hojas de importación con filtros opcionales.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? ordenCompraId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ordenCompraId, desde, hasta, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<HojaImportacionDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<HojaImportacionDto>>.Ok(result.Value!));
    }

    /// <summary>Obtiene una hoja con gastos y distribución.</summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<HojaImportacionDto>.Fail(result.Error!));
        return Ok(ApiResponse<HojaImportacionDto>.Ok(result.Value!));
    }

    /// <summary>Crea una hoja de importación con gastos iniciales.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHojaImportacionDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<HojaImportacionDto>.Fail(result.Error!));
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.HojaImportacionId },
            ApiResponse<HojaImportacionDto>.Ok(result.Value!, "Hoja de importación creada."));
    }

    /// <summary>Agrega un gasto a una hoja no liquidada.</summary>
    [HttpPost("{id:long}/gastos")]
    public async Task<IActionResult> AddGasto(long id, [FromBody] AddGastoImportacionDto dto, CancellationToken ct)
    {
        var result = await _service.AddGastoAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<HojaImportacionDto>.Fail(result.Error!));
        return Ok(ApiResponse<HojaImportacionDto>.Ok(result.Value!, "Gasto agregado."));
    }

    /// <summary>Elimina un gasto de una hoja no liquidada.</summary>
    [HttpDelete("{id:long}/gastos/{gastoId:long}")]
    public async Task<IActionResult> RemoveGasto(long id, long gastoId, CancellationToken ct)
    {
        var result = await _service.RemoveGastoAsync(id, gastoId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<HojaImportacionDto>.Fail(result.Error!));
        return Ok(ApiResponse<HojaImportacionDto>.Ok(result.Value!, "Gasto eliminado."));
    }

    /// <summary>Liquida la hoja: distribuye gastos y actualiza costos de inventario.</summary>
    [HttpPost("{id:long}/liquidar")]
    public async Task<IActionResult> Liquidar(long id, CancellationToken ct)
    {
        var result = await _service.LiquidarAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<HojaImportacionDto>.Fail(result.Error!));
        return Ok(ApiResponse<HojaImportacionDto>.Ok(result.Value!, "Hoja liquidada. Costos de inventario actualizados."));
    }

    /// <summary>Elimina una hoja no liquidada.</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Hoja eliminada."));
    }
}
