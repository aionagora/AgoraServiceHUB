using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.CXC;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.CxC;

namespace AgoraHub360.ERP.Application.Services;

public class ClienteCreditoConfiguracionService : IClienteCreditoConfiguracionService
{
    private readonly IRepository<ClienteCreditoConfiguracion> _configRepo;
    private readonly IRepository<Cliente> _clienteRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ClienteCreditoConfiguracionService(
        IRepository<ClienteCreditoConfiguracion> configRepo,
        IRepository<Cliente> clienteRepo,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _configRepo = configRepo;
        _clienteRepo = clienteRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ClienteCreditoConfiguracionDto>> GetByClienteAsync(int clienteId, CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<ClienteCreditoConfiguracionDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        // Validar que el cliente exista y pertenezca a la empresa activa
        var cliente = await _clienteRepo.GetByIdAsync(clienteId, ct);
        if (cliente is null || !cliente.Activo || cliente.EmpresaId != empresaId)
            return Result<ClienteCreditoConfiguracionDto>.Failure("El cliente especificado no existe o no pertenece a la empresa activa.");

        var config = (await _configRepo.FindAsync(
            x => x.EmpresaId == empresaId && x.ClienteId == clienteId && x.Activo, ct))
            .FirstOrDefault();

        if (config is null)
        {
            return Result<ClienteCreditoConfiguracionDto>.Success(new ClienteCreditoConfiguracionDto
            {
                ClienteId = clienteId,
                CreditoHabilitado = false,
                DiasCredito = 0,
                LimiteCredito = 0,
                Observaciones = string.Empty
            });
        }

        return Result<ClienteCreditoConfiguracionDto>.Success(new ClienteCreditoConfiguracionDto
        {
            ClienteId = config.ClienteId,
            CreditoHabilitado = config.CreditoHabilitado,
            DiasCredito = config.DiasCredito,
            LimiteCredito = config.LimiteCredito,
            Observaciones = config.Observaciones
        });
    }

    public async Task<Result<ClienteCreditoConfiguracionDto>> GuardarAsync(
        int clienteId,
        GuardarClienteCreditoConfiguracionRequestDto request,
        CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<ClienteCreditoConfiguracionDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        // Validaciones de entrada
        if (request.DiasCredito < 0)
            return Result<ClienteCreditoConfiguracionDto>.Failure("Los días de crédito no pueden ser negativos.");

        if (request.LimiteCredito < 0)
            return Result<ClienteCreditoConfiguracionDto>.Failure("El límite de crédito no puede ser negativo.");

        // Validar que el cliente exista y pertenezca a la empresa activa
        var cliente = await _clienteRepo.GetByIdAsync(clienteId, ct);
        if (cliente is null || !cliente.Activo || cliente.EmpresaId != empresaId)
            return Result<ClienteCreditoConfiguracionDto>.Failure("El cliente especificado no existe o no pertenece a la empresa activa.");

        var config = (await _configRepo.FindAsync(
            x => x.EmpresaId == empresaId && x.ClienteId == clienteId && x.Activo, ct))
            .FirstOrDefault();

        if (config is null)
        {
            config = new ClienteCreditoConfiguracion
            {
                EmpresaId = empresaId,
                ClienteId = clienteId,
                CreditoHabilitado = request.CreditoHabilitado,
                DiasCredito = request.DiasCredito,
                LimiteCredito = request.LimiteCredito,
                Observaciones = request.Observaciones ?? string.Empty,
                Activo = true
            };
            await _configRepo.AddAsync(config, ct);
        }
        else
        {
            config.CreditoHabilitado = request.CreditoHabilitado;
            config.DiasCredito = request.DiasCredito;
            config.LimiteCredito = request.LimiteCredito;
            config.Observaciones = request.Observaciones ?? string.Empty;
            await _configRepo.UpdateAsync(config, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ClienteCreditoConfiguracionDto>.Success(new ClienteCreditoConfiguracionDto
        {
            ClienteId = config.ClienteId,
            CreditoHabilitado = config.CreditoHabilitado,
            DiasCredito = config.DiasCredito,
            LimiteCredito = config.LimiteCredito,
            Observaciones = config.Observaciones
        });
    }
}
