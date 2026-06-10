namespace AgoraHub360.ERP.Domain.Enums;

/// <summary>
/// Estado de la cuenta por cobrar.
/// Pendiente: Saldo > 0, no vencida.
/// Vencida: Saldo > 0, fecha vencimiento pasada.
/// Parcial: Pagos recibidos pero saldo > 0.
/// Pagada: Saldo = 0.
/// Anulada: Factura/comprobante anulado.
/// </summary>
public enum EstadoCuentaPorCobrar
{
    Pendiente = 1,
    Parcial = 2,
    Pagada = 3,
    Vencida = 4,
    Anulada = 5
}
