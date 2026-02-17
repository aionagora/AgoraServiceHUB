namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

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
        var items = await _repository.GetAllAsync(ct);
        return Result<IReadOnlyList<ClienteDto>>.Success(
            items.Select(MapToDto).ToList().AsReadOnly());
    }

    public async Task<Result<ClienteDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<ClienteDto>.Failure($"Cliente con Id {id} no encontrado.");
        return Result<ClienteDto>.Success(MapToDto(entity));
    }

    public async Task<Result<ClienteDto>> CreateAsync(CreateClienteDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<ClienteDto>.Failure("No se pudo determinar la empresa activa.");

        // Validar código único
        var byCodigo = await _repository.FindAsync(
            c => c.EmpresaId == empresaId.Value && c.Codigo == dto.Codigo, ct);
        if (byCodigo.Count > 0)
            return Result<ClienteDto>.Failure($"Ya existe un cliente con código '{dto.Codigo}'.");

        // Validar NIT único si se proporcionó
        if (!string.IsNullOrWhiteSpace(dto.NIT))
        {
            var byNit = await _repository.FindAsync(
                c => c.EmpresaId == empresaId.Value && c.NIT == dto.NIT, ct);
            if (byNit.Count > 0)
                return Result<ClienteDto>.Failure($"Ya existe un cliente con NIT '{dto.NIT}'.");
        }

        var entity = new Cliente
        {
            Codigo = dto.Codigo,
            RazonSocial = dto.RazonSocial,
            NIT = dto.NIT,
            Direccion = dto.Direccion,
            Telefono = dto.Telefono,
            Email = dto.Email,
            NombreContacto = dto.NombreContacto,
            TipoCliente = dto.TipoCliente,
            EmpresaId = empresaId.Value
        };

        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ClienteDto>.Success(MapToDto(entity));
    }

    public async Task<Result<ClienteDto>> UpdateAsync(int id, UpdateClienteDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<ClienteDto>.Failure($"Cliente con Id {id} no encontrado.");

        // Validar código único
        var byCodigo = await _repository.FindAsync(
            c => c.EmpresaId == entity.EmpresaId && c.Codigo == dto.Codigo && c.Id != id, ct);
        if (byCodigo.Count > 0)
            return Result<ClienteDto>.Failure($"Ya existe otro cliente con código '{dto.Codigo}'.");

        // Validar NIT único
        if (!string.IsNullOrWhiteSpace(dto.NIT))
        {
            var byNit = await _repository.FindAsync(
                c => c.EmpresaId == entity.EmpresaId && c.NIT == dto.NIT && c.Id != id, ct);
            if (byNit.Count > 0)
                return Result<ClienteDto>.Failure($"Ya existe otro cliente con NIT '{dto.NIT}'.");
        }

        entity.Codigo = dto.Codigo;
        entity.RazonSocial = dto.RazonSocial;
        entity.NIT = dto.NIT;
        entity.Direccion = dto.Direccion;
        entity.Telefono = dto.Telefono;
        entity.Email = dto.Email;
        entity.NombreContacto = dto.NombreContacto;
        entity.TipoCliente = dto.TipoCliente;
        entity.Activo = dto.Activo;

        await _repository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ClienteDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<bool>.Failure($"Cliente con Id {id} no encontrado.");

        await _repository.DeleteAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static ClienteDto MapToDto(Cliente e) => new()
    {
        Id = e.Id,
        Codigo = e.Codigo,
        RazonSocial = e.RazonSocial,
        NIT = e.NIT,
        Direccion = e.Direccion,
        Telefono = e.Telefono,
        Email = e.Email,
        NombreContacto = e.NombreContacto,
        TipoCliente = e.TipoCliente,
        Activo = e.Activo,
        FechaCreacion = e.FechaCreacion
    };
}
