namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public interface IProductVariantService
{
    Task<Result<IReadOnlyList<ProductVariantDto>>> GetByProductAsync(long parentProductId, CancellationToken ct = default);
    Task<Result<ProductVariantDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<ProductVariantDto>> CreateAsync(CreateProductVariantDto dto, CancellationToken ct = default);
    Task<Result<ProductVariantDto>> UpdateAsync(long id, UpdateProductVariantDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
