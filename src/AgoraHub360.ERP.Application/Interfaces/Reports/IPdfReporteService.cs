using AgoraHub360.ERP.Shared.DTOs.Reportes;

namespace AgoraHub360.ERP.Application.Interfaces.Reports;

/// <summary>
/// Servicio de generación de reportes PDF para Cuentas por Cobrar, facturación y ventas.
/// </summary>
public interface IPdfReporteService
{
    /// <summary>
    /// Genera el recibo de pago PDF para un pago específico.
    /// </summary>
    Task<byte[]> GenerarReciboPagoAsync(long pagoId, CancellationToken ct = default);

    /// <summary>
    /// Genera el estado de cuenta PDF de un cliente.
    /// </summary>
    Task<byte[]> GenerarEstadoCuentaClienteAsync(
        int clienteId,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        bool soloPendientes = false,
        bool incluirPagos = false,
        CancellationToken ct = default);

    /// <summary>
    /// Genera la factura PDF.
    /// </summary>
    Task<byte[]> GenerarFacturaPdfAsync(long facturaId, CancellationToken ct = default);

    /// <summary>
    /// Genera el reporte general de CxC PDF.
    /// </summary>
    Task<byte[]> GenerarReporteCxcGeneralAsync(
        int? clienteId = null,
        string? estado = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        bool? vencidas = null,
        bool? conSaldo = null,
        CancellationToken ct = default);

    /// <summary>
    /// Genera el reporte de CxC vencidas PDF.
    /// </summary>
    Task<byte[]> GenerarReporteCxcVencidasAsync(CancellationToken ct = default);

    /// <summary>
    /// Genera el PDF de una venta comercial (NOTA DE VENTA).
    /// </summary>
    Task<byte[]> GenerarVentaPdfAsync(long ventaId, CancellationToken ct = default);
}
