namespace AgoraHub360.ERP.Shared.DTOs.RUL;

public record IndustryDto(
    int IndustryId,
    string Code,
    string Name,
    bool Activo);

public record CreateIndustryDto(string Code, string Name);
public record UpdateIndustryDto(string Code, string Name, bool Activo);

public record ProductIndustryRuleDto(
    long RuleId,
    int IndustryId,
    string IndustryCode,
    string ConditionJson,
    string ActionsJson,
    int Priority,
    bool Activo);

public record CreateProductIndustryRuleDto(
    int IndustryId,
    string ConditionJson,
    string ActionsJson,
    int Priority = 100);

public record CompanyProductFeatureDto(
    int EmpresaId,
    long CompanyProductId,
    string FeatureCode,
    bool IsEnabled);

public record SetFeatureDto(string FeatureCode, bool IsEnabled);
