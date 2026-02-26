namespace AgoraHub360.ERP.Domain.Entities.MDM;

/// <summary>
/// Feature activable por empresa+producto, evaluado por RulesEngine o manualmente.
/// FeatureCode: LOT, SERIAL, FEFO, HAZMAT, QC, BOM, EXPIRY, ALLERGEN, etc.
/// </summary>
public class CompanyProductFeature
{
    public int EmpresaId { get; set; }
    public long CompanyProductId { get; set; }
    public string FeatureCode { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }

    // Navegación
    public CompanyProduct CompanyProduct { get; set; } = null!;
}
