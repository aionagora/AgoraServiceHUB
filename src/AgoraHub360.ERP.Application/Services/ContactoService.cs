namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ContactoService : IContactoService
{
    private readonly IRepository<Contacto> _contactoRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<ClienteSucursal> _clienteSucursalRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ContactoService(
        IRepository<Contacto> contactoRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<ClienteSucursal> clienteSucursalRepository,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _contactoRepository = contactoRepository;
        _clienteRepository = clienteRepository;
        _clienteSucursalRepository = clienteSucursalRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<ContactoDto>>> GetAllByClienteAsync(int clienteId, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<IReadOnlyList<ContactoDto>>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;
        var cliente = await _clienteRepository.GetByIdAsync(clienteId, ct);
        if (cliente is null || cliente.EmpresaId != empresaId)
            return Result<IReadOnlyList<ContactoDto>>.Failure("Cliente no encontrado para la empresa activa.");

        var items = await _contactoRepository.FindAsync(x => x.ClienteId == clienteId, ct);
        var dtos = items
            .OrderByDescending(x => x.EsPrincipal)
            .ThenBy(x => x.Nombres)
            .Select(MapToDto)
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<ContactoDto>>.Success(dtos);
    }

    public async Task<Result<ContactoDto>> GetByIdAsync(int clienteId, int id, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<ContactoDto>.Failure(empresaIdResult.Error!);

        var entity = await _contactoRepository.GetByIdAsync(id, ct);
        if (entity is null || entity.ClienteId != clienteId || entity.EmpresaId != empresaIdResult.Value)
            return Result<ContactoDto>.Failure("Contacto no encontrado.");

        return Result<ContactoDto>.Success(MapToDto(entity));
    }

    public async Task<Result<ContactoDto>> CreateAsync(int clienteId, CreateContactoDto dto, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<ContactoDto>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;
        var cliente = await _clienteRepository.GetByIdAsync(clienteId, ct);
        if (cliente is null || cliente.EmpresaId != empresaId)
            return Result<ContactoDto>.Failure("Cliente no encontrado para la empresa activa.");

        var sucursalValidation = await ValidateSucursalAsync(clienteId, dto.ClienteSucursalId, ct);
        if (!sucursalValidation.IsSuccess)
            return Result<ContactoDto>.Failure(sucursalValidation.Error!);

        var existentes = await _contactoRepository.FindAsync(x => x.ClienteId == clienteId, ct);
        var debeSerPrincipal = dto.EsPrincipal || !existentes.Any(x => x.Activo && x.EsPrincipal);

        if (debeSerPrincipal)
            await ClearPrincipalAsync(existentes, ct);

        var entity = new Contacto
        {
            EmpresaId = empresaId,
            ClienteId = clienteId,
            ClienteSucursalId = dto.ClienteSucursalId,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            Cargo = dto.Cargo,
            Telefono = dto.Telefono,
            Celular = dto.Celular,
            WhatsApp = dto.WhatsApp,
            Email = dto.Email,
            EsPrincipal = debeSerPrincipal,
            Activo = dto.Activo
        };

        await _contactoRepository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ContactoDto>.Success(MapToDto(entity));
    }

    public async Task<Result<ContactoDto>> UpdateAsync(int clienteId, int id, UpdateContactoDto dto, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<ContactoDto>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;
        var entity = await _contactoRepository.GetByIdAsync(id, ct);
        if (entity is null || entity.ClienteId != clienteId || entity.EmpresaId != empresaId)
            return Result<ContactoDto>.Failure("Contacto no encontrado.");

        var sucursalValidation = await ValidateSucursalAsync(clienteId, dto.ClienteSucursalId, ct);
        if (!sucursalValidation.IsSuccess)
            return Result<ContactoDto>.Failure(sucursalValidation.Error!);

        var existentes = await _contactoRepository.FindAsync(x => x.ClienteId == clienteId, ct);
        if (dto.EsPrincipal)
            await ClearPrincipalAsync(existentes.Where(x => x.Id != id), ct);

        entity.ClienteSucursalId = dto.ClienteSucursalId;
        entity.Nombres = dto.Nombres;
        entity.Apellidos = dto.Apellidos;
        entity.Cargo = dto.Cargo;
        entity.Telefono = dto.Telefono;
        entity.Celular = dto.Celular;
        entity.WhatsApp = dto.WhatsApp;
        entity.Email = dto.Email;
        entity.EsPrincipal = dto.EsPrincipal;
        entity.Activo = dto.Activo;

        if (entity.Activo && !entity.EsPrincipal)
        {
            var otraPrincipalActiva = existentes.Any(x => x.Id != entity.Id && x.Activo && x.EsPrincipal);
            if (!otraPrincipalActiva)
                return Result<ContactoDto>.Failure("Debe existir un contacto principal activo por cliente.");
        }

        await _contactoRepository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ContactoDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int clienteId, int id, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<bool>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;
        var entity = await _contactoRepository.GetByIdAsync(id, ct);
        if (entity is null || entity.ClienteId != clienteId || entity.EmpresaId != empresaId)
            return Result<bool>.Failure("Contacto no encontrado.");

        var contactos = await _contactoRepository.FindAsync(x => x.ClienteId == clienteId, ct);

        if (entity.EsPrincipal)
        {
            var reemplazo = contactos
                .Where(x => x.Id != id && x.Activo)
                .OrderBy(x => x.Id)
                .FirstOrDefault();

            if (reemplazo is null)
                return Result<bool>.Failure("No se puede desactivar el único contacto principal activo.");

            reemplazo.EsPrincipal = true;
            await _contactoRepository.UpdateAsync(reemplazo, ct);
        }

        entity.Activo = false;
        entity.EsPrincipal = false;
        await _contactoRepository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> ValidateSucursalAsync(int clienteId, int? clienteSucursalId, CancellationToken ct)
    {
        if (!clienteSucursalId.HasValue)
            return Result<bool>.Success(true);

        var sucursal = await _clienteSucursalRepository.GetByIdAsync(clienteSucursalId.Value, ct);
        if (sucursal is null || sucursal.ClienteId != clienteId)
            return Result<bool>.Failure("La sucursal seleccionada no pertenece al cliente.");

        return Result<bool>.Success(true);
    }

    private async Task ClearPrincipalAsync(IEnumerable<Contacto> contactos, CancellationToken ct)
    {
        foreach (var contacto in contactos.Where(x => x.EsPrincipal))
        {
            contacto.EsPrincipal = false;
            await _contactoRepository.UpdateAsync(contacto, ct);
        }
    }

    private Result<int> TryGetEmpresaId()
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<int>.Failure("No se pudo determinar la empresa activa.");

        return Result<int>.Success(_currentUser.EmpresaId.Value);
    }

    private static ContactoDto MapToDto(Contacto e) => new()
    {
        Id = e.Id,
        ClienteId = e.ClienteId,
        EmpresaId = e.EmpresaId,
        ClienteSucursalId = e.ClienteSucursalId,
        Nombres = e.Nombres,
        Apellidos = e.Apellidos,
        Cargo = e.Cargo,
        Telefono = e.Telefono,
        Celular = e.Celular,
        WhatsApp = e.WhatsApp,
        Email = e.Email,
        EsPrincipal = e.EsPrincipal,
        Activo = e.Activo,
        FechaCreacion = e.FechaCreacion
    };
}
