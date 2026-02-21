namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Catalog. Can be global (Scope=1) or company-specific (Scope=2).
/// All products belong to a catalog.
/// </summary>
public class Catalog : AuditableEntity
{
    public long CatalogId { get; set; }

    /// <summary>1=Global, 2=Company</summary>
    public byte Scope { get; set; } = 1;

    /// <summary>Only applies when Scope=Company.</summary>
    public int? EmpresaId { get; set; }

    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
