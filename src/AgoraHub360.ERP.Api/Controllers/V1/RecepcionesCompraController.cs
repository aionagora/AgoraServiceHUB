namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Compras;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compras/recepciones")]
[Authorize]
public class RecepcionesCompraController : ControllerBase
{
    private readonly IRecepcionCompraService _service;

    public RecepcionesCompraController(IRecepcionCompraService service)
    {
        _service = service;
    }

    /// <summary>Lista recepciones con filtros opcionales.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? ordenCompraId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ordenCompraId, desde, hasta, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<RecepcionCompraDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<RecepcionCompraDto>>.Ok(result.Value!));
    }

    /// <summary>Obtiene una recepción por Id con sus líneas.</summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<RecepcionCompraDto>.Fail(result.Error!));
        return Ok(ApiResponse<RecepcionCompraDto>.Ok(result.Value!));
    }

    /// <summary>
    /// Crea una recepción: valida OC aprobada, genera movimientos Receipt,
    /// actualiza stock y CantidadRecepcionada de las líneas de OC.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecepcionCompraDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<RecepcionCompraDto>.Fail(result.Error!));
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.RecepcionCompraId },
            ApiResponse<RecepcionCompraDto>.Ok(result.Value!, "Recepción registrada exitosamente."));
    }

    /// <summary>Elimina (soft-delete) una recepción no confirmada.</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Recepción eliminada."));
    }
}
