namespace AgoraHub360.ERP.Domain.Entities.ACC;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Línea de una plantilla de contabilización.
/// Define qué cuenta afectar, si al Debe o al Haber, y qué campo del documento usar como monto.
/// </summary>
public class PlantillaContableLinea : AuditableEntity
{
    public int PlantillaContableLineaId { get; set; }
    public int PlantillaContableId { get; set; }

    /// <summary>Orden de la línea.</summary>
    public int NumeroLinea { get; set; }

    /// <summary>Cuenta contable a afectar.</summary>
    public int CuentaContableId { get; set; }
    public CuentaContable? CuentaContable { get; set; }

    /// <summary>Tipo de movimiento: "Debe" o "Haber".</summary>
    public string TipoMovimiento { get; set; } = "Debe";

    /// <summary>
    /// Campo del documento origen a usar como monto.
    /// Valores: "Subtotal", "Impuesto", "Total", "GastoAsignado", "CostoTotal", "Cantidad".
    /// </summary>
    public string CampoMonto { get; set; } = "Total";

    /// <summary>Factor multiplicador (ej: 1.0 para usar el monto tal cual, 0.13 para IVA 13%).</summary>
    public decimal Factor { get; set; } = 1m;

    /// <summary>Glosa de la línea. Soporta tokens: {CuentaCodigo}, {CuentaNombre}.</summary>
    public string? Glosa { get; set; }

    // Navegación
    public PlantillaContable? PlantillaContable { get; set; }
}
