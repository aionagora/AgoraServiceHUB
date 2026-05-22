namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Inventario;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/inventario")]
[Authorize]
public class MovimientosInventarioController : ControllerBase
{
    private readonly IMovimientoInventarioService _service;

    public MovimientosInventarioController(IMovimientoInventarioService service)
    {
        _service = service;
    }

    /// <summary>Lista movimientos con filtros opcionales.</summary>
    [HttpGet("movimientos")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? productoId,
        [FromQuery] int? almacenId,
        [FromQuery] string? tipo,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(productoId, almacenId, tipo, desde, hasta, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<MovimientoInventarioDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<MovimientoInventarioDto>>.Ok(result.Value!));
    }

    /// <summary>Obtiene un movimiento por Id.</summary>
    [HttpGet("movimientos/{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<MovimientoInventarioDto>.Fail(result.Error!));
        return Ok(ApiResponse<MovimientoInventarioDto>.Ok(result.Value!));
    }

    /// <summary>Registra un nuevo movimiento de inventario.</summary>
    [HttpPost("movimientos")]
    public async Task<IActionResult> Create([FromBody] CreateMovimientoInventarioDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<MovimientoInventarioDto>.Fail(result.Error!));
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            ApiResponse<MovimientoInventarioDto>.Ok(result.Value!, "Movimiento registrado exitosamente."));
    }

    /// <summary>Obtiene el kardex de un producto con saldo acumulado.</summary>
    [HttpGet("kardex/{companyProductId:long}")]
    public async Task<IActionResult> GetKardex(
        long companyProductId,
        [FromQuery] int? almacenId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken ct)
    {
        var result = await _service.GetKardexAsync(companyProductId, almacenId, desde, hasta, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<KardexDto>.Fail(result.Error!));
        return Ok(ApiResponse<KardexDto>.Ok(result.Value!));
    }

    /// <summary>Obtiene el stock actual por producto y almacén.</summary>
    [HttpGet("stock")]
    public async Task<IActionResult> GetStock(
        [FromQuery] int? almacenId,
        CancellationToken ct)
    {
        var result = await _service.GetStockAsync(almacenId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<StockProductoDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<StockProductoDto>>.Ok(result.Value!));
    }
}
