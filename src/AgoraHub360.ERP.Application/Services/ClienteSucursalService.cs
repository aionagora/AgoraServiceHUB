namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ClienteSucursalService : IClienteSucursalService
{
    private readonly IRepository<ClienteSucursal> _clienteSucursalRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ClienteSucursalService(
        IRepository<ClienteSucursal> clienteSucursalRepository,
        IRepository<Cliente> clienteRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _clienteSucursalRepository = clienteSucursalRepository;
        _clienteRepository = clienteRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<ClienteSucursalDto>>> GetAllByClienteAsync(int clienteId, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        // Se mantiene la validación de empresa activa.
        if (!empresaIdResult.IsSuccess)
            return Result<IReadOnlyList<ClienteSucursalDto>>.Failure(empresaIdResult.Error!);

        var cliente = await _clienteRepository.GetByIdAsync(clienteId, ct);
        if (cliente is null || cliente.EmpresaId != empresaIdResult.Value)
            return Result<IReadOnlyList<ClienteSucursalDto>>.Failure("Cliente no encontrado para la empresa activa.");

        var sucursales = await _clienteSucursalRepository.FindAsync(x => x.ClienteId == clienteId, ct);
        var dtos = sucursales
            .OrderByDescending(x => x.EsPrincipal)
            .ThenBy(x => x.Nombre)
            .Select(MapToDto)
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<ClienteSucursalDto>>.Success(dtos);
    }

    public async Task<Result<ClienteSucursalDto>> GetByIdAsync(int clienteId, int id, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<ClienteSucursalDto>.Failure(empresaIdResult.Error!);

        var entity = await _clienteSucursalRepository.GetByIdAsync(id, ct);
        if (entity is null || entity.ClienteId != clienteId || entity.EmpresaId != empresaIdResult.Value)
            return Result<ClienteSucursalDto>.Failure("Sucursal de cliente no encontrada.");

        return Result<ClienteSucursalDto>.Success(MapToDto(entity));
    }

    public async Task<Result<ClienteSucursalDto>> CreateAsync(int clienteId, CreateClienteSucursalDto dto, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<ClienteSucursalDto>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;

        var cliente = await _clienteRepository.GetByIdAsync(clienteId, ct);
        if (cliente is null || cliente.EmpresaId != empresaId)
            return Result<ClienteSucursalDto>.Failure("Cliente no encontrado para la empresa activa.");

        var existentes = await _clienteSucursalRepository.FindAsync(x => x.ClienteId == clienteId, ct);

        // Validaciones de negocio
        if (string.IsNullOrWhiteSpace(dto.Codigo))
            return Result<ClienteSucursalDto>.Failure("El código de la sucursal es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<ClienteSucursalDto>.Failure("El nombre de la sucursal es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Direccion))
            return Result<ClienteSucursalDto>.Failure("La dirección de la sucursal es obligatoria.");

        if (!dto.PaisId.HasValue || dto.PaisId.Value <= 0)
            return Result<ClienteSucursalDto>.Failure("El país es obligatorio para la sucursal.");

        if (!string.IsNullOrWhiteSpace(dto.Email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(dto.Email))
            return Result<ClienteSucursalDto>.Failure("El formato del email no es válido.");

        if (existentes.Any(x => x.Codigo == dto.Codigo))
            return Result<ClienteSucursalDto>.Failure($"Ya existe una sucursal de cliente con código '{dto.Codigo}'.");

        var debeSerPrincipal = dto.EsPrincipal || !existentes.Any(x => x.Activo && x.EsPrincipal);

        if (debeSerPrincipal)
            await ClearPrincipalAsync(existentes, ct);

        var entity = new ClienteSucursal
        {
            EmpresaId = empresaId,
            ClienteId = clienteId,
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            AliasComercial = dto.AliasComercial,
            PaisId = dto.PaisId,
            DepartamentoId = dto.DepartamentoId,
            ProvinciaId = dto.ProvinciaId,
            CiudadId = dto.CiudadId,
            ZonaId = dto.ZonaId,
            Direccion = dto.Direccion,
            Referencia = dto.Referencia,
            UrlMapa = dto.UrlMapa,
            Latitud = dto.Latitud,
            Longitud = dto.Longitud,
            ContactoPrincipalId = dto.ContactoPrincipalId,
            ResponsableNombre = dto.ResponsableNombre,
            ResponsableCargo = dto.ResponsableCargo,
            Telefono = dto.Telefono,
            Celular = dto.Celular,
            WhatsApp = dto.WhatsApp,
            Email = dto.Email,
            EsPrincipal = debeSerPrincipal,
            Activo = dto.Activo,
            RecibePedidos = dto.RecibePedidos,
            RecibeFacturacion = dto.RecibeFacturacion,
            EsPuntoEntrega = dto.EsPuntoEntrega,
            Observaciones = dto.Observaciones
        };

        await _clienteSucursalRepository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ClienteSucursalDto>.Success(MapToDto(entity));
    }

    public async Task<Result<ClienteSucursalDto>> UpdateAsync(int clienteId, int id, UpdateClienteSucursalDto dto, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<ClienteSucursalDto>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;

        var entity = await _clienteSucursalRepository.GetByIdAsync(id, ct);
        if (entity is null || entity.ClienteId != clienteId || entity.EmpresaId != empresaId)
            return Result<ClienteSucursalDto>.Failure("Sucursal de cliente no encontrada.");

        var existentes = await _clienteSucursalRepository.FindAsync(x => x.ClienteId == clienteId, ct);

        // Validaciones de negocio
        if (string.IsNullOrWhiteSpace(dto.Codigo))
            return Result<ClienteSucursalDto>.Failure("El código de la sucursal es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<ClienteSucursalDto>.Failure("El nombre de la sucursal es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Direccion))
            return Result<ClienteSucursalDto>.Failure("La dirección de la sucursal es obligatoria.");

        if (!dto.PaisId.HasValue || dto.PaisId.Value <= 0)
            return Result<ClienteSucursalDto>.Failure("El país es obligatorio para la sucursal.");

        if (!string.IsNullOrWhiteSpace(dto.Email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(dto.Email))
            return Result<ClienteSucursalDto>.Failure("El formato del email no es válido.");

        if (existentes.Any(x => x.Id != id && x.Codigo == dto.Codigo))
            return Result<ClienteSucursalDto>.Failure($"Ya existe otra sucursal de cliente con código '{dto.Codigo}'.");

        if (dto.EsPrincipal)
            await ClearPrincipalAsync(existentes.Where(x => x.Id != id), ct);

        entity.Codigo = dto.Codigo;
        entity.Nombre = dto.Nombre;
        entity.AliasComercial = dto.AliasComercial;
        entity.PaisId = dto.PaisId;
        entity.DepartamentoId = dto.DepartamentoId;
        entity.ProvinciaId = dto.ProvinciaId;
        entity.CiudadId = dto.CiudadId;
        entity.ZonaId = dto.ZonaId;
        entity.Direccion = dto.Direccion;
        entity.Referencia = dto.Referencia;
        entity.UrlMapa = dto.UrlMapa;
        entity.Latitud = dto.Latitud;
        entity.Longitud = dto.Longitud;
        entity.ContactoPrincipalId = dto.ContactoPrincipalId;
        entity.ResponsableNombre = dto.ResponsableNombre;
        entity.ResponsableCargo = dto.ResponsableCargo;
        entity.Telefono = dto.Telefono;
        entity.Celular = dto.Celular;
        entity.WhatsApp = dto.WhatsApp;
        entity.Email = dto.Email;
        entity.EsPrincipal = dto.EsPrincipal;
        entity.Activo = dto.Activo;
        entity.RecibePedidos = dto.RecibePedidos;
        entity.RecibeFacturacion = dto.RecibeFacturacion;
        entity.EsPuntoEntrega = dto.EsPuntoEntrega;
        entity.Observaciones = dto.Observaciones;

        if (entity.Activo && !entity.EsPrincipal)
        {
            var otraPrincipalActiva = existentes.Any(x => x.Id != entity.Id && x.Activo && x.EsPrincipal);
            if (!otraPrincipalActiva)
                return Result<ClienteSucursalDto>.Failure("Debe existir una sucursal principal activa por cliente.");
        }

        await _clienteSucursalRepository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ClienteSucursalDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int clienteId, int id, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<bool>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;

        var entity = await _clienteSucursalRepository.GetByIdAsync(id, ct);
        if (entity is null || entity.ClienteId != clienteId || entity.EmpresaId != empresaId)
            return Result<bool>.Failure("Sucursal de cliente no encontrada.");

        var sucursales = await _clienteSucursalRepository.FindAsync(x => x.ClienteId == clienteId, ct);

        if (entity.EsPrincipal)
        {
            var reemplazo = sucursales
                .Where(x => x.Id != id && x.Activo)
                .OrderBy(x => x.Id)
                .FirstOrDefault();

            // Si hay un reemplazo activo, lo marcamos como principal.
            if (reemplazo is not null)
            {
                reemplazo.EsPrincipal = true;
                await _clienteSucursalRepository.UpdateAsync(reemplazo, ct);
            }
            // Si es la unica sucursal simplemente dejamos que se marque como inactiva (a menos que haya regla estricta que no deje que el cliente se quede sin sucursales principales activas)
        }

        entity.Activo = false;
        entity.EsPrincipal = false;

        await _clienteSucursalRepository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> SetPrincipalAsync(int clienteId, int id, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<bool>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;

        var entity = await _clienteSucursalRepository.GetByIdAsync(id, ct);
        if (entity is null || entity.ClienteId != clienteId || entity.EmpresaId != empresaId)
            return Result<bool>.Failure("Sucursal de cliente no encontrada.");

        if (!entity.Activo)
            return Result<bool>.Failure("No se puede establecer como principal una sucursal inactiva.");

        var sucursales = await _clienteSucursalRepository.FindAsync(x => x.ClienteId == clienteId, ct);
        await ClearPrincipalAsync(sucursales.Where(x => x.Id != id), ct);

        entity.EsPrincipal = true;
        await _clienteSucursalRepository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    private Result<int> TryGetEmpresaId()
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<int>.Failure("No se pudo determinar la empresa activa.");

        return Result<int>.Success(_currentUser.EmpresaId.Value);
    }

    private async Task ClearPrincipalAsync(IEnumerable<ClienteSucursal> sucursales, CancellationToken ct)
    {
        foreach (var sucursal in sucursales.Where(x => x.EsPrincipal))
        {
            sucursal.EsPrincipal = false;
            await _clienteSucursalRepository.UpdateAsync(sucursal, ct);
        }
    }

    private static ClienteSucursalDto MapToDto(ClienteSucursal e) => new()
    {
        Id = e.Id,
        ClienteId = e.ClienteId,
        EmpresaId = e.EmpresaId,
        Codigo = e.Codigo,
        Nombre = e.Nombre,
        AliasComercial = e.AliasComercial,
        PaisId = e.PaisId,
        DepartamentoId = e.DepartamentoId,
        ProvinciaId = e.ProvinciaId,
        CiudadId = e.CiudadId,
        ZonaId = e.ZonaId,
        Direccion = e.Direccion,
        Referencia = e.Referencia,
        UrlMapa = e.UrlMapa,
        Latitud = e.Latitud,
        Longitud = e.Longitud,
        ContactoPrincipalId = e.ContactoPrincipalId,
        ResponsableNombre = e.ResponsableNombre,
        ResponsableCargo = e.ResponsableCargo,
        Telefono = e.Telefono,
        Celular = e.Celular,
        WhatsApp = e.WhatsApp,
        Email = e.Email,
        EsPrincipal = e.EsPrincipal,
        Activo = e.Activo,
        RecibePedidos = e.RecibePedidos,
        RecibeFacturacion = e.RecibeFacturacion,
        EsPuntoEntrega = e.EsPuntoEntrega,
        Observaciones = e.Observaciones,
        FechaCreacion = e.FechaCreacion
    };
}
