namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contabilidad/asientos")]
[Authorize]
public class AsientosContablesController : ControllerBase
{
    private readonly IAsientoContableService _service;

    public AsientosContablesController(IAsientoContableService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
        [FromQuery] string? estado, [FromQuery] int? tipoComprobanteId,
        [FromQuery] string? search, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(desde, hasta, estado, tipoComprobanteId, search, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<AsientoContableDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<AsientoContableDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<AsientoContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<AsientoContableDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAsientoContableDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<AsientoContableDto>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.AsientoContableId },
            ApiResponse<AsientoContableDto>.Ok(result.Value!, "Comprobante creado."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateAsientoContableDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<AsientoContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<AsientoContableDto>.Ok(result.Value!, "Comprobante actualizado."));
    }

    [HttpPost("{id:long}/copiar")]
    public async Task<IActionResult> Copiar(long id, CancellationToken ct)
    {
        var result = await _service.CopiarAsync(id, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<AsientoContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<AsientoContableDto>.Ok(result.Value!, "Comprobante copiado."));
    }

    [HttpPost("{id:long}/contabilizar")]
    public async Task<IActionResult> Contabilizar(long id, CancellationToken ct)
    {
        var result = await _service.ContabilizarAsync(id, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<AsientoContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<AsientoContableDto>.Ok(result.Value!, "Comprobante contabilizado."));
    }

    [HttpPost("{id:long}/anular")]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
    {
        var result = await _service.AnularAsync(id, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<AsientoContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<AsientoContableDto>.Ok(result.Value!, "Comprobante anulado."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Comprobante eliminado."));
    }

    // ?? Catálogos ??
    [HttpGet("tipos-comprobante")]
    public async Task<IActionResult> GetTiposComprobante(CancellationToken ct)
    {
        var r = await _service.GetTiposComprobanteAsync(ct);
        return r.IsSuccess ? Ok(ApiResponse<IReadOnlyList<TipoComprobanteDto>>.Ok(r.Value!)) : BadRequest(ApiResponse<IReadOnlyList<TipoComprobanteDto>>.Fail(r.Error!));
    }

    [HttpGet("tipos-cambio")]
    public async Task<IActionResult> GetTiposCambio(CancellationToken ct)
    {
        var r = await _service.GetTiposCambioAsync(ct);
        return r.IsSuccess ? Ok(ApiResponse<IReadOnlyList<TipoCambioDto>>.Ok(r.Value!)) : BadRequest(ApiResponse<IReadOnlyList<TipoCambioDto>>.Fail(r.Error!));
    }

    [HttpGet("tipos-pago")]
    public async Task<IActionResult> GetTiposPago(CancellationToken ct)
    {
        var r = await _service.GetTiposPagoAsync(ct);
        return r.IsSuccess ? Ok(ApiResponse<IReadOnlyList<TipoPagoDto>>.Ok(r.Value!)) : BadRequest(ApiResponse<IReadOnlyList<TipoPagoDto>>.Fail(r.Error!));
    }

    [HttpPost("seed-catalogos")]
    public async Task<IActionResult> SeedCatalogos(CancellationToken ct)
    {
        var r = await _service.SeedCatalogosAsync(ct);
        return r.IsSuccess ? Ok(ApiResponse<int>.Ok(r.Value!, $"{r.Value} catálogos generados.")) : BadRequest(ApiResponse<int>.Fail(r.Error!));
    }
}
