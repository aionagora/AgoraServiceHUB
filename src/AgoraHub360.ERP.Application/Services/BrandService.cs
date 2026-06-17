namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class BrandService : IBrandService
{
    private readonly IRepository<Brand> _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public BrandService(IRepository<Brand> repo, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _repo = repo;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<BrandDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<BrandDto>>.Failure("No existe empresa activa en la sesión.");

        var items = await _repo.FindAsync(b => b.EmpresaId == empresaId.Value, ct);
        return Result<IReadOnlyList<BrandDto>>.Success(
            items.Select(Map).ToList().AsReadOnly());
    }

    public async Task<Result<BrandDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<BrandDto>.Failure($"Marca {id} no encontrada.");
        return Result<BrandDto>.Success(Map(entity));
    }

    public async Task<Result<BrandDto>> CreateAsync(CreateBrandDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId
            ?? throw new InvalidOperationException("EmpresaId required.");

        var dup = await _repo.FindAsync(b => b.Name == dto.Name, ct);
        if (dup.Any()) return Result<BrandDto>.Failure($"Ya existe una marca con nombre '{dto.Name}'.");

        var entity = new Brand
        {
            EmpresaId = empresaId,
            Name = dto.Name,
            LogoUrl = dto.LogoUrl,
            Activo = true
        };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<BrandDto>.Success(Map(entity));
    }

    public async Task<Result<BrandDto>> UpdateAsync(long id, UpdateBrandDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<BrandDto>.Failure($"Marca {id} no encontrada.");

        var dup = await _repo.FindAsync(b => b.Name == dto.Name && b.BrandId != id, ct);
        if (dup.Any()) return Result<BrandDto>.Failure($"Ya existe otra marca con nombre '{dto.Name}'.");

        entity.Name = dto.Name;
        entity.LogoUrl = dto.LogoUrl;
        entity.Activo = dto.Activo;

        await _repo.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<BrandDto>.Success(Map(entity));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<bool>.Failure($"Marca {id} no encontrada.");
        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static BrandDto Map(Brand b) => new(b.BrandId, b.Name, b.LogoUrl, b.Activo);
}
