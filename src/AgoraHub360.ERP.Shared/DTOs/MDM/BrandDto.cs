namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public record BrandDto(
    long BrandId,
    string Name,
    string? LogoUrl,
    bool Activo);

public record CreateBrandDto(
    string Name,
    string? LogoUrl = null);

public record UpdateBrandDto(
    string Name,
    string? LogoUrl,
    bool Activo);
