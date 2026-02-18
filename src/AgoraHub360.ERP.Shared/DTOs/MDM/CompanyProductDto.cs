namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public record CompanyProductDto(
    long CompanyProductId,
    int EmpresaId,
    long ProductId,
    string NombreComercial,
    string Sku,
    string? CodigoInterno,
    bool IsVisiblePOS,
    bool IsVisibleEcommerce,
    bool IsVisibleB2B,
    bool AllowReturns,
    int? WarrantyDays,
    decimal? MinStock,
    decimal? MaxStock,
    decimal? ReorderPoint,
    byte? CostingMethod,
    bool Activo);

public record CreateCompanyProductDto(
    long ProductId,
    string Sku,
    string? CodigoInterno = null,
    bool IsVisiblePOS = true,
    bool IsVisibleEcommerce = false,
    bool IsVisibleB2B = false,
    bool AllowReturns = true,
    int? WarrantyDays = null,
    decimal? MinStock = null,
    decimal? MaxStock = null,
    decimal? ReorderPoint = null,
    byte? CostingMethod = null);

public record UpdateCompanyProductDto(
    string Sku,
    string? CodigoInterno,
    bool IsVisiblePOS,
    bool IsVisibleEcommerce,
    bool IsVisibleB2B,
    bool AllowReturns,
    int? WarrantyDays,
    decimal? MinStock,
    decimal? MaxStock,
    decimal? ReorderPoint,
    byte? CostingMethod,
    bool Activo);
