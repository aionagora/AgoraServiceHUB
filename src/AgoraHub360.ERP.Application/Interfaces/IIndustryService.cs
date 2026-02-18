namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.RUL;

public interface IIndustryService
{
    Task<Result<IReadOnlyList<IndustryDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<IndustryDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<IndustryDto>> CreateAsync(CreateIndustryDto dto, CancellationToken ct = default);
    Task<Result<IndustryDto>> UpdateAsync(int id, UpdateIndustryDto dto, CancellationToken ct = default);

    Task<Result<IReadOnlyList<ProductIndustryRuleDto>>> GetRulesAsync(int industryId, CancellationToken ct = default);
    Task<Result<ProductIndustryRuleDto>> CreateRuleAsync(CreateProductIndustryRuleDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteRuleAsync(long ruleId, CancellationToken ct = default);
}
