using AgoraHub360.ERP.Domain.Common;

namespace AgoraHub360.ERP.Domain.Entities.VTA;

/// <summary>
/// Detalle de línea de una factura importada manualmente desde XML.
/// </summary>
public class FacturaVentaXmlDetalle : TenantEntity
{
    public long Id { get; set; }

    public long FacturaVentaXmlId { get; set; }
    public FacturaVentaXml? FacturaVentaXml { get; set; }

    public string? ActividadEconomica { get; set; }
    public string? CodigoProductoSin { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public int UnidadMedida { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal MontoDescuento { get; set; }
    public decimal SubTotal { get; set; }
}
