namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public interface ICategoriaProductoService
{
    Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<CategoryDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default);
    Task<Result<CategoryDto>> UpdateAsync(long id, UpdateCategoryDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
