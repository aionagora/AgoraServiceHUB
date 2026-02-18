namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Catálogo de productos. Puede ser global (Scope=1) o específico de empresa (Scope=2).
/// Todos los productos pertenecen a un catálogo.
/// </summary>
public class Catalog : AuditableEntity
{
    public long CatalogId { get; set; }

    /// <summary>1=Global, 2=Empresa</summary>
    public byte Scope { get; set; } = 1;

    /// <summary>Solo aplica cuando Scope=Empresa.</summary>
    public int? EmpresaId { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    // Navegación
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
