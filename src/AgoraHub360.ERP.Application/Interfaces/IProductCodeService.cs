namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public interface IProductCodeService
{
    Task<Result<IReadOnlyList<ProductCodeDto>>> GetByProductAsync(long productId, CancellationToken ct = default);
    Task<Result<ProductCodeDto>> CreateAsync(CreateProductCodeDto dto, CancellationToken ct = default);
    Task<Result<ProductCodeDto>> UpdateAsync(long id, UpdateProductCodeDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
