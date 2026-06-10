using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.CxC;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgoraHub360.ERP.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cuentas-por-cobrar")]
[Authorize]
public class CuentasPorCobrarController : ControllerBase
{
    private readonly ICuentasPorCobrarService _service;

    public CuentasPorCobrarController(ICuentasPorCobrarService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene todas las cuentas por cobrar de la empresa activa.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? clienteId,
        [FromQuery] string? estado,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        CancellationToken ct)
    {
        var filter = new CuentaPorCobrarFilterDto
        {
            ClienteId = clienteId,
            Estado = estado,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta
        };

        var result = await _service.GetAllAsync(filter, ct);
        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>
    /// Obtiene el detalle de una cuenta por cobrar específica.
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.IsSuccess
            ? Ok(result)
            : NotFound(result);
    }

    /// <summary>
    /// Genera o actualiza la cuenta por cobrar desde una factura de venta.
    /// </summary>
    [HttpPost("generar-desde-factura/{facturaVentaId:long}")]
    public async Task<IActionResult> GenerarDesdeFactura(long facturaVentaId, CancellationToken ct)
    {
        var result = await _service.GenerarDesdeFacturaAsync(facturaVentaId, ct);
        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>
    /// Actualiza el saldo de la cuenta por cobrar cuando se registra un pago.
    /// </summary>
    [HttpPut("actualizar-por-pago/{facturaVentaId:long}")]
    public async Task<IActionResult> ActualizarPorPago(
        long facturaVentaId,
        [FromBody] ActualizarCxcPorPagoRequestDto dto,
        CancellationToken ct)
    {
        var result = await _service.ActualizarPorPagoAsync(facturaVentaId, dto.MontoPagado, ct);
        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>
    /// Anula una cuenta por cobrar.
    /// </summary>
    [HttpPut("{id:long}/anular")]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
    {
        var result = await _service.AnularAsync(id, ct);
        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>
    /// Obtiene el saldo total pendiente de la empresa activa.
    /// </summary>
    [HttpGet("saldo-pendiente")]
    public async Task<IActionResult> GetSaldoPendiente(CancellationToken ct)
    {
        var result = await _service.GetSaldoTotalPendienteAsync(ct);
        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>
    /// Obtiene el reporte de antigüedad de saldos.
    /// </summary>
    [HttpGet("antiguedad-saldos")]
    public async Task<IActionResult> GetAntiguedadSaldos(CancellationToken ct)
    {
        var result = await _service.GetAntiguedadSaldosAsync(ct);
        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }
}

/// <summary>
/// DTO para actualizar CxC por pago.
/// </summary>
public class ActualizarCxcPorPagoRequestDto
{
    public decimal MontoPagado { get; set; }
}
