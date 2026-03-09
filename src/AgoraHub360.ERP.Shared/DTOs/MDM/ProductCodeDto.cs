namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public record ProductCodeDto(
    long ProductCodeId,
    int EmpresaId,
    long ProductId,
    byte CodeType,
    string Valor,
    long? ProviderId,
    long? CustomerId,
    int? ChannelId,
    DateOnly? ValidFrom,
    DateOnly? ValidTo,
    bool IsPrimary,
    bool IsActive);

public record CreateProductCodeDto(
    long ProductId,
    byte CodeType,
    string Valor,
    long? ProviderId = null,
    long? CustomerId = null,
    int? ChannelId = null,
    DateOnly? ValidFrom = null,
    DateOnly? ValidTo = null,
    bool IsPrimary = false);

public record UpdateProductCodeDto(
    byte CodeType,
    string Valor,
    DateOnly? ValidFrom,
    DateOnly? ValidTo,
    bool IsPrimary,
    bool IsActive);
