namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

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
        var items = await _repository.GetAllAsync(ct);
        return Result<IReadOnlyList<ProveedorDto>>.Success(
            items.Select(MapToDto).ToList().AsReadOnly());
    }

    public async Task<Result<ProveedorDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<ProveedorDto>.Failure($"Proveedor con Id {id} no encontrado.");
        return Result<ProveedorDto>.Success(MapToDto(entity));
    }

    public async Task<Result<ProveedorDto>> CreateAsync(CreateProveedorDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<ProveedorDto>.Failure("No se pudo determinar la empresa activa.");

        // Validar código único
        var byCodigo = await _repository.FindAsync(
            p => p.EmpresaId == empresaId.Value && p.Codigo == dto.Codigo, ct);
        if (byCodigo.Count > 0)
            return Result<ProveedorDto>.Failure($"Ya existe un proveedor con código '{dto.Codigo}'.");

        // Validar NIT único si se proporcionó
        if (!string.IsNullOrWhiteSpace(dto.NIT))
        {
            var byNit = await _repository.FindAsync(
                p => p.EmpresaId == empresaId.Value && p.NIT == dto.NIT, ct);
            if (byNit.Count > 0)
                return Result<ProveedorDto>.Failure($"Ya existe un proveedor con NIT '{dto.NIT}'.");
        }

        var entity = new Proveedor
        {
            Codigo = dto.Codigo,
            RazonSocial = dto.RazonSocial,
            NIT = dto.NIT,
            Direccion = dto.Direccion,
            Telefono = dto.Telefono,
            Email = dto.Email,
            NombreContacto = dto.NombreContacto,
            TipoProveedor = dto.TipoProveedor,
            Pais = dto.Pais,
            CondicionPago = dto.CondicionPago,
            EmpresaId = empresaId.Value
        };

        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ProveedorDto>.Success(MapToDto(entity));
    }

    public async Task<Result<ProveedorDto>> UpdateAsync(int id, UpdateProveedorDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<ProveedorDto>.Failure($"Proveedor con Id {id} no encontrado.");

        // Validar código único
        var byCodigo = await _repository.FindAsync(
            p => p.EmpresaId == entity.EmpresaId && p.Codigo == dto.Codigo && p.Id != id, ct);
        if (byCodigo.Count > 0)
            return Result<ProveedorDto>.Failure($"Ya existe otro proveedor con código '{dto.Codigo}'.");

        // Validar NIT único
        if (!string.IsNullOrWhiteSpace(dto.NIT))
        {
            var byNit = await _repository.FindAsync(
                p => p.EmpresaId == entity.EmpresaId && p.NIT == dto.NIT && p.Id != id, ct);
            if (byNit.Count > 0)
                return Result<ProveedorDto>.Failure($"Ya existe otro proveedor con NIT '{dto.NIT}'.");
        }

        entity.Codigo = dto.Codigo;
        entity.RazonSocial = dto.RazonSocial;
        entity.NIT = dto.NIT;
        entity.Direccion = dto.Direccion;
        entity.Telefono = dto.Telefono;
        entity.Email = dto.Email;
        entity.NombreContacto = dto.NombreContacto;
        entity.TipoProveedor = dto.TipoProveedor;
        entity.Pais = dto.Pais;
        entity.CondicionPago = dto.CondicionPago;
        entity.Activo = dto.Activo;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ProveedorDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<bool>.Failure($"Proveedor con Id {id} no encontrado.");

        await _repository.DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static ProveedorDto MapToDto(Proveedor e) => new()
    {
        Id = e.Id,
        Codigo = e.Codigo,
        RazonSocial = e.RazonSocial,
        NIT = e.NIT,
        Direccion = e.Direccion,
        Telefono = e.Telefono,
        Email = e.Email,
        NombreContacto = e.NombreContacto,
        TipoProveedor = e.TipoProveedor,
        Pais = e.Pais,
        CondicionPago = e.CondicionPago,
        Activo = e.Activo,
        FechaCreacion = e.FechaCreacion
    };
}
