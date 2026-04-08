namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Catalog — tenant-scoped.
/// Each company has its own catalogs. Scope field is kept for backward compat.
/// </summary>
public class Catalog : TenantEntity
{
    public long CatalogId { get; set; }

    /// <summary>1=General, 2=Specific. All catalogs belong to the company now.</summary>
    public byte Scope { get; set; } = 1;

    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
