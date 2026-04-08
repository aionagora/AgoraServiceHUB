namespace AgoraHub360.ERP.Domain.Entities.CMP;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;

/// <summary>
/// Línea de detalle de una Orden de Compra.
/// Cada línea referencia un CompanyProduct con cantidad, precio, descuento e impuesto.
/// </summary>
public class OrdenCompraLinea : AuditableEntity
{
    public long OrdenCompraLineaId { get; set; }
    public long OrdenCompraId { get; set; }

    /// <summary>Número de línea dentro de la OC (1, 2, 3…).</summary>
    public int NumeroLinea { get; set; }

    public long CompanyProductId { get; set; }
    public CompanyProduct? CompanyProduct { get; set; }

    /// <summary>Descripción libre (puede diferir del nombre del producto).</summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Unidad de medida de compra.</summary>
    public string UnidadMedida { get; set; } = "UND";

    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    /// <summary>Porcentaje de descuento de línea (0–100).</summary>
    public decimal PorcentajeDescuento { get; set; }

    /// <summary>Monto de descuento calculado: Cantidad * PrecioUnitario * %Desc / 100.</summary>
    public decimal MontoDescuento { get; set; }

    /// <summary>Subtotal antes de impuesto: (Cantidad * PrecioUnitario) - MontoDescuento.</summary>
    public decimal Subtotal { get; set; }

    /// <summary>Porcentaje de impuesto (ej: 13 para IVA 13%).</summary>
    public decimal PorcentajeImpuesto { get; set; }

    /// <summary>Monto de impuesto calculado sobre el Subtotal.</summary>
    public decimal MontoImpuesto { get; set; }

    /// <summary>Total de la línea: Subtotal + MontoImpuesto.</summary>
    public decimal TotalLinea { get; set; }

    /// <summary>Cantidad ya recepcionada. Se actualiza al crear recepciones.</summary>
    public decimal CantidadRecepcionada { get; set; }

    /// <summary>True si la línea está totalmente recepcionada.</summary>
    public bool RecepcionCompleta => CantidadRecepcionada >= Cantidad;

    /// <summary>Cantidad pendiente de recepcionar.</summary>
    public decimal CantidadPendiente => Cantidad - CantidadRecepcionada;

    // Navegación
    public OrdenCompra? OrdenCompra { get; set; }

    /// <summary>Recalcula montos de la línea.</summary>
    public void Recalcular()
    {
        var bruto = Cantidad * PrecioUnitario;
        MontoDescuento = bruto * PorcentajeDescuento / 100m;
        Subtotal = bruto - MontoDescuento;
        MontoImpuesto = Subtotal * PorcentajeImpuesto / 100m;
        TotalLinea = Subtotal + MontoImpuesto;
    }
}
