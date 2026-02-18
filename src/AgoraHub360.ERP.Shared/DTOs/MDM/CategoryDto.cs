namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public record CategoryDto(
    long CategoryId,
    long CatalogId,
    long? ParentCategoryId,
    string Nombre,
    string? Path,
    int SortOrder,
    bool Activo);

public record CreateCategoryDto(
    long CatalogId,
    long? ParentCategoryId,
    string Nombre,
    int SortOrder = 0);

public record UpdateCategoryDto(
    long? ParentCategoryId,
    string Nombre,
    int SortOrder,
    bool Activo);
