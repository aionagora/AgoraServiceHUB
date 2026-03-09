namespace AgoraHub360.ERP.Shared.DTOs.Compras;

/// <summary>DTO de lectura para Confirmación del Proveedor (PI).</summary>
public record ConfirmacionProveedorDto(
    long ConfirmacionProveedorId,
    long OrdenCompraId,
    string OrdenCompraNumero,
    string? NumeroProforma,
    DateTime FechaConfirmacion,
    bool CondicionesOK,
    string? ObservacionesProveedor,
    DateTime? FechaEntregaComprometida,
    string Estado,
    string? Observaciones,
    int IteracionNegociacion);

/// <summary>DTO para registrar la confirmación o negociación del proveedor.</summary>
public record RegistrarConfirmacionProveedorDto(
    string? NumeroProforma,
    DateTime FechaConfirmacion,
    bool CondicionesOK,
    string? ObservacionesProveedor = null,
    DateTime? FechaEntregaComprometida = null,
    string? Observaciones = null);

/// <summary>DTO de lectura para Pago de Orden de Compra.</summary>
public record PagoOrdenCompraDto(
    long PagoOrdenCompraId,
    long OrdenCompraId,
    string OrdenCompraNumero,
    /// <summary>Anticipo | Saldo | Total</summary>
    string TipoPago,
    decimal MontoProgramado,
    string MonedaId,
    decimal TasaCambio,
    DateTime FechaProgramada,
    DateTime? FechaEjecucion,
    decimal? MontoEjecutado,
    bool Ejecutado,
    string? ReferenciaTransferencia,
    string? Observaciones);

/// <summary>DTO para programar un pago en la OC.</summary>
public record ProgramarPagoDto(
    /// <summary>Anticipo | Saldo | Total</summary>
    string TipoPago,
    decimal MontoProgramado,
    string MonedaId,
    decimal TasaCambio,
    DateTime FechaProgramada,
    string? Observaciones = null);

/// <summary>DTO para registrar la ejecución de un pago programado.</summary>
public record EjecutarPagoDto(
    DateTime FechaEjecucion,
    decimal MontoEjecutado,
    string ReferenciaTransferencia,
    string? Observaciones = null);
