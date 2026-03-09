namespace AgoraHub360.ERP.Domain.Entities.CMP;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Línea de recepción: cantidad efectivamente recibida de una línea de OC.
/// Incluye campos de control de diferencias por línea.
/// </summary>
public class RecepcionCompraLinea : AuditableEntity
{
    public long RecepcionCompraLineaId { get; set; }
    public long RecepcionCompraId { get; set; }
    public long OrdenCompraLineaId { get; set; }

    /// <summary>Cantidad recibida en buen estado en esta recepción.</summary>
    public decimal CantidadRecibida { get; set; }

    /// <summary>Cantidad con daños detectados en esta recepción.</summary>
    public decimal CantidadDañada { get; set; }

    /// <summary>Cantidad sobrante respecto al packing list.</summary>
    public decimal CantidadSobrante { get; set; }

    /// <summary>Cantidad faltante respecto al packing list / OC.</summary>
    public decimal CantidadFaltante { get; set; }

    /// <summary>Costo unitario de recepción (por defecto el de la OC, puede ajustarse).</summary>
    public decimal CostoUnitario { get; set; }

    /// <summary>Notas de la línea (ej: "2 unidades dañadas, embalaje roto").</summary>
    public string? Notas { get; set; }

    /// <summary>
    /// Cantidad que efectivamente ingresa al stock aceptable
    /// (CantidadRecibida, excluye dañadas en cuarentena).
    /// </summary>
    public decimal CantidadAceptada => CantidadRecibida - CantidadDañada;

    // Navegación
    public RecepcionCompra? RecepcionCompra { get; set; }
    public OrdenCompraLinea? OrdenCompraLinea { get; set; }
}
