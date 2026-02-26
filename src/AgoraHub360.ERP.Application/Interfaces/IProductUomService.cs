namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public interface IProductUomService
{
    Task<Result<IReadOnlyList<ProductUomDto>>> GetByProductAsync(long productId, CancellationToken ct = default);
    Task<Result<ProductUomDto>> CreateAsync(CreateProductUomDto dto, CancellationToken ct = default);
    Task<Result<ProductUomDto>> UpdateAsync(long id, UpdateProductUomDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
