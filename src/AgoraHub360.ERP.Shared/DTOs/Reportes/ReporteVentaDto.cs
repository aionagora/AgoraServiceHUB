namespace AgoraHub360.ERP.Shared.DTOs.Reportes;

public class ReporteVentaDto
{
    // Empresa
    public string EmpresaNombre { get; set; } = string.Empty;
    public string? EmpresaNit { get; set; }
    public string? EmpresaDireccion { get; set; }
    public string? EmpresaTelefono { get; set; }
    public string? EmpresaEmail { get; set; }

    // Venta
    public string NumeroVenta { get; set; } = string.Empty;
    public DateTime FechaVenta { get; set; }
    public DateTime? FechaVencimientoPago { get; set; }
    public string MonedaCodigo { get; set; } = "BOB";
    public decimal TipoCambio { get; set; } = 1m;
    public string? SucursalNombre { get; set; }
    public string? AlmacenNombre { get; set; }
    public string? VendedorNombre { get; set; }
    public string? Origen { get; set; }
    public long? PedidoVentaId { get; set; }

    // Cliente
    public string ClienteNombre { get; set; } = "-";
    public string ClienteNit { get; set; } = "-";
    public string? ClienteDireccion { get; set; }
    public string? ClienteTelefono { get; set; }

    // Detalle
    public List<ReporteVentaItemDto> Items { get; set; } = new();

    // Totales
    public decimal Subtotal { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal Total { get; set; }

    // Otros
    public string? Observaciones { get; set; }
}

public class ReporteVentaItemDto
{
    public int Item { get; set; }
    public string Sku { get; set; } = "N/D";
    public string Descripcion { get; set; } = string.Empty;
    public string? DetalleAdicional { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoPorcentaje { get; set; }
    public decimal DescuentoMonto { get; set; }
    public decimal TotalLinea { get; set; }
}
