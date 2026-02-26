namespace AgoraHub360.ERP.Shared.DTOs.PRC;

public record PriceListDto(
    long PriceListId,
    int EmpresaId,
    string Code,
    string Name,
    string CurrencyId,
    int? ChannelId,
    DateOnly? ValidFrom,
    DateOnly? ValidTo,
    bool IsDefault,
    bool Activo);

public record CreatePriceListDto(
    string Code,
    string Name,
    string CurrencyId,
    int? ChannelId = null,
    DateOnly? ValidFrom = null,
    DateOnly? ValidTo = null,
    bool IsDefault = false);

public record UpdatePriceListDto(
    string Code,
    string Name,
    string CurrencyId,
    int? ChannelId,
    DateOnly? ValidFrom,
    DateOnly? ValidTo,
    bool IsDefault,
    bool Activo);

public record PriceListItemDto(
    long ItemId,
    long PriceListId,
    long? CompanyProductId,
    long? VariantId,
    decimal Price,
    decimal? MinQty,
    decimal? DiscountPercent,
    DateOnly? ValidFrom,
    DateOnly? ValidTo);

public record CreatePriceListItemDto(
    long PriceListId,
    long? CompanyProductId,
    long? VariantId,
    decimal Price,
    decimal? MinQty = null,
    decimal? DiscountPercent = null,
    DateOnly? ValidFrom = null,
    DateOnly? ValidTo = null);
