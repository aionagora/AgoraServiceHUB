namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public record CatalogDto(
    long CatalogId,
    int EmpresaId,
    byte Scope,
    string Name,
    bool IsDefault,
    bool Activo);

public record CreateCatalogDto(
    string Name,
    byte Scope = 1,
    bool IsDefault = false);

public record UpdateCatalogDto(
    string Name,
    bool IsDefault,
    bool Activo);
