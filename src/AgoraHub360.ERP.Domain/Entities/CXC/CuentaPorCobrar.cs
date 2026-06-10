using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Enums;

namespace AgoraHub360.ERP.Domain.Entities.CXC;

/// <summary>
/// Cuenta por Cobrar — representa el saldo pendiente de cobro de una factura de venta.
/// Esquema: cxc.CuentasPorCobrar
/// </summary>
public sealed class CuentaPorCobrar : TenantEntity
{
    public long Id { get; set; }

    // ── Vínculo con factura de venta ──────────────────────────────────────────
    public long FacturaVentaId { get; set; }
    public FacturaVenta? FacturaVenta { get; set; }

    // ── Cliente (denormalizado para consultas rápidas) ─────────────────────────
    public int? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public string? ClienteNit { get; set; }

    // ── Datos de la factura (denormalizados) ────────────────────────────────────
    public string NumeroFactura { get; set; } = string.Empty;
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
