namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public record CatalogDto(
    long CatalogId,
    byte Scope,
    int? EmpresaId,
    string Name,
    bool IsDefault,
    bool Activo);

public record CreateCatalogDto(
    byte Scope,
    int? EmpresaId,
    string Name,
    bool IsDefault = false);

public record UpdateCatalogDto(
    string Name,
    bool IsDefault,
    bool Activo);
