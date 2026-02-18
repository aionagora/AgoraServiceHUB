namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public interface IAttributeDefinitionService
{
    Task<Result<IReadOnlyList<AttributeDefinitionDto>>> GetByIndustryAsync(int industryId, CancellationToken ct = default);
    Task<Result<AttributeDefinitionDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<AttributeDefinitionDto>> CreateAsync(CreateAttributeDefinitionDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);

    Task<Result<AttributeOptionDto>> AddOptionAsync(CreateAttributeOptionDto dto, CancellationToken ct = default);
    Task<Result<AttributeOptionDto>> UpdateOptionAsync(long optionId, UpdateAttributeOptionDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteOptionAsync(long optionId, CancellationToken ct = default);

    Task<Result<IReadOnlyList<ProductAttributeDto>>> GetProductAttributesAsync(long productId, CancellationToken ct = default);
    Task<Result<ProductAttributeDto>> UpsertProductAttributeAsync(UpsertProductAttributeDto dto, CancellationToken ct = default);
}
