namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public record ProductDto2(
    long ProductId,
    long CatalogId,
    string CatalogName,
    byte ProductKind,
    string GenericName,
    string CommercialName,
    string? ShortDescription,
    string? LongDescription,
    long? BrandId,
    string? BrandName,
    long? ManufacturerId,
    string? ManufacturerName,
    int DefaultUomId,
    string DefaultUomCode,
    bool IsStockable,
    bool IsSellable,
    bool IsPurchasable,
    int LifecycleStatusId,
    string LifecycleStatusCode,
    bool Activo);

public class CreateProductDto2
{
    public long CatalogId { get; set; }
    public byte ProductKind { get; set; } = 1;
    public string GenericName { get; set; } = string.Empty;
    public string CommercialName { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public long? BrandId { get; set; }
    public long? ManufacturerId { get; set; }
    public int DefaultUomId { get; set; }
    public bool IsStockable { get; set; } = true;
    public bool IsSellable { get; set; } = true;
    public bool IsPurchasable { get; set; } = true;
    public int LifecycleStatusId { get; set; }
}

public class UpdateProductDto2
{
    public byte ProductKind { get; set; } = 1;
    public string GenericName { get; set; } = string.Empty;
    public string CommercialName { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public long? BrandId { get; set; }
    public long? ManufacturerId { get; set; }
    public int DefaultUomId { get; set; }
    public bool IsStockable { get; set; } = true;
    public bool IsSellable { get; set; } = true;
    public bool IsPurchasable { get; set; } = true;
    public int LifecycleStatusId { get; set; }
    public bool Activo { get; set; } = true;
}
