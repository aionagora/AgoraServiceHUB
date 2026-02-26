namespace AgoraHub360.ERP.Domain.Entities.ACC;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Asiento contable (journal entry).
/// Cabecera del comprobante con líneas Debe/Haber que deben cuadrar en partida doble.
/// </summary>
public class AsientoContable : TenantEntity
{
    public long AsientoContableId { get; set; }

    /// <summary>Número del asiento (ej: AST-2025-00001).</summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>Fecha contable del asiento.</summary>
    public DateTime Fecha { get; set; }

    /// <summary>Tipo: Manual, Automático.</summary>
    public string Tipo { get; set; } = "Manual";

    /// <summary>Glosa/Concepto general del asiento.</summary>
    public string Glosa { get; set; } = string.Empty;

    /// <summary>Estado: Borrador, Contabilizado, Anulado.</summary>
    public string Estado { get; set; } = "Borrador";

    /// <summary>Tipo de documento origen (ej: Recepción, Importación, Venta, Ajuste).</summary>
    public string? OrigenTipo { get; set; }

    /// <summary>Id del documento origen (ej: RecepcionCompraId, HojaImportacionId).</summary>
    public long? OrigenId { get; set; }

    /// <summary>Referencia al documento origen (ej: REC-000001).</summary>
    public string? OrigenReferencia { get; set; }

    /// <summary>Total Debe (calculado).</summary>
    public decimal TotalDebe { get; set; }

    /// <summary>Total Haber (calculado).</summary>
    public decimal TotalHaber { get; set; }

    // Navegación
    public ICollection<AsientoContableLinea> Lineas { get; set; } = new List<AsientoContableLinea>();
}
