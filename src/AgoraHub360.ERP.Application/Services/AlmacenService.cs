namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class AlmacenService : IAlmacenService
{
    private readonly IRepository<Almacen> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AlmacenService(
        IRepository<Almacen> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<AlmacenDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _repository.GetAllAsync(ct);
        return Result<IReadOnlyList<AlmacenDto>>.Success(
            items.Select(MapToDto).ToList().AsReadOnly());
    }

    public async Task<Result<AlmacenDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<AlmacenDto>.Failure($"Almacén con Id {id} no encontrado.");
        return Result<AlmacenDto>.Success(MapToDto(entity));
    }

    public async Task<Result<AlmacenDto>> CreateAsync(CreateAlmacenDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<AlmacenDto>.Failure("No se pudo determinar la empresa activa.");

        // Validar código único por empresa
        var existing = await _repository.FindAsync(
            a => a.EmpresaId == empresaId.Value && a.Codigo == dto.Codigo, ct);
        if (existing.Count > 0)
            return Result<AlmacenDto>.Failure($"Ya existe un almacén con código '{dto.Codigo}'.");

        var entity = new Almacen
        {
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Direccion = dto.Direccion,
            Responsable = dto.Responsable,
            Telefono = dto.Telefono,
            EmpresaId = empresaId.Value
        };

        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<AlmacenDto>.Success(MapToDto(entity));
    }

    public async Task<Result<AlmacenDto>> UpdateAsync(int id, UpdateAlmacenDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<AlmacenDto>.Failure($"Almacén con Id {id} no encontrado.");

        // Validar código único (excluyendo el registro actual)
        var existing = await _repository.FindAsync(
            a => a.EmpresaId == entity.EmpresaId && a.Codigo == dto.Codigo && a.Id != id, ct);
        if (existing.Count > 0)
            return Result<AlmacenDto>.Failure($"Ya existe otro almacén con código '{dto.Codigo}'.");

        entity.Codigo = dto.Codigo;
        entity.Nombre = dto.Nombre;
        entity.Direccion = dto.Direccion;
        entity.Responsable = dto.Responsable;
        entity.Telefono = dto.Telefono;
        entity.Activo = dto.Activo;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<AlmacenDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<bool>.Failure($"Almacén con Id {id} no encontrado.");

        await _repository.DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static AlmacenDto MapToDto(Almacen e) => new()
    {
        Id = e.Id,
        Codigo = e.Codigo,
        Nombre = e.Nombre,
        SucursalId = e.SucursalId,
        Direccion = e.Direccion,
        Responsable = e.Responsable,
        Telefono = e.Telefono,
        Activo = e.Activo,
        FechaCreacion = e.FechaCreacion
    };
}
