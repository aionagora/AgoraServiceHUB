namespace AgoraHub360.ERP.Domain.Entities.ACC;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.CST;

/// <summary>
/// Línea de un asiento contable.
/// Cada línea afecta una cuenta con un monto al Debe o al Haber.
/// </summary>
public class AsientoContableLinea : AuditableEntity
{
    public long AsientoContableLineaId { get; set; }
    public long AsientoContableId { get; set; }

    /// <summary>Número de línea secuencial.</summary>
    public int NumeroLinea { get; set; }

    /// <summary>Cuenta contable afectada.</summary>
    public int CuentaContableId { get; set; }
    public CuentaContable? CuentaContable { get; set; }

    /// <summary>Monto al Debe (debit).</summary>
    public decimal Debe { get; set; }

    /// <summary>Monto al Haber (credit).</summary>
    public decimal Haber { get; set; }

    /// <summary>Glosa/detalle de la línea.</summary>
    public string? Glosa { get; set; }

    /// <summary>Referencia adicional (ej: número de factura, proveedor).</summary>
    public string? Referencia { get; set; }

    /// <summary>Centro de costo analítico opcional para la línea.</summary>
    public int? CentroCostoId { get; set; }
    public CentroCosto? CentroCosto { get; set; }

    // Navegación
    public AsientoContable? AsientoContable { get; set; }
}
