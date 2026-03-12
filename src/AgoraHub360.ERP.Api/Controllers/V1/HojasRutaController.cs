namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Logistica;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/logistica/hojas-ruta")]
[Authorize]
public class HojasRutaController : ControllerBase
{
    private readonly IHojaRutaService _service;

    public HojasRutaController(IHojaRutaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? estado,
        [FromQuery] string? subEstado,
        [FromQuery] string? tipoOP,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(estado, subEstado, tipoOP, desde, hasta, search, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<HojaRutaDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<HojaRutaDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<HojaRutaDto>.Fail(result.Error!));
        return Ok(ApiResponse<HojaRutaDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHojaRutaDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<HojaRutaDto>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById),
            new { id = result.Value!.HojaRutaId },
            ApiResponse<HojaRutaDto>.Ok(result.Value!, "Hoja de Ruta creada."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateHojaRutaDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<HojaRutaDto>.Fail(result.Error!));
        return Ok(ApiResponse<HojaRutaDto>.Ok(result.Value!, "Hoja de Ruta actualizada."));
    }

    [HttpPost("{id:long}/cambiar-estado")]
    public async Task<IActionResult> CambiarEstado(long id, [FromBody] CambiarEstadoHojaRutaDto dto, CancellationToken ct)
    {
        var result = await _service.CambiarEstadoAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<HojaRutaDto>.Fail(result.Error!));
        return Ok(ApiResponse<HojaRutaDto>.Ok(result.Value!, "Estado actualizado."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Hoja de Ruta eliminada."));
    }

    /// <summary>Devuelve las hojas de ruta vinculadas a una Orden de Pedido.</summary>
    [HttpGet("by-orden-pedido/{ordenPedidoId:long}")]
    public async Task<IActionResult> GetByOrdenPedido(long ordenPedidoId, CancellationToken ct)
    {
        var result = await _service.GetByOrdenPedidoAsync(ordenPedidoId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<List<HojaRutaDto>>.Fail(result.Error!));
        return Ok(ApiResponse<List<HojaRutaDto>>.Ok(result.Value!));
    }
}
