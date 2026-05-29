using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Enums;

namespace AgoraHub360.ERP.Domain.Entities.VTA;

public class VentaDetalle : TenantEntity
{
    public long Id { get; set; }

    public long VentaId { get; set; }
    public Venta? Venta { get; set; }

    public TipoItemVenta TipoItemVenta { get; set; }

    public long? CompanyProductId { get; set; }
    public CompanyProduct? CompanyProduct { get; set; }

    public int? AlmacenId { get; set; }
    public Almacen? Almacen { get; set; }

    public string Descripcion { get; set; } = string.Empty;
    public string? DetalleAdicional { get; set; }

    public decimal Cantidad { get; set; }

    public int? UnidadMedidaId { get; set; }
    public Uom? UnidadMedida { get; set; }

    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoPorcentaje { get; set; }
    public decimal DescuentoMonto { get; set; }
    public decimal ImpuestoMonto { get; set; }
    public decimal TotalLinea { get; set; }

    public decimal? CostoUnitario { get; set; }

    public bool DescuentaInventario { get; set; } = false;
}
