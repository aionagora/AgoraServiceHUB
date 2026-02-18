namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Plantilla / producto base del catálogo maestro global.
/// No es tenant-aware: el vínculo con empresa se hace mediante CompanyProduct.
/// </summary>
public class Product : AuditableEntity
{
    public long ProductId { get; set; }
    public long CatalogId { get; set; }

    /// <summary>1=Item, 2=Servicio, 3=Kit, 4=MateriaPrima, 5=Empaque</summary>
    public byte ProductKind { get; set; } = 1;

    public string NombreGenerico { get; set; } = string.Empty;
    public string NombreComercial { get; set; } = string.Empty;
    public string? DescripcionCorta { get; set; }
    public string? DescripcionLarga { get; set; }

    public long? BrandId { get; set; }
    public long? ManufacturerId { get; set; }

    /// <summary>UoM base/predeterminada del producto.</summary>
    public int DefaultUomId { get; set; }

    public bool IsStockable { get; set; } = true;
    public bool IsSellable { get; set; } = true;
    public bool IsPurchasable { get; set; } = true;

    public int LifecycleStatusId { get; set; }

    // Navegación
    public Catalog Catalog { get; set; } = null!;
    public Brand? Brand { get; set; }
    public Manufacturer? Manufacturer { get; set; }
    public Uom DefaultUom { get; set; } = null!;
    public ProductStatus LifecycleStatus { get; set; } = null!;

    public ICollection<CompanyProduct> CompanyProducts { get; set; } = new List<CompanyProduct>();
    public ICollection<ProductCode> ProductCodes { get; set; } = new List<ProductCode>();
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    public ICollection<ProductClassificationLink> ClassificationLinks { get; set; } = new List<ProductClassificationLink>();
    public ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
    public ICollection<ProductUom> ProductUoms { get; set; } = new List<ProductUom>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}
