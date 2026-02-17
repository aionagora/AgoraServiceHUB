namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Proveedor;

public class ProveedorService : IProveedorService
{
    private readonly IRepository<Proveedor> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ProveedorService(
        IRepository<Proveedor> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<ProveedorDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<ProveedorDto>>.Failure("No se pudo determinar la empresa activa.");

        var items = await _repository.FindAsync(p => p.EmpresaId == empresaId.Value, ct);
        return Result<IReadOnlyList<ProveedorDto>>.Success(
            items.Select(MapToDto).ToList().AsReadOnly());
    }

    public async Task<Result<ProveedorDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<ProveedorDto>.Failure("No se pudo determinar la empresa activa.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<ProveedorDto>.Failure($"Proveedor con Id {id} no encontrado.");

        if (entity.EmpresaId != empresaId.Value)
            return Result<ProveedorDto>.Failure("No tiene permisos para acceder a este proveedor.");

        return Result<ProveedorDto>.Success(MapToDto(entity));
    }

    public async Task<Result<ProveedorDto>> CreateAsync(CreateProveedorDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<ProveedorDto>.Failure("No se pudo determinar la empresa activa.");

        // Validar NIT único
        var byNit = await _repository.FindAsync(
            p => p.EmpresaId == empresaId.Value && p.NIT == dto.NIT, ct);
        if (byNit.Any())
            return Result<ProveedorDto>.Failure($"Ya existe un proveedor con NIT '{dto.NIT}'.");

        var entity = new Proveedor
        {
            RazonSocial = dto.RazonSocial,
            NIT = dto.NIT,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Direccion = dto.Direccion,
            TipoProveedor = dto.TipoProveedor,
            EmpresaId = empresaId.Value,
            Activo = true
        };

        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ProveedorDto>.Success(MapToDto(entity));
    }

    public async Task<Result<ProveedorDto>> UpdateAsync(int id, UpdateProveedorDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<ProveedorDto>.Failure("No se pudo determinar la empresa activa.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<ProveedorDto>.Failure($"Proveedor con Id {id} no encontrado.");

        if (entity.EmpresaId != empresaId.Value)
            return Result<ProveedorDto>.Failure("No tiene permisos para modificar este proveedor.");

        // Validar NIT único
        var byNit = await _repository.FindAsync(
            p => p.EmpresaId == empresaId.Value && p.NIT == dto.NIT && p.Id != id, ct);
        if (byNit.Any())
            return Result<ProveedorDto>.Failure($"Ya existe otro proveedor con NIT '{dto.NIT}'.");

        entity.RazonSocial = dto.RazonSocial;
        entity.NIT = dto.NIT;
        entity.Telefono = dto.Telefono;
        entity.Email = dto.Email;
        entity.Direccion = dto.Direccion;
        entity.TipoProveedor = dto.TipoProveedor;
        entity.Activo = dto.Activo;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ProveedorDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No se pudo determinar la empresa activa.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<bool>.Failure($"Proveedor con Id {id} no encontrado.");

        if (entity.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("No tiene permisos para eliminar este proveedor.");

        await _repository.DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static ProveedorDto MapToDto(Proveedor e) => new()
    {
        Id = e.Id,
        RazonSocial = e.RazonSocial,
        NIT = e.NIT,
        Telefono = e.Telefono,
        Email = e.Email,
        Direccion = e.Direccion,
        TipoProveedor = e.TipoProveedor,
        Activo = e.Activo,
        EmpresaId = e.EmpresaId
    };
}
