namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public interface IProductoService
{
    Task<Result<IReadOnlyList<ProductoDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<ProductoDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<ProductoDto>> CreateAsync(CreateProductoDto dto, CancellationToken ct = default);
    Task<Result<ProductoDto>> UpdateAsync(int id, UpdateProductoDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
