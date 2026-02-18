namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.CategoriaProducto;

public interface ICategoriaProductoService
{
    Task<Result<IReadOnlyList<CategoriaProductoDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<CategoriaProductoDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<CategoriaProductoDto>> CreateAsync(CreateCategoriaProductoDto dto, CancellationToken ct = default);
    Task<Result<CategoriaProductoDto>> UpdateAsync(int id, UpdateCategoriaProductoDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
