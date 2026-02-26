namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public record ManufacturerDto(
    long ManufacturerId,
    string Name,
    string? Country,
    bool Activo);

public record CreateManufacturerDto(
    string Name,
    string? Country = null);

public record UpdateManufacturerDto(
    string Name,
    string? Country,
    bool Activo);
