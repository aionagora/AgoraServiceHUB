namespace AgoraHub360.ERP.Domain.Entities.CMP;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Línea de recepción: cantidad efectivamente recibida de una línea de OC.
/// </summary>
public class RecepcionCompraLinea : AuditableEntity
{
    public long RecepcionCompraLineaId { get; set; }
    public long RecepcionCompraId { get; set; }
    public long OrdenCompraLineaId { get; set; }

    /// <summary>Cantidad recibida en esta recepción.</summary>
    public decimal CantidadRecibida { get; set; }

    /// <summary>Costo unitario de recepción (por defecto el de la OC, puede ajustarse).</summary>
    public decimal CostoUnitario { get; set; }

    /// <summary>Notas de la línea (ej: "2 unidades dañadas").</summary>
    public string? Notas { get; set; }

    // Navegación
    public RecepcionCompra? RecepcionCompra { get; set; }
    public OrdenCompraLinea? OrdenCompraLinea { get; set; }
}
