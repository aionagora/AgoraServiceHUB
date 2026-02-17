namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class UnidadMedidaService : IUnidadMedidaService
{
    private readonly IRepository<UnidadMedida> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UnidadMedidaService(
        IRepository<UnidadMedida> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<UnidadMedidaDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _repository.GetAllAsync(ct);
        return Result<IReadOnlyList<UnidadMedidaDto>>.Success(
            items.Select(MapToDto).ToList().AsReadOnly());
    }

    public async Task<Result<UnidadMedidaDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<UnidadMedidaDto>.Failure($"Unidad de medida con Id {id} no encontrada.");
        return Result<UnidadMedidaDto>.Success(MapToDto(entity));
    }

    public async Task<Result<UnidadMedidaDto>> CreateAsync(CreateUnidadMedidaDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<UnidadMedidaDto>.Failure("No se pudo determinar la empresa activa.");

        // Validar nombre único
        var byName = await _repository.FindAsync(
            u => u.EmpresaId == empresaId.Value && u.Nombre == dto.Nombre, ct);
        if (byName.Count > 0)
            return Result<UnidadMedidaDto>.Failure($"Ya existe una unidad de medida con nombre '{dto.Nombre}'.");

        // Validar abreviatura única
        var byAbbr = await _repository.FindAsync(
            u => u.EmpresaId == empresaId.Value && u.Abreviatura == dto.Abreviatura, ct);
        if (byAbbr.Count > 0)
            return Result<UnidadMedidaDto>.Failure($"Ya existe una unidad de medida con abreviatura '{dto.Abreviatura}'.");

        var entity = new UnidadMedida
        {
            Nombre = dto.Nombre,
            Abreviatura = dto.Abreviatura,
            EmpresaId = empresaId.Value
        };

        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<UnidadMedidaDto>.Success(MapToDto(entity));
    }

    public async Task<Result<UnidadMedidaDto>> UpdateAsync(int id, UpdateUnidadMedidaDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<UnidadMedidaDto>.Failure($"Unidad de medida con Id {id} no encontrada.");

        // Validar nombre único
        var byName = await _repository.FindAsync(
            u => u.EmpresaId == entity.EmpresaId && u.Nombre == dto.Nombre && u.Id != id, ct);
        if (byName.Count > 0)
            return Result<UnidadMedidaDto>.Failure($"Ya existe otra unidad de medida con nombre '{dto.Nombre}'.");

        // Validar abreviatura única
        var byAbbr = await _repository.FindAsync(
            u => u.EmpresaId == entity.EmpresaId && u.Abreviatura == dto.Abreviatura && u.Id != id, ct);
        if (byAbbr.Count > 0)
            return Result<UnidadMedidaDto>.Failure($"Ya existe otra unidad de medida con abreviatura '{dto.Abreviatura}'.");

        entity.Nombre = dto.Nombre;
        entity.Abreviatura = dto.Abreviatura;
        entity.Activo = dto.Activo;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<UnidadMedidaDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<bool>.Failure($"Unidad de medida con Id {id} no encontrada.");

        await _repository.DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static UnidadMedidaDto MapToDto(UnidadMedida e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Abreviatura = e.Abreviatura,
        Activo = e.Activo,
        FechaCreacion = e.FechaCreacion
    };
}
