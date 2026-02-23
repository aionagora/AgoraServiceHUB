namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class CatalogService : ICatalogService
{
    private readonly IRepository<Catalog> _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CatalogService(IRepository<Catalog> repo, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _repo = repo;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<CatalogDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        var items = await _repo.FindAsync(
            c => c.Scope == 1 || (c.Scope == 2 && c.EmpresaId == empresaId), ct);
        return Result<IReadOnlyList<CatalogDto>>.Success(
            items.Select(Map).ToList().AsReadOnly());
    }

    public async Task<Result<CatalogDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<CatalogDto>.Failure($"Catalog {id} not found.");
        return Result<CatalogDto>.Success(Map(entity));
    }

    public async Task<Result<CatalogDto>> CreateAsync(CreateCatalogDto dto, CancellationToken ct = default)
    {
        var dup = await _repo.FindAsync(
            c => c.Scope == dto.Scope && c.EmpresaId == dto.EmpresaId && c.Name == dto.Name, ct);
        if (dup.Any())
            return Result<CatalogDto>.Failure($"A catalog '{dto.Name}' with that scope already exists.");

        var entity = new Catalog
        {
            Scope = dto.Scope,
            EmpresaId = dto.EmpresaId,
            Name = dto.Name,
            IsDefault = dto.IsDefault,
            Activo = true
        };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<CatalogDto>.Success(Map(entity));
    }

    public async Task<Result<CatalogDto>> UpdateAsync(long id, UpdateCatalogDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<CatalogDto>.Failure($"Catalog {id} not found.");

        entity.Name = dto.Name;
        entity.IsDefault = dto.IsDefault;
        entity.Activo = dto.Activo;

        await _repo.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<CatalogDto>.Success(Map(entity));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<bool>.Failure($"Catalog {id} not found.");
        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static CatalogDto Map(Catalog c) => new(
        c.CatalogId, c.Scope, c.EmpresaId, c.Name, c.IsDefault, c.Activo);
}
