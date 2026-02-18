namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Categoría del catálogo con soporte jerárquico (árbol N niveles).
/// Un producto puede pertenecer a múltiples categorías (N:N).
/// </summary>
public class Category : AuditableEntity
{
    public long CategoryId { get; set; }
    public long CatalogId { get; set; }
    public long? ParentCategoryId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Path materializado para búsquedas rápidas: "/Electrónica/Celulares".</summary>
    public string? Path { get; set; }

    public int SortOrder { get; set; }

    // Navegación
    public Catalog Catalog { get; set; } = null!;
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}
