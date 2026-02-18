namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;
using AgoraHub360.ERP.Shared.DTOs.RUL;

public interface ICompanyProductService
{
    Task<Result<IReadOnlyList<CompanyProductDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<CompanyProductDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<CompanyProductDto>> CreateAsync(CreateCompanyProductDto dto, CancellationToken ct = default);
    Task<Result<CompanyProductDto>> UpdateAsync(long id, UpdateCompanyProductDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);

    Task<Result<IReadOnlyList<CompanyProductFeatureDto>>> GetFeaturesAsync(long companyProductId, CancellationToken ct = default);
    Task<Result<CompanyProductFeatureDto>> SetFeatureAsync(long companyProductId, SetFeatureDto dto, CancellationToken ct = default);
    Task<Result<bool>> ApplyIndustryRulesAsync(long companyProductId, int industryId, CancellationToken ct = default);
}
