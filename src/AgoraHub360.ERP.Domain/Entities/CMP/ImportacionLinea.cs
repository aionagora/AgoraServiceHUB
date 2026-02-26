namespace AgoraHub360.ERP.Domain.Entities.CMP;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Línea de distribución de la hoja de importación.
/// Cada línea corresponde a una línea de OC y muestra cómo se distribuyó el landed cost.
/// </summary>
public class ImportacionLinea : AuditableEntity
{
    public long ImportacionLineaId { get; set; }
    public long HojaImportacionId { get; set; }
    public long OrdenCompraLineaId { get; set; }

    /// <summary>Costo FOB unitario (precio de compra original).</summary>
    public decimal CostoFobUnitario { get; set; }

    /// <summary>Costo FOB total de la línea (CostoFobUnit * Cantidad).</summary>
    public decimal CostoFobTotal { get; set; }

    /// <summary>Factor de distribución usado (depende del método: valor, peso, etc.).</summary>
    public decimal FactorDistribucion { get; set; }

    /// <summary>Monto de gasto asignado a esta línea.</summary>
    public decimal GastoAsignado { get; set; }

    /// <summary>Costo Landed unitario: (CostoFobTotal + GastoAsignado) / Cantidad.</summary>
    public decimal CostoLandedUnitario { get; set; }

    /// <summary>Costo Landed total: CostoFobTotal + GastoAsignado.</summary>
    public decimal CostoLandedTotal { get; set; }

    // Navegación
    public HojaImportacion? HojaImportacion { get; set; }
    public OrdenCompraLinea? OrdenCompraLinea { get; set; }
}
