namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Cliente;

public class ClienteService : IClienteService
{
    private readonly IRepository<Cliente> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ClienteService(
        IRepository<Cliente> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<ClienteDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<ClienteDto>>.Failure("No se pudo determinar la empresa activa.");

        var items = await _repository.FindAsync(c => c.EmpresaId == empresaId.Value, ct);
        return Result<IReadOnlyList<ClienteDto>>.Success(
            items.Select(MapToDto).ToList().AsReadOnly());
    }

    public async Task<Result<ClienteDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<ClienteDto>.Failure("No se pudo determinar la empresa activa.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<ClienteDto>.Failure($"Cliente con Id {id} no encontrado.");

        if (entity.EmpresaId != empresaId.Value)
            return Result<ClienteDto>.Failure("No tiene permisos para acceder a este cliente.");

        return Result<ClienteDto>.Success(MapToDto(entity));
    }

    public async Task<Result<ClienteDto>> CreateAsync(CreateClienteDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<ClienteDto>.Failure("No se pudo determinar la empresa activa.");

        // Validar NIT único
        var byNit = await _repository.FindAsync(
            c => c.EmpresaId == empresaId.Value && c.NIT == dto.NIT, ct);
        if (byNit.Any())
            return Result<ClienteDto>.Failure($"Ya existe un cliente con NIT '{dto.NIT}'.");

        var entity = new Cliente
        {
            RazonSocial = dto.RazonSocial,
            NIT = dto.NIT,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Direccion = dto.Direccion,
            EmpresaId = empresaId.Value,
            Activo = true
        };

        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ClienteDto>.Success(MapToDto(entity));
    }

    public async Task<Result<ClienteDto>> UpdateAsync(int id, UpdateClienteDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<ClienteDto>.Failure("No se pudo determinar la empresa activa.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<ClienteDto>.Failure($"Cliente con Id {id} no encontrado.");

        if (entity.EmpresaId != empresaId.Value)
            return Result<ClienteDto>.Failure("No tiene permisos para modificar este cliente.");

        // Validar NIT único
        var byNit = await _repository.FindAsync(
            c => c.EmpresaId == empresaId.Value && c.NIT == dto.NIT && c.Id != id, ct);
        if (byNit.Any())
            return Result<ClienteDto>.Failure($"Ya existe otro cliente con NIT '{dto.NIT}'.");

        entity.RazonSocial = dto.RazonSocial;
        entity.NIT = dto.NIT;
        entity.Telefono = dto.Telefono;
        entity.Email = dto.Email;
        entity.Direccion = dto.Direccion;
        entity.Activo = dto.Activo;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ClienteDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No se pudo determinar la empresa activa.");

        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<bool>.Failure($"Cliente con Id {id} no encontrado.");

        if (entity.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("No tiene permisos para eliminar este cliente.");

        await _repository.DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static ClienteDto MapToDto(Cliente e) => new()
    {
        Id = e.Id,
        RazonSocial = e.RazonSocial,
        NIT = e.NIT,
        Telefono = e.Telefono,
        Email = e.Email,
        Direccion = e.Direccion,
        Activo = e.Activo,
        EmpresaId = e.EmpresaId
    };
}
