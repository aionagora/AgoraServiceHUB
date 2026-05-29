using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgoraHub360.ERP.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/facturas-venta")]
[Authorize]
public class FacturasVentaController : ControllerBase
{
    private readonly IFacturaVentaService _service;

    public FacturasVentaController(IFacturaVentaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<FacturaVentaResumenDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<FacturaVentaResumenDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase)
                || result.Error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)
                || result.Error.Contains("no existe", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FacturaVentaDto>.Fail(result.Error!));
            }

            return BadRequest(ApiResponse<FacturaVentaDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<FacturaVentaDto>.Ok(result.Value!));
    }

    [HttpGet("por-venta/{ventaId:long}")]
    public async Task<IActionResult> GetByVentaId(long ventaId, CancellationToken ct)
    {
        var result = await _service.GetByVentaIdAsync(ventaId, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase)
                || result.Error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)
                || result.Error.Contains("no existe", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FacturaVentaDto>.Fail(result.Error!));
            }

            return BadRequest(ApiResponse<FacturaVentaDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<FacturaVentaDto>.Ok(result.Value!));
    }

    [HttpPost("generar")]
    public async Task<IActionResult> Generar([FromBody] GenerarFacturaVentaRequestDto dto, CancellationToken ct)
    {
        var result = await _service.GenerarDesdeVentaAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<FacturaVentaDto>.Fail(result.Error!));

        return Ok(ApiResponse<FacturaVentaDto>.Ok(result.Value!, "Factura de venta generada exitosamente."));
    }

    [HttpPost("{id:long}/anular")]
    public async Task<IActionResult> Anular(long id, [FromBody] AnularFacturaVentaRequestDto dto, CancellationToken ct)
    {
        var result = await _service.AnularAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("no encontrada", StringComparison.OrdinalIgnoreCase)
                || result.Error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)
                || result.Error.Contains("no existe", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FacturaVentaDto>.Fail(result.Error!));
            }

            return BadRequest(ApiResponse<FacturaVentaDto>.Fail(result.Error!));
        }

        return Ok(ApiResponse<FacturaVentaDto>.Ok(result.Value!, "Factura de venta anulada."));
    }
}
