namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public class UomDto
{
    public int UomId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class CreateUomDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class UpdateUomDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

public record ProductUomDto(
    long ProductUomId,
    long ProductId,
    int UomId,
    string UomCode,
    string UomName,
    bool IsBase,
    decimal FactorToBase,
    string? Barcode);

public record CreateProductUomDto(
    long ProductId,
    int UomId,
    bool IsBase,
    decimal FactorToBase,
    string? Barcode = null);
