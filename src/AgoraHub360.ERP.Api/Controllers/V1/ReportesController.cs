using AgoraHub360.ERP.Application.Interfaces.Reports;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgoraHub360.ERP.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly IPdfReporteService _pdfReporteService;

    public ReportesController(IPdfReporteService pdfReporteService)
    {
        _pdfReporteService = pdfReporteService;
    }

    /// <summary>
    /// GET /api/v1/cuentas-por-cobrar/pagos/{pagoId}/recibo-pdf
    /// Genera el recibo de pago PDF.
    /// </summary>
    [HttpGet("api/v{version:apiVersion}/cuentas-por-cobrar/pagos/{pagoId:long}/recibo-pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReciboPagoPdf(long pagoId, CancellationToken ct)
    {
        try
        {
            var pdf = await _pdfReporteService.GenerarReciboPagoAsync(pagoId, ct);
            return File(pdf, "application/pdf", $"recibo-pago-{pagoId:D6}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Success = false, Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/v1/reportes/cxc/estado-cuenta-cliente/{clienteId}
    /// Genera el estado de cuenta PDF de un cliente.
    /// </summary>
    [HttpGet("api/v{version:apiVersion}/reportes/cxc/estado-cuenta-cliente/{clienteId:int}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEstadoCuentaClientePdf(
        int clienteId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] bool soloPendientes = false,
        [FromQuery] bool incluirPagos = false,
        CancellationToken ct = default)
    {
        try
        {
            var pdf = await _pdfReporteService.GenerarEstadoCuentaClienteAsync(
                clienteId, fechaDesde, fechaHasta, soloPendientes, incluirPagos, ct);
            return File(pdf, "application/pdf", $"estado-cuenta-{clienteId}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Success = false, Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/v1/facturas-venta/{facturaId}/pdf
    /// Genera la factura PDF.
    /// </summary>
    [HttpGet("api/v{version:apiVersion}/facturas-venta/{facturaId:long}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFacturaPdf(long facturaId, CancellationToken ct)
    {
        try
        {
            var pdf = await _pdfReporteService.GenerarFacturaPdfAsync(facturaId, ct);
            return File(pdf, "application/pdf", $"factura-{facturaId}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Success = false, Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/v1/reportes/cxc/pdf
    /// Genera el reporte general de CxC PDF.
    /// </summary>
    [HttpGet("api/v{version:apiVersion}/reportes/cxc/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReporteCxcGeneralPdf(
        [FromQuery] int? clienteId = null,
        [FromQuery] string? estado = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] bool? vencidas = null,
        [FromQuery] bool? conSaldo = null,
        CancellationToken ct = default)
    {
        try
        {
            var pdf = await _pdfReporteService.GenerarReporteCxcGeneralAsync(
                clienteId, estado, fechaDesde, fechaHasta, vencidas, conSaldo, ct);
            return File(pdf, "application/pdf", "reporte-cxc.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Success = false, Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/v1/reportes/cxc/vencidas/pdf
    /// Genera el reporte de CxC vencidas PDF.
    /// </summary>
    [HttpGet("api/v{version:apiVersion}/reportes/cxc/vencidas/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReporteCxcVencidasPdf(CancellationToken ct)
    {
        try
        {
            var pdf = await _pdfReporteService.GenerarReporteCxcVencidasAsync(ct);
            return File(pdf, "application/pdf", "reporte-cxc-vencidas.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Success = false, Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/v1/reportes/ventas/{ventaId}/pdf
    /// Genera el PDF comercial de una venta (NOTA DE VENTA).
    /// </summary>
    [HttpGet("api/v{version:apiVersion}/reportes/ventas/{ventaId:long}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVentaPdf(long ventaId, CancellationToken ct)
    {
        try
        {
            var pdf = await _pdfReporteService.GenerarVentaPdfAsync(ventaId, ct);
            return File(pdf, "application/pdf", $"venta-{ventaId:D6}.pdf");
        }
        catch (NotImplementedException)
        {
            return NotFound(new { Success = false, Message = "PDF de venta pendiente de implementación de template." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Success = false, Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
}
