namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public record AttributeDefinitionDto(
    long AttributeId,
    int IndustryId,
    string IndustryCode,
    string Code,
    string Name,
    byte DataType,
    bool IsRequired,
    bool IsSearchable,
    bool IsVariantAxis,
    string? ValidationRegex,
    decimal? MinValue,
    decimal? MaxValue,
    string? UnitHint,
    bool Activo,
    IReadOnlyList<AttributeOptionDto> Options);

public record AttributeOptionDto(
    long OptionId,
    long AttributeId,
    string Value,
    int SortOrder);

public record CreateAttributeDefinitionDto(
    int IndustryId,
    string Code,
    string Name,
    byte DataType,
    bool IsRequired = false,
    bool IsSearchable = false,
    bool IsVariantAxis = false,
    string? ValidationRegex = null,
    decimal? MinValue = null,
    decimal? MaxValue = null,
    string? UnitHint = null);

public record CreateAttributeOptionDto(
    long AttributeId,
    string Value,
    int SortOrder = 0);

public record UpdateAttributeOptionDto(
    string Value,
    int SortOrder);

public record ProductAttributeDto(
    long ProductAttributeId,
    long ProductId,
    long AttributeId,
    string AttributeName,
    string? ValueString,
    decimal? ValueDecimal,
    int? ValueInt,
    bool? ValueBool,
    DateOnly? ValueDate,
    string? ValueJson,
    long? OptionId,
    string? OptionValue,
    DateOnly? ValidFrom,
    DateOnly? ValidTo);

public record UpsertProductAttributeDto(
    long ProductId,
    long AttributeId,
    string? ValueString = null,
    decimal? ValueDecimal = null,
    int? ValueInt = null,
    bool? ValueBool = null,
    DateOnly? ValueDate = null,
    string? ValueJson = null,
    long? OptionId = null,
    DateOnly? ValidFrom = null,
    DateOnly? ValidTo = null);
