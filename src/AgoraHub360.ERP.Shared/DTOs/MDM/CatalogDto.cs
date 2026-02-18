namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public record CatalogDto(
    long CatalogId,
    byte Scope,
    int? EmpresaId,
    string Nombre,
    bool IsDefault,
    bool Activo);

public record CreateCatalogDto(
    byte Scope,
    int? EmpresaId,
    string Nombre,
    bool IsDefault = false);

public record UpdateCatalogDto(
    string Nombre,
    bool IsDefault,
    bool Activo);
