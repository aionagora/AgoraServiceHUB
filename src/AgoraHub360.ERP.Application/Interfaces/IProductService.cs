namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public interface IProductService
{
    Task<Result<IReadOnlyList<ProductDto2>>> GetAllAsync(long? catalogId = null, CancellationToken ct = default);
    Task<Result<ProductDto2>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<ProductDto2>> CreateAsync(CreateProductDto2 dto, CancellationToken ct = default);
    Task<Result<ProductDto2>> UpdateAsync(long id, UpdateProductDto2 dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
