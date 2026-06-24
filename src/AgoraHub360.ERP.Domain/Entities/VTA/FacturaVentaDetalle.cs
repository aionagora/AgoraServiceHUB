using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Enums;

namespace AgoraHub360.ERP.Domain.Entities.VTA;

public class FacturaVentaDetalle : TenantEntity
{
    public long Id { get; set; }

    public long FacturaVentaId { get; set; }
    public FacturaVenta? FacturaVenta { get; set; }

    public long? VentaDetalleId { get; set; }
    public VentaDetalle? VentaDetalle { get; set; }

    public TipoItemVenta TipoItemVenta { get; set; }

    public string? ItemCode { get; set; }
    public string? CodigoProducto { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? DetalleAdicional { get; set; }

    public decimal Cantidad { get; set; }
    public string? UnidadMedida { get; set; }

    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoMonto { get; set; }
    public decimal ImpuestoMonto { get; set; }
    public decimal TotalLinea { get; set; }
}
