namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class UomService : IUomService
{
    private readonly IRepository<Uom> _repo;
    private readonly IUnitOfWork _uow;

    public UomService(IRepository<Uom> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<UomDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _repo.FindAsync(_ => true, ct);
        return Result<IReadOnlyList<UomDto>>.Success(items.Select(Map).ToList().AsReadOnly());
    }

    public async Task<Result<UomDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<UomDto>.Failure($"UoM {id} not found.");
        return Result<UomDto>.Success(Map(entity));
    }

    public async Task<Result<UomDto>> CreateAsync(CreateUomDto dto, CancellationToken ct = default)
    {
        var dup = await _repo.FindAsync(u => u.Code == dto.Code, ct);
        if (dup.Any())
            return Result<UomDto>.Failure($"A unit of measure with code '{dto.Code}' already exists.");

        var entity = new Uom
        {
            Code = dto.Code,
            Name = dto.Name,
            Activo = true
        };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<UomDto>.Success(Map(entity));
    }

    public async Task<Result<UomDto>> UpdateAsync(int id, UpdateUomDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<UomDto>.Failure($"UoM {id} not found.");

        var dup = await _repo.FindAsync(u => u.Code == dto.Code && u.UomId != id, ct);
        if (dup.Any())
            return Result<UomDto>.Failure($"Another unit of measure with code '{dto.Code}' already exists.");

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.Activo = dto.Activo;

        await _repo.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<UomDto>.Success(Map(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<bool>.Failure($"UoM {id} not found.");
        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static UomDto Map(Uom u) => new UomDto
    {
        UomId = u.UomId,
        Code = u.Code,
        Name = u.Name,
        Activo = u.Activo
    };
}
