using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;

namespace AgoraHub360.ERP.Domain.Entities.VTA;

/// <summary>
/// Detail line representing a product in a Sales Order.
/// </summary>
public class PedidoVentaDetalle : TenantEntity
{
    public long Id { get; set; }

    public long PedidoVentaId { get; set; }
    public PedidoVenta? PedidoVenta { get; set; }

    public long CompanyProductId { get; set; }
    public CompanyProduct? CompanyProduct { get; set; }

    public decimal CantidadSolicitada { get; set; }
    public decimal CantidadReservada { get; set; } = 0m;
    public decimal CantidadConfirmada { get; set; }
    public decimal CantidadDespachada { get; set; }

    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }

    public string? Observaciones { get; set; }
}
