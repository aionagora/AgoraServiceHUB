namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ManufacturerService : IManufacturerService
{
    private readonly IRepository<Manufacturer> _repo;
    private readonly IUnitOfWork _uow;

    public ManufacturerService(IRepository<Manufacturer> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<ManufacturerDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _repo.GetAllAsync(ct);
        return Result<IReadOnlyList<ManufacturerDto>>.Success(
            items.Select(Map).ToList().AsReadOnly());
    }

    public async Task<Result<ManufacturerDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<ManufacturerDto>.Failure($"Fabricante {id} no encontrado.");
        return Result<ManufacturerDto>.Success(Map(entity));
    }

    public async Task<Result<ManufacturerDto>> CreateAsync(CreateManufacturerDto dto, CancellationToken ct = default)
    {
        var dup = await _repo.FindAsync(m => m.Name == dto.Name, ct);
        if (dup.Any()) return Result<ManufacturerDto>.Failure($"Ya existe un fabricante con nombre '{dto.Name}'.");

        var entity = new Manufacturer
        {
            Name = dto.Name,
            Country = dto.Country,
            Activo = true
        };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<ManufacturerDto>.Success(Map(entity));
    }

    public async Task<Result<ManufacturerDto>> UpdateAsync(long id, UpdateManufacturerDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<ManufacturerDto>.Failure($"Fabricante {id} no encontrado.");

        var dup = await _repo.FindAsync(m => m.Name == dto.Name && m.ManufacturerId != id, ct);
        if (dup.Any()) return Result<ManufacturerDto>.Failure($"Ya existe otro fabricante con nombre '{dto.Name}'.");

        entity.Name = dto.Name;
        entity.Country = dto.Country;
        entity.Activo = dto.Activo;

        await _repo.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<ManufacturerDto>.Success(Map(entity));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<bool>.Failure($"Fabricante {id} no encontrado.");
        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static ManufacturerDto Map(Manufacturer m) => new(m.ManufacturerId, m.Name, m.Country, m.Activo);
}
