namespace AgoraHub360.ERP.Domain.Entities.CMP;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Pago programado/ejecutado asociado a una Orden de Compra.
/// Representa el paso [5]: anticipos y saldo final.
/// </summary>
public class PagoOrdenCompra : TenantEntity
{
    public long PagoOrdenCompraId { get; set; }

    public long OrdenCompraId { get; set; }
    public OrdenCompra? OrdenCompra { get; set; }

    /// <summary>Tipo de pago: Anticipo, Saldo, Total.</summary>
    public TipoPagoImportacion TipoPago { get; set; }

    /// <summary>Monto programado.</summary>
    public decimal MontoProgramado { get; set; }

    /// <summary>Moneda del pago.</summary>
    public string MonedaId { get; set; } = "BOB";

    /// <summary>Tasa de cambio al momento del pago.</summary>
    public decimal TasaCambio { get; set; } = 1;

    /// <summary>Fecha programada de pago.</summary>
    public DateTime FechaProgramada { get; set; }

    /// <summary>Fecha real de ejecución del pago.</summary>
    public DateTime? FechaEjecucion { get; set; }

    /// <summary>Monto efectivamente pagado.</summary>
    public decimal? MontoEjecutado { get; set; }

    /// <summary>True cuando el pago fue ejecutado.</summary>
    public bool Ejecutado { get; set; }

    /// <summary>Referencia bancaria / número de transferencia.</summary>
    public string? ReferenciaTransferencia { get; set; }

    public string? Observaciones { get; set; }
}
