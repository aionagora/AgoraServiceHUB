namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public record ProductDto2(
    long ProductId,
    long CatalogId,
    string CatalogNombre,
    byte ProductKind,
    string NombreGenerico,
    string NombreComercial,
    string? DescripcionCorta,
    string? DescripcionLarga,
    long? BrandId,
    string? BrandNombre,
    long? ManufacturerId,
    string? ManufacturerNombre,
    int DefaultUomId,
    string DefaultUomCode,
    bool IsStockable,
    bool IsSellable,
    bool IsPurchasable,
    int LifecycleStatusId,
    string LifecycleStatusCode,
    bool Activo);

public record CreateProductDto2(
    long CatalogId,
    byte ProductKind,
    string NombreGenerico,
    string NombreComercial,
    string? DescripcionCorta,
    string? DescripcionLarga,
    long? BrandId,
    long? ManufacturerId,
    int DefaultUomId,
    bool IsStockable = true,
    bool IsSellable = true,
    bool IsPurchasable = true,
    int LifecycleStatusId = 0);

public record UpdateProductDto2(
    byte ProductKind,
    string NombreGenerico,
    string NombreComercial,
    string? DescripcionCorta,
    string? DescripcionLarga,
    long? BrandId,
    long? ManufacturerId,
    int DefaultUomId,
    bool IsStockable,
    bool IsSellable,
    bool IsPurchasable,
    int LifecycleStatusId,
    bool Activo);
