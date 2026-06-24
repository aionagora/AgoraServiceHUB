using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Enums;

namespace AgoraHub360.ERP.Domain.Entities.CXC;

/// <summary>
/// Cuenta por Cobrar — representa el saldo pendiente de cobro.
/// Puede originarse desde una venta confirmada (TipoDocumentoOrigen = "Venta")
/// o desde una factura fiscal (TipoDocumentoOrigen = "Factura").
/// Esquema: cxc.CuentasPorCobrar
/// </summary>
public sealed class CuentaPorCobrar : TenantEntity
{
    public long Id { get; set; }

    // ── Vínculo con documento origen ───────────────────────────────────────────
    /// <summary>Id del documento que origina la CxC (FacturaVenta o Venta).</summary>
    public long? FacturaVentaId { get; set; }
    public FacturaVenta? FacturaVenta { get; set; }

    /// <summary>Id de la venta que origina la CxC (directa o vía factura).</summary>
    public long? VentaId { get; set; }
    public Venta? Venta { get; set; }

    /// <summary>
    /// Tipo de documento origen: "Factura" (vía factura fiscal) o "Venta" (vía venta directa).
    /// </summary>
    public string? TipoDocumentoOrigen { get; set; }

    // ── Cliente (denormalizado para consultas rápidas) ─────────────────────────
    public int? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public string? ClienteNit { get; set; }

    // ── Datos del documento (denormalizados) ────────────────────────────────────
    public string? NumeroFactura { get; set; }
    public string? NumeroVenta { get; set; }
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }

    public string? MonedaCodigo { get; set; }
    public decimal TipoCambio { get; set; } = 1m;

    // ── Montos ────────────────────────────────────────────────────────────────
    public decimal TotalFactura { get; set; }
    public decimal TotalPagado { get; set; }
    private decimal? _saldoPendiente;
    public decimal SaldoPendiente
    {
        get => _saldoPendiente ?? (TotalFactura - TotalPagado);
        private set => _saldoPendiente = value;
    }

    // ── Estado ────────────────────────────────────────────────────────────────
    public EstadoCuentaPorCobrar Estado { get; set; } = EstadoCuentaPorCobrar.Pendiente;

    // ── Cálculo de días de mora (solo lectura en BD) ─────────────────────────
    // public int? DiasMora => FechaVencimiento.HasValue && SaldoPendiente > 0
    //     ? (int)(DateTime.UtcNow.Date - FechaVencimiento.Value.Date).TotalDays
    //     : null;
}
