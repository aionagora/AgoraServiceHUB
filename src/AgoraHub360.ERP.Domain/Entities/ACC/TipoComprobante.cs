namespace AgoraHub360.ERP.Domain.Entities.ACC;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Tipo de comprobante contable (Ingreso, Egreso, Traspaso, Diario, etc.).
/// </summary>
public class TipoComprobante : TenantEntity
{
    public int TipoComprobanteId { get; set; }

    /// <summary>Código corto (ej: "ING", "EGR", "TRA").</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Nombre del tipo (ej: "Comprobante de Ingreso").</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Prefijo para numeración (ej: "CI", "CE", "CT").</summary>
    public string Prefijo { get; set; } = string.Empty;

    /// <summary>Orden de visualización.</summary>
    public int Orden { get; set; }
}
