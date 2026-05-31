namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ClientePerfilFiscalService : IClientePerfilFiscalService
{
    private readonly IRepository<ClientePerfilFiscal> _perfilRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<ClienteSucursal> _clienteSucursalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ClientePerfilFiscalService(
        IRepository<ClientePerfilFiscal> perfilRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<ClienteSucursal> clienteSucursalRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _perfilRepository = perfilRepository;
        _clienteRepository = clienteRepository;
        _clienteSucursalRepository = clienteSucursalRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<ClientePerfilFiscalDto>>> GetAllByClienteAsync(int clienteId, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<IReadOnlyList<ClientePerfilFiscalDto>>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;
        var cliente = await _clienteRepository.GetByIdAsync(clienteId, ct);
        if (cliente is null || cliente.EmpresaId != empresaId)
            return Result<IReadOnlyList<ClientePerfilFiscalDto>>.Failure("Cliente no encontrado para la empresa activa.");

        var perfiles = await _perfilRepository.FindAsync(x => x.EmpresaId == empresaId && x.ClienteId == clienteId, ct);
        var dtos = perfiles
            .OrderByDescending(x => x.EsPredeterminado)
            .ThenBy(x => x.Alias)
            .Select(MapToDto)
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<ClientePerfilFiscalDto>>.Success(dtos);
    }

    public async Task<Result<ClientePerfilFiscalDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<ClientePerfilFiscalDto>.Failure(empresaIdResult.Error!);

        var entity = await _perfilRepository.GetByIdAsync(id, ct);
        if (entity is null || entity.EmpresaId != empresaIdResult.Value)
            return Result<ClientePerfilFiscalDto>.Failure("Perfil fiscal no encontrado.");

        return Result<ClientePerfilFiscalDto>.Success(MapToDto(entity));
    }

    public async Task<Result<ClientePerfilFiscalDto>> CreateAsync(int clienteId, CreateClientePerfilFiscalDto dto, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<ClientePerfilFiscalDto>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;

        var cliente = await _clienteRepository.GetByIdAsync(clienteId, ct);
        if (cliente is null || cliente.EmpresaId != empresaId)
            return Result<ClientePerfilFiscalDto>.Failure("Cliente no encontrado para la empresa activa.");

        var validacionBasica = ValidarCamposBasicos(dto.Alias, dto.TipoDocumentoIdentidad, dto.NumeroDocumento, dto.RazonSocial, dto.EmailFactura);
        if (!validacionBasica.IsSuccess)
            return Result<ClientePerfilFiscalDto>.Failure(validacionBasica.Error!);

        if (dto.ClienteSucursalId.HasValue)
        {
            var sucursal = await _clienteSucursalRepository.GetByIdAsync(dto.ClienteSucursalId.Value, ct);
            if (sucursal is null || sucursal.EmpresaId != empresaId || sucursal.ClienteId != clienteId)
                return Result<ClientePerfilFiscalDto>.Failure("La sucursal seleccionada no pertenece al cliente o a la empresa activa.");
        }

        var numeroDocumento = dto.NumeroDocumento.Trim();
        var razonSocial = dto.RazonSocial.Trim();

        var duplicadosActivos = await _perfilRepository.FindAsync(
            x => x.EmpresaId == empresaId
                 && x.ClienteId == clienteId
                 && x.NumeroDocumento == numeroDocumento
                 && x.RazonSocial == razonSocial
                 && x.Activo,
            ct);

        if (duplicadosActivos.Any())
            return Result<ClientePerfilFiscalDto>.Failure("Ya existe un perfil fiscal activo con el mismo documento y razón social para este cliente.");

        var perfilesCliente = await _perfilRepository.FindAsync(
            x => x.EmpresaId == empresaId && x.ClienteId == clienteId,
            ct);

        var perfilesActivos = perfilesCliente.Where(x => x.Activo).ToList();
        var debeSerPredeterminado = dto.EsPredeterminado || !perfilesActivos.Any();

        if (debeSerPredeterminado)
            await ClearPredeterminadosAsync(perfilesActivos, ct);

        var entity = new ClientePerfilFiscal
        {
            EmpresaId = empresaId,
            ClienteId = clienteId,
            ClienteSucursalId = dto.ClienteSucursalId,
            Alias = dto.Alias.Trim(),
            TipoDocumentoIdentidad = dto.TipoDocumentoIdentidad.Trim(),
            NumeroDocumento = numeroDocumento,
            Complemento = dto.Complemento?.Trim(),
            RazonSocial = razonSocial,
            TipoPersona = dto.TipoPersona?.Trim(),
            TipoPerfilFiscal = dto.TipoPerfilFiscal?.Trim(),
            EmailFactura = dto.EmailFactura?.Trim(),
            TelefonoFactura = dto.TelefonoFactura?.Trim(),
            RequiereEmail = dto.RequiereEmail,
            EsPredeterminado = debeSerPredeterminado,
            ValidadoFacturacion = false,
            CodigoClienteApi = dto.CodigoClienteApi?.Trim(),
            CodigoExternoFacturacion = dto.CodigoExternoFacturacion?.Trim(),
            Observaciones = dto.Observaciones?.Trim(),
            Activo = true
        };

        await _perfilRepository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ClientePerfilFiscalDto>.Success(MapToDto(entity));
    }

    public async Task<Result<ClientePerfilFiscalDto>> UpdateAsync(long id, UpdateClientePerfilFiscalDto dto, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<ClientePerfilFiscalDto>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;

        var entity = await _perfilRepository.GetByIdAsync(id, ct);
        if (entity is null || entity.EmpresaId != empresaId)
            return Result<ClientePerfilFiscalDto>.Failure("Perfil fiscal no encontrado.");

        var validacionBasica = ValidarCamposBasicos(dto.Alias, dto.TipoDocumentoIdentidad, dto.NumeroDocumento, dto.RazonSocial, dto.EmailFactura);
        if (!validacionBasica.IsSuccess)
            return Result<ClientePerfilFiscalDto>.Failure(validacionBasica.Error!);

        if (dto.ClienteSucursalId.HasValue)
        {
            var sucursal = await _clienteSucursalRepository.GetByIdAsync(dto.ClienteSucursalId.Value, ct);
            if (sucursal is null || sucursal.EmpresaId != empresaId || sucursal.ClienteId != entity.ClienteId)
                return Result<ClientePerfilFiscalDto>.Failure("La sucursal seleccionada no pertenece al cliente o a la empresa activa.");
        }

        var numeroDocumento = dto.NumeroDocumento.Trim();
        var razonSocial = dto.RazonSocial.Trim();

        if (dto.Activo)
        {
            var duplicadosActivos = await _perfilRepository.FindAsync(
                x => x.EmpresaId == empresaId
                     && x.ClienteId == entity.ClienteId
                     && x.Id != id
                     && x.NumeroDocumento == numeroDocumento
                     && x.RazonSocial == razonSocial
                     && x.Activo,
                ct);

            if (duplicadosActivos.Any())
                return Result<ClientePerfilFiscalDto>.Failure("Ya existe otro perfil fiscal activo con el mismo documento y razón social para este cliente.");
        }

        var perfilesCliente = await _perfilRepository.FindAsync(
            x => x.EmpresaId == empresaId && x.ClienteId == entity.ClienteId,
            ct);

        var otrosActivos = perfilesCliente.Where(x => x.Id != entity.Id && x.Activo).ToList();

        if (!dto.Activo && entity.EsPredeterminado)
        {
            var reemplazo = otrosActivos.OrderBy(x => x.Id).FirstOrDefault();
            if (reemplazo is null)
                return Result<ClientePerfilFiscalDto>.Failure("No se puede desactivar el único perfil fiscal predeterminado activo sin reasignar otro.");

            reemplazo.EsPredeterminado = true;
            await _perfilRepository.UpdateAsync(reemplazo, ct);
        }

        if (dto.Activo && dto.EsPredeterminado)
            await ClearPredeterminadosAsync(otrosActivos, ct);

        if (dto.Activo && !dto.EsPredeterminado && entity.EsPredeterminado)
        {
            var otroPredeterminadoActivo = otrosActivos.Any(x => x.EsPredeterminado);
            if (!otroPredeterminadoActivo)
                return Result<ClientePerfilFiscalDto>.Failure("Debe existir un perfil fiscal predeterminado activo por cliente.");
        }

        entity.ClienteSucursalId = dto.ClienteSucursalId;
        entity.Alias = dto.Alias.Trim();
        entity.TipoDocumentoIdentidad = dto.TipoDocumentoIdentidad.Trim();
        entity.NumeroDocumento = numeroDocumento;
        entity.Complemento = dto.Complemento?.Trim();
        entity.RazonSocial = razonSocial;
        entity.TipoPersona = dto.TipoPersona?.Trim();
        entity.TipoPerfilFiscal = dto.TipoPerfilFiscal?.Trim();
        entity.EmailFactura = dto.EmailFactura?.Trim();
        entity.TelefonoFactura = dto.TelefonoFactura?.Trim();
        entity.RequiereEmail = dto.RequiereEmail;
        entity.EsPredeterminado = dto.Activo && dto.EsPredeterminado;
        entity.CodigoClienteApi = dto.CodigoClienteApi?.Trim();
        entity.CodigoExternoFacturacion = dto.CodigoExternoFacturacion?.Trim();
        entity.Observaciones = dto.Observaciones?.Trim();
        entity.Activo = dto.Activo;

        await _perfilRepository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ClientePerfilFiscalDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<bool>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;

        var entity = await _perfilRepository.GetByIdAsync(id, ct);
        if (entity is null || entity.EmpresaId != empresaId)
            return Result<bool>.Failure("Perfil fiscal no encontrado.");

        if (!entity.Activo)
            return Result<bool>.Success(true);

        var activosDelCliente = await _perfilRepository.FindAsync(
            x => x.EmpresaId == empresaId
                 && x.ClienteId == entity.ClienteId
                 && x.Activo
                 && x.Id != entity.Id,
            ct);

        if (entity.EsPredeterminado)
        {
            var reemplazo = activosDelCliente.OrderBy(x => x.Id).FirstOrDefault();
            if (reemplazo is null)
                return Result<bool>.Failure("No se puede eliminar el único perfil fiscal predeterminado activo sin reasignar otro.");

            reemplazo.EsPredeterminado = true;
            await _perfilRepository.UpdateAsync(reemplazo, ct);
        }

        entity.Activo = false;
        entity.EsPredeterminado = false;

        await _perfilRepository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> SetPredeterminadoAsync(long id, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<bool>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;

        var entity = await _perfilRepository.GetByIdAsync(id, ct);
        if (entity is null || entity.EmpresaId != empresaId)
            return Result<bool>.Failure("Perfil fiscal no encontrado.");

        if (!entity.Activo)
            return Result<bool>.Failure("No se puede establecer como predeterminado un perfil fiscal inactivo.");

        var perfilesActivos = await _perfilRepository.FindAsync(
            x => x.EmpresaId == empresaId
                 && x.ClienteId == entity.ClienteId
                 && x.Activo
                 && x.Id != entity.Id,
            ct);

        await ClearPredeterminadosAsync(perfilesActivos, ct);

        entity.EsPredeterminado = true;
        await _perfilRepository.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    private Result<int> TryGetEmpresaId()
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<int>.Failure("No se pudo determinar la empresa activa.");

        return Result<int>.Success(_currentUser.EmpresaId.Value);
    }

    private async Task ClearPredeterminadosAsync(IEnumerable<ClientePerfilFiscal> perfiles, CancellationToken ct)
    {
        foreach (var perfil in perfiles.Where(x => x.EsPredeterminado))
        {
            perfil.EsPredeterminado = false;
            await _perfilRepository.UpdateAsync(perfil, ct);
        }
    }

    private static Result<bool> ValidarCamposBasicos(
        string? alias,
        string? tipoDocumentoIdentidad,
        string? numeroDocumento,
        string? razonSocial,
        string? emailFactura)
    {
        if (string.IsNullOrWhiteSpace(alias))
            return Result<bool>.Failure("El alias es obligatorio.");

        if (string.IsNullOrWhiteSpace(tipoDocumentoIdentidad))
            return Result<bool>.Failure("El tipo de documento de identidad es obligatorio.");

        if (string.IsNullOrWhiteSpace(numeroDocumento))
            return Result<bool>.Failure("El número de documento es obligatorio.");

        if (string.IsNullOrWhiteSpace(razonSocial))
            return Result<bool>.Failure("La razón social es obligatoria.");

        if (!string.IsNullOrWhiteSpace(emailFactura)
            && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(emailFactura))
            return Result<bool>.Failure("El formato del email de facturación no es válido.");

        return Result<bool>.Success(true);
    }

    private static ClientePerfilFiscalDto MapToDto(ClientePerfilFiscal e) => new()
    {
        Id = e.Id,
        ClienteId = e.ClienteId,
        ClienteSucursalId = e.ClienteSucursalId,
        Alias = e.Alias,
        TipoDocumentoIdentidad = e.TipoDocumentoIdentidad,
        NumeroDocumento = e.NumeroDocumento,
        Complemento = e.Complemento,
        RazonSocial = e.RazonSocial,
        TipoPersona = e.TipoPersona,
        TipoPerfilFiscal = e.TipoPerfilFiscal,
        EmailFactura = e.EmailFactura,
        TelefonoFactura = e.TelefonoFactura,
        RequiereEmail = e.RequiereEmail,
        EsPredeterminado = e.EsPredeterminado,
        ValidadoFacturacion = e.ValidadoFacturacion,
        CodigoClienteApi = e.CodigoClienteApi,
        CodigoExternoFacturacion = e.CodigoExternoFacturacion,
        Observaciones = e.Observaciones,
        Activo = e.Activo,
        FechaCreacion = e.FechaCreacion
    };
}
