namespace AgoraHub360.ERP.Shared.DTOs.Reportes;

/// <summary>
/// Datos para la generación de factura PDF.
/// </summary>
public class FacturaPdfDto
{
    // Empresa
    public string EmpresaNombre { get; set; } = string.Empty;
    public string? EmpresaNit { get; set; }
    public string? EmpresaDireccion { get; set; }
    public string? EmpresaTelefono { get; set; }
    public string? EmpresaEmail { get; set; }

    // Cliente
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteNit { get; set; } = string.Empty;
    public string? ClienteDireccion { get; set; }

    // Factura
    public string NumeroFactura { get; set; } = string.Empty;
    public string? NumeroAutorizacion { get; set; }
    public DateTime FechaEmision { get; set; }
    public string? MonedaCodigo { get; set; }
    public decimal TipoCambio { get; set; } = 1m;

    // Montos
    public decimal Subtotal { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal ImpuestoTotal { get; set; }
    public decimal Total { get; set; }

    // Estado
    public string EstadoFactura { get; set; } = string.Empty;
    public string? Observaciones { get; set; }

    // SIAT
    public string? Cuf { get; set; }
    public string? Cufd { get; set; }
    public string? Leyenda { get; set; }
    public string? SiatQr { get; set; }
    public string EstadoSiat { get; set; } = string.Empty;

    // Detalles
    public List<FacturaPdfDetalleDto> Detalles { get; set; } = new();
}

public class FacturaPdfDetalleDto
{
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoMonto { get; set; }
    public decimal ImpuestoMonto { get; set; }
    public decimal TotalLinea { get; set; }
}
