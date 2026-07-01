namespace AgoraHub360.ERP.Shared.DTOs.Ventas;

/// <summary>DTO de respuesta al importar una factura XML manualmente.</summary>
public sealed class FacturaVentaXmlImportadaDto
{
    public long Id { get; set; }
    public long VentaId { get; set; }
    public int NumeroFactura { get; set; }
    public string Cuf { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public string NombreRazonSocialCliente { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public decimal MontoTotal { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observacion { get; set; }
}

/// <summary>DTO de detalle de una factura XML importada.</summary>
public sealed class FacturaVentaXmlDetalleDto
{
    public string CodigoProducto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public int UnidadMedida { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal MontoDescuento { get; set; }
    public decimal SubTotal { get; set; }
}
