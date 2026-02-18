namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public record UomDto(
    int UomId,
    string Code,
    string Nombre,
    bool Activo);

public record CreateUomDto(string Code, string Nombre);
public record UpdateUomDto(string Code, string Nombre, bool Activo);

public record ProductUomDto(
    long ProductUomId,
    long ProductId,
    int UomId,
    string UomCode,
    string UomNombre,
    bool IsBase,
    decimal FactorToBase,
    string? Barcode);

public record CreateProductUomDto(
    long ProductId,
    int UomId,
    bool IsBase,
    decimal FactorToBase,
    string? Barcode = null);
