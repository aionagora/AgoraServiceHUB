namespace AgoraHub360.ERP.Domain.Entities.CMP;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;

/// <summary>
/// Línea de detalle de una Orden de Pedido.
/// </summary>
public class OrdenPedidoLinea : AuditableEntity
{
    public long OrdenPedidoLineaId { get; set; }
    public long OrdenPedidoId { get; set; }

    public int NumeroLinea { get; set; }

    public long CompanyProductId { get; set; }
    public CompanyProduct? CompanyProduct { get; set; }

    public string Descripcion { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = "UND";

    /// <summary>Cantidad solicitada.</summary>
    public decimal CantidadSolicitada { get; set; }

    /// <summary>Cantidad disponible en stock al momento de la revisión.</summary>
    public decimal? CantidadStockDisponible { get; set; }

    /// <summary>Cantidad en tránsito (OCs aprobadas pendientes de recepción).</summary>
    public decimal? CantidadEnTransito { get; set; }

    /// <summary>Cantidad que se requiere comprar (CantidadSolicitada - Stock - Tránsito, ? 0).</summary>
    public decimal? CantidadAComprar { get; set; }

    public string? Notas { get; set; }

    // Navegación
    public OrdenPedido? OrdenPedido { get; set; }

    /// <summary>Calcula la cantidad a comprar luego de la revisión de stock.</summary>
    public void CalcularCantidadAComprar()
    {
        var disponible = (CantidadStockDisponible ?? 0) + (CantidadEnTransito ?? 0);
        CantidadAComprar = Math.Max(0, CantidadSolicitada - disponible);
    }
}
