namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Hierarchical category with N-level tree support — tenant-scoped.
/// A product can belong to multiple categories (N:N via ProductCategory).
/// </summary>
public class Category : TenantEntity
{
    public long CategoryId { get; set; }
    public long CatalogId { get; set; }
    public long? ParentCategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Materialized path for fast queries: "/Electronics/Phones".</summary>
    public string? Path { get; set; }

    public int SortOrder { get; set; }

    // Navigation
    public Catalog Catalog { get; set; } = null!;
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}
