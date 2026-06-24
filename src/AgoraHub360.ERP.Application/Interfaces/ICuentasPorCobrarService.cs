using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.CxC;

namespace AgoraHub360.ERP.Application.Interfaces;

/// <summary>
/// Servicio de Cuentas por Cobrar.
/// Gestiona la cartera de facturas pendientes de cobro.
/// </summary>
public interface ICuentasPorCobrarService
{
    /// <summary>
    /// Obtiene todas las cuentas por cobrar de la empresa activa, opcionalmente filtradas.
    /// </summary>
    Task<Result<PaginatedResultDto<CuentaPorCobrarResumenDto>>> GetAllAsync(CuentaPorCobrarFilterDto? filter = null, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el detalle de una cuenta por cobrar específica, incluyendo pagos aplicados.
    /// </summary>
    Task<Result<CuentaPorCobrarDetalleDto>> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Genera o actualiza la cuenta por cobrar a partir de una factura de venta.
    /// Se invoca automáticamente al facturar una venta.
    /// </summary>
    Task<Result<CuentaPorCobrarResumenDto>> GenerarDesdeFacturaAsync(long facturaVentaId, CancellationToken ct = default);

    /// <summary>
    /// Genera una cuenta por cobrar a partir de una venta confirmada, sin requerir factura fiscal.
    /// Se invoca automáticamente al confirmar una venta.
    /// Si ya existe una CxC activa para la venta, retorna la existente sin duplicar.
    /// </summary>
    Task<Result<CuentaPorCobrarResumenDto>> GenerarDesdeVentaAsync(long ventaId, CancellationToken ct = default);

    /// <summary>
    /// Actualiza el saldo pagado y estado de la cuenta por cobrar cuando se registra un pago.
    /// Se invoca automáticamente desde el registro de pago.
    /// </summary>
    Task<Result<CuentaPorCobrarResumenDto>> ActualizarPorPagoAsync(long facturaVentaId, decimal montoPagado, CancellationToken ct = default);

    /// <summary>
    /// Actualiza la cuenta por cobrar a partir del Id de venta.
    /// Busca la factura asociada y recalcula el saldo pagado con todos los pagos activos de la venta.
    /// </summary>
    Task<Result<CuentaPorCobrarResumenDto>> ActualizarPorPagoVentaAsync(long ventaId, CancellationToken ct = default);

    /// <summary>
    /// Anula la cuenta por cobrar (cuando se anula la factura).
    /// </summary>
    Task<Result<bool>> AnularAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el saldo total pendiente de la empresa activa.
    /// </summary>
    Task<Result<decimal>> GetSaldoTotalPendienteAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene el reporte de antigüedad de saldos de la empresa activa.
    /// </summary>
    Task<Result<AntiguedadSaldosResumenDto>> GetAntiguedadSaldosAsync(CancellationToken ct = default);
}
