namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Rol;

public class RolService : IRolService
{
    private readonly IRepository<Rol> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RolService(IRepository<Rol> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<RolDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var roles = await _repository.GetAllAsync(ct);
        var dtos = roles.Select(MapToDto).ToList().AsReadOnly();
        return Result<IReadOnlyList<RolDto>>.Success(dtos);
    }

    public async Task<Result<RolDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var rol = await _repository.GetByIdAsync(id, ct);
        if (rol is null)
            return Result<RolDto>.Failure($"Rol con Id {id} no encontrado.");

        return Result<RolDto>.Success(MapToDto(rol));
    }

    public async Task<Result<RolDto>> CreateAsync(CreateRolDto dto, CancellationToken ct = default)
    {
        var existing = await _repository.FindAsync(r => r.Nombre == dto.Nombre, ct);
        if (existing.Count > 0)
            return Result<RolDto>.Failure($"Ya existe un rol con nombre '{dto.Nombre}'.");

        var rol = new Rol
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Activo = true
        };

        await _repository.AddAsync(rol, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<RolDto>.Success(MapToDto(rol));
    }

    public async Task<Result<RolDto>> UpdateAsync(int id, UpdateRolDto dto, CancellationToken ct = default)
    {
        var rol = await _repository.GetByIdAsync(id, ct);
        if (rol is null)
            return Result<RolDto>.Failure($"Rol con Id {id} no encontrado.");

        var existing = await _repository.FindAsync(r => r.Nombre == dto.Nombre && r.Id != id, ct);
        if (existing.Count > 0)
            return Result<RolDto>.Failure($"Ya existe otro rol con nombre '{dto.Nombre}'.");

        rol.Nombre = dto.Nombre;
        rol.Descripcion = dto.Descripcion;
        rol.Activo = dto.Activo;

        await _repository.UpdateAsync(rol, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<RolDto>.Success(MapToDto(rol));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var rol = await _repository.GetByIdAsync(id, ct);
        if (rol is null)
            return Result<bool>.Failure($"Rol con Id {id} no encontrado.");

        await _repository.DeleteAsync(rol, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    private static RolDto MapToDto(Rol r) => new()
    {
        Id = r.Id,
        Nombre = r.Nombre,
        Descripcion = r.Descripcion,
        Activo = r.Activo
    };
}
