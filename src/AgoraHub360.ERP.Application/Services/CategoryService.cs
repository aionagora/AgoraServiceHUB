namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _repo;
    private readonly IUnitOfWork _uow;

    public CategoryService(IRepository<Category> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(long? catalogId = null, CancellationToken ct = default)
    {
        var items = await _repo.FindAsync(
            c => catalogId == null || c.CatalogId == catalogId, ct);
        return Result<IReadOnlyList<CategoryDto>>.Success(items.Select(Map).ToList().AsReadOnly());
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetByCatalogAsync(long catalogId, CancellationToken ct = default)
    {
        var items = await _repo.FindAsync(c => c.CatalogId == catalogId, ct);
        return Result<IReadOnlyList<CategoryDto>>.Success(items.Select(Map).ToList().AsReadOnly());
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<CategoryDto>.Failure($"Category {id} not found.");
        return Result<CategoryDto>.Success(Map(entity));
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        var dup = await _repo.FindAsync(
            c => c.CatalogId == dto.CatalogId
              && c.ParentCategoryId == dto.ParentCategoryId
              && c.Name == dto.Name, ct);
        if (dup.Any())
            return Result<CategoryDto>.Failure($"Category '{dto.Name}' already exists at that level.");

        var entity = new Category
        {
            CatalogId = dto.CatalogId,
            ParentCategoryId = dto.ParentCategoryId,
            Name = dto.Name,
            SortOrder = dto.SortOrder,
            Activo = true
        };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<CategoryDto>.Success(Map(entity));
    }

    public async Task<Result<CategoryDto>> UpdateAsync(long id, UpdateCategoryDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<CategoryDto>.Failure($"Category {id} not found.");

        entity.ParentCategoryId = dto.ParentCategoryId;
        entity.Name = dto.Name;
        entity.SortOrder = dto.SortOrder;
        entity.Activo = dto.Activo;

        await _repo.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<CategoryDto>.Success(Map(entity));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<bool>.Failure($"Category {id} not found.");
        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static CategoryDto Map(Category c) => new(
        c.CategoryId, c.CatalogId, c.ParentCategoryId, c.Name, c.Path, c.SortOrder, c.Activo);
}
