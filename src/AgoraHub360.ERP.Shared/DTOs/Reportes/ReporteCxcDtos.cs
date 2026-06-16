namespace AgoraHub360.ERP.Shared.DTOs.Reportes;

/// <summary>
/// Datos para el recibo de pago PDF.
/// </summary>
public class ReciboPagoDto
{
    public long PagoId { get; set; }
    public string NumeroRecibo { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; }

    // Empresa
    public string EmpresaNombre { get; set; } = string.Empty;
    public string? EmpresaNit { get; set; }
    public string? EmpresaDireccion { get; set; }
    public string? EmpresaTelefono { get; set; }
    public string? EmpresaEmail { get; set; }
    public string? SucursalNombre { get; set; }

    // Cliente
    public int? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public string? ClienteNit { get; set; }

    // Factura relacionada
    public long FacturaVentaId { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public long? CuentaPorCobrarId { get; set; }

    // Montos
    public decimal MontoPagado { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public decimal SaldoAnterior { get; set; }
    public decimal SaldoPosterior { get; set; }
    public string? MonedaCodigo { get; set; }
    public decimal TipoCambio { get; set; } = 1m;

    // Observaciones
    public string? Observaciones { get; set; }
    public string? UsuarioRegistro { get; set; }
}

/// <summary>
/// Datos para el estado de cuenta PDF.
/// </summary>
public class EstadoCuentaClienteDto
{
    // Empresa
    public string EmpresaNombre { get; set; } = string.Empty;
    public string? EmpresaNit { get; set; }
    public string? EmpresaDireccion { get; set; }
    public string? EmpresaTelefono { get; set; }
    public string? EmpresaEmail { get; set; }

    // Cliente
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string? ClienteNit { get; set; }
    public string? ClienteDireccion { get; set; }
    public string? ClienteTelefono { get; set; }

    // Rango consultado
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }

    // Resumen
    public decimal TotalFacturado { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public decimal SaldoVencido { get; set; }

    // Detalle
    public string? MonedaCodigo { get; set; }
    public List<EstadoCuentaDocumentoDto> Documentos { get; set; } = new();
    public List<EstadoCuentaPagoDto>? Pagos { get; set; }
}

public class EstadoCuentaDocumentoDto
{
    public long Id { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal Saldo { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int? DiasVencidos { get; set; }
}

public class EstadoCuentaPagoDto
{
    public long PagoId { get; set; }
    public DateTime FechaPago { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string? Referencia { get; set; }
}

/// <summary>
/// Datos para el reporte general de CxC PDF.
/// </summary>
public class ReporteCxcGeneralDto
{
    // Empresa
    public string EmpresaNombre { get; set; } = string.Empty;
    public string? EmpresaNit { get; set; }

    // Filtros aplicados
    public int? ClienteId { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }

    // Resumen de cartera
    public int TotalCuentas { get; set; }
    public int CuentasPendientes { get; set; }
    public int CuentasVencidas { get; set; }
    public int CuentasPagadas { get; set; }
    public decimal TotalSaldoPendiente { get; set; }

    // Detalle
    public string? MonedaCodigo { get; set; }
    public List<ReporteCxcItemDto> Items { get; set; } = new();
    public List<ReporteCxcTotalPorEstadoDto> TotalesPorEstado { get; set; } = new();
}

public class ReporteCxcItemDto
{
    public long Id { get; set; }
    public string? ClienteNombre { get; set; }
    public string? ClienteNit { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class ReporteCxcTotalPorEstadoDto
{
    public string Estado { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal TotalSaldo { get; set; }
}

/// <summary>
/// Datos para el reporte de CxC vencidas PDF.
/// </summary>
public class ReporteCxcVencidasDto
{
    // Empresa
    public string EmpresaNombre { get; set; } = string.Empty;
    public string? EmpresaNit { get; set; }

    // Resumen
    public int TotalVencidas { get; set; }
    public decimal TotalSaldoVencido { get; set; }
    public DateTime FechaCorte { get; set; }

    // Detalle
    public string? MonedaCodigo { get; set; }
    public List<ReporteCxcVencidaItemDto> Items { get; set; } = new();
}

public class ReporteCxcVencidaItemDto
{
    public long Id { get; set; }
    public string? ClienteNombre { get; set; }
    public string? ClienteNit { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int DiasVencidos { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string Estado { get; set; } = string.Empty;
}
