namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public record CategoryDto(
    long CategoryId,
    long CatalogId,
    long? ParentCategoryId,
    string Name,
    string? Path,
    int SortOrder,
    bool Activo);

public class CreateCategoryDto
{
    public long CatalogId { get; set; }
    public long? ParentCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class UpdateCategoryDto
{
    public long? ParentCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool Activo { get; set; } = true;
}
