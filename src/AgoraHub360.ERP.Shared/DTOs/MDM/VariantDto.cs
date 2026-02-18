namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public record ProductVariantDto(
    long VariantId,
    int EmpresaId,
    long ParentProductId,
    string ParentNombreComercial,
    string Sku,
    string? Barcode,
    string? VariantName,
    bool Activo,
    IReadOnlyList<VariantAxisValueDto> AxisValues);

public record VariantAxisValueDto(
    long AttributeId,
    string AttributeName,
    long? OptionId,
    string? OptionValue,
    string? ValueString);

public record CreateProductVariantDto(
    long ParentProductId,
    string Sku,
    string? Barcode = null,
    IList<VariantAxisValueDto>? AxisValues = null);

public record UpdateProductVariantDto(
    string Sku,
    string? Barcode,
    string? VariantName,
    bool Activo,
    IList<VariantAxisValueDto>? AxisValues = null);
