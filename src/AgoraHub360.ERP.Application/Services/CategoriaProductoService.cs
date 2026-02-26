namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class CategoriaProductoService : ICategoriaProductoService
{
    private readonly ICategoryService _categoryService;

    public CategoriaProductoService(ICategoryService categoryService) => _categoryService = categoryService;

    public Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken ct = default)
        => _categoryService.GetAllAsync(ct: ct);

    public Task<Result<CategoryDto>> GetByIdAsync(long id, CancellationToken ct = default)
        => _categoryService.GetByIdAsync(id, ct);

    public Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
        => _categoryService.CreateAsync(dto, ct);

    public Task<Result<CategoryDto>> UpdateAsync(long id, UpdateCategoryDto dto, CancellationToken ct = default)
        => _categoryService.UpdateAsync(id, dto, ct);

    public Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
        => _categoryService.DeleteAsync(id, ct);
}
