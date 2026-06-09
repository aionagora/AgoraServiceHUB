namespace AgoraHub360.ERP.Shared.DTOs.CxC;

/// <summary>
/// Resumen de una cuenta por cobrar para listados y reportes rápidos.
/// </summary>
public class CuentaPorCobrarResumenDto
{
    public long Id { get; set; }

    public long FacturaVentaId { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string? NumeroVenta { get; set; }

    public int? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public string? ClienteNit { get; set; }

    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }

    public string? MonedaCodigo { get; set; }
    public decimal TipoCambio { get; set; }

    public decimal TotalFactura { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }

    public string Estado { get; set; } = string.Empty;
}

/// <summary>
/// Detalle completo de una cuenta por cobrar, incluyendo los pagos aplicados.
/// </summary>
public class CuentaPorCobrarDetalleDto
{
    public long Id { get; set; }

    public long FacturaVentaId { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string? NumeroVenta { get; set; }

    public int? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public string? ClienteNit { get; set; }

    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }

    public string? MonedaCodigo { get; set; }
    public decimal TipoCambio { get; set; }

    public decimal TotalFactura { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }

    public string Estado { get; set; } = string.Empty;

    public List<PagoAplicadoDto> Pagos { get; set; } = new();
}

/// <summary>
/// Pago aplicado a una cuenta por cobrar.
/// </summary>
public class PagoAplicadoDto
{
    public long PagoId { get; set; }
    public DateTime FechaPago { get; set; }
    public string TipoPago { get; set; } = string.Empty;
    public string ModoPago { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? Referencia { get; set; }
    public string EstadoPago { get; set; } = string.Empty;
}

/// <summary>
/// Filtro para consultar cuentas por cobrar.
/// </summary>
public class CuentaPorCobrarFilterDto
{
    public int? ClienteId { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public DateTime? FechaVencimientoDesde { get; set; }
    public DateTime? FechaVencimientoHasta { get; set; }
}
