using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.FE;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

namespace AgoraHub360.ERP.Application.Services;

/// <summary>
/// Servicio de aplicación para gestionar configuraciones FE por empresa.
/// Cifra secretos al guardar, garantiza una sola configuración activa,
/// y nunca expone secretos en DTOs de salida.
/// </summary>
public class ConfiguracionFEService : IConfiguracionFEService
{
    private readonly IConfiguracionFERepository _configRepo;
    private readonly IProveedorFERepository _proveedorRepo;
    private readonly IAmbienteFERepository _ambienteRepo;
    private readonly ICifradoService _cifradoService;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ConfiguracionFEService(
        IConfiguracionFERepository configRepo,
        IProveedorFERepository proveedorRepo,
        IAmbienteFERepository ambienteRepo,
        ICifradoService cifradoService,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _configRepo = configRepo;
        _proveedorRepo = proveedorRepo;
        _ambienteRepo = ambienteRepo;
        _cifradoService = cifradoService;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<ConfiguracionFEDto>>> ListarPorEmpresaAsync(
        int empresaId,
        CancellationToken cancellationToken = default)
    {
        ValidarEmpresaId();
        var configs = await _configRepo.ListarPorEmpresaAsync(empresaId, cancellationToken);
        var dtos = configs.Select(MapToDto).ToList();
        return Result<IReadOnlyList<ConfiguracionFEDto>>.Success(dtos);
    }

    public async Task<Result<ConfiguracionFEDto>> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = ObtenerEmpresaId();
        var config = await _configRepo.ObtenerPorIdAsync(id, cancellationToken);
        if (config == null)
            return Result<ConfiguracionFEDto>.Failure("Configuración FE no encontrada.");

        if (config.EmpresaId != empresaId)
            return Result<ConfiguracionFEDto>.Failure("No tiene permisos para acceder a esta configuración.");

        return Result<ConfiguracionFEDto>.Success(MapToDto(config));
    }

    public async Task<Result<ConfiguracionFEDto>> CrearAsync(
        CrearConfiguracionFERequestDto request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = ObtenerEmpresaId();
        ValidarRequest(request);

        var proveedor = await _proveedorRepo.ObtenerPorIdAsync(request.ProveedorFacturacionElectronicaId, cancellationToken);
        if (proveedor == null)
            return Result<ConfiguracionFEDto>.Failure("El proveedor FE seleccionado no existe o no está activo.");

        var ambiente = await _ambienteRepo.ObtenerPorIdAsync(request.AmbienteFacturacionElectronicaId, cancellationToken);
        if (ambiente == null)
            return Result<ConfiguracionFEDto>.Failure("El ambiente FE seleccionado no existe o no está activo.");

        if (string.IsNullOrWhiteSpace(request.ClientSecret))
            return Result<ConfiguracionFEDto>.Failure("ClientSecret es obligatorio.");

        if (proveedor.RequierePosToken && string.IsNullOrWhiteSpace(request.PosToken))
            return Result<ConfiguracionFEDto>.Failure("El proveedor seleccionado requiere PosToken.");

        var config = new ConfiguracionFacturacionElectronica
        {
            EmpresaId = empresaId,
            NombreConfiguracion = request.NombreConfiguracion,
            ProveedorFacturacionElectronicaId = request.ProveedorFacturacionElectronicaId,
            AmbienteFacturacionElectronicaId = request.AmbienteFacturacionElectronicaId,
            ClientId = request.ClientId,
            ClientSecretEncrypted = _cifradoService.Cifrar(request.ClientSecret),
            TokenUrl = request.TokenUrl,
            ApiManagementUrl = request.ApiManagementUrl,
            ApiBillingUrl = request.ApiBillingUrl,
            PosTokenEncrypted = _cifradoService.Cifrar(request.PosToken),
            SucursalFiscal = request.SucursalFiscal,
            PuntoVentaFiscal = request.PuntoVentaFiscal,
            ActivityCode = request.ActivityCode,
            NitEmisor = request.NitEmisor,
            TimeoutSegundos = request.TimeoutSegundos,
            EsConfiguracionActiva = request.EsConfiguracionActiva,
            Observaciones = request.Observaciones
        };

        if (request.EsConfiguracionActiva)
        {
            var activas = await _configRepo.ListarPorEmpresaAsync(empresaId, cancellationToken);
            foreach (var a in activas.Where(x => x.EsConfiguracionActiva))
            {
                a.EsConfiguracionActiva = false;
            }
        }

        await _configRepo.AgregarAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ConfiguracionFEDto>.Success(MapToDto(config));
    }

    public async Task<Result<ConfiguracionFEDto>> ActualizarAsync(
        int id,
        ActualizarConfiguracionFERequestDto request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = ObtenerEmpresaId();
        var config = await _configRepo.ObtenerPorIdAsync(id, cancellationToken);
        if (config == null)
            return Result<ConfiguracionFEDto>.Failure("Configuración FE no encontrada.");

        if (config.EmpresaId != empresaId)
            return Result<ConfiguracionFEDto>.Failure("No tiene permisos para modificar esta configuración.");

        var proveedor = await _proveedorRepo.ObtenerPorIdAsync(request.ProveedorFacturacionElectronicaId, cancellationToken);
        if (proveedor == null)
            return Result<ConfiguracionFEDto>.Failure("El proveedor FE seleccionado no existe o no está activo.");

        var ambiente = await _ambienteRepo.ObtenerPorIdAsync(request.AmbienteFacturacionElectronicaId, cancellationToken);
        if (ambiente == null)
            return Result<ConfiguracionFEDto>.Failure("El ambiente FE seleccionado no existe o no está activo.");

        config.NombreConfiguracion = request.NombreConfiguracion;
        config.ProveedorFacturacionElectronicaId = request.ProveedorFacturacionElectronicaId;
        config.AmbienteFacturacionElectronicaId = request.AmbienteFacturacionElectronicaId;
        config.ClientId = request.ClientId;
        config.TokenUrl = request.TokenUrl;
        config.ApiManagementUrl = request.ApiManagementUrl;
        config.ApiBillingUrl = request.ApiBillingUrl;
        config.SucursalFiscal = request.SucursalFiscal;
        config.PuntoVentaFiscal = request.PuntoVentaFiscal;
        config.ActivityCode = request.ActivityCode;
        config.NitEmisor = request.NitEmisor;
        config.TimeoutSegundos = request.TimeoutSegundos;
        config.Observaciones = request.Observaciones;

        if (!string.IsNullOrWhiteSpace(request.ClientSecret))
            config.ClientSecretEncrypted = _cifradoService.Cifrar(request.ClientSecret);

        if (!string.IsNullOrWhiteSpace(request.PosToken))
            config.PosTokenEncrypted = _cifradoService.Cifrar(request.PosToken);

        if (request.EsConfiguracionActiva && !config.EsConfiguracionActiva)
        {
            var activas = await _configRepo.ListarPorEmpresaAsync(empresaId, cancellationToken);
            foreach (var a in activas.Where(x => x.EsConfiguracionActiva && x.Id != config.Id))
            {
                a.EsConfiguracionActiva = false;
            }
        }
        config.EsConfiguracionActiva = request.EsConfiguracionActiva;

        await _configRepo.ActualizarAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ConfiguracionFEDto>.Success(MapToDto(config));
    }

    public async Task<Result<ConfiguracionFEDto>> ActivarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = ObtenerEmpresaId();
        var config = await _configRepo.ObtenerPorIdAsync(id, cancellationToken);
        if (config == null)
            return Result<ConfiguracionFEDto>.Failure("Configuración FE no encontrada.");

        if (config.EmpresaId != empresaId)
            return Result<ConfiguracionFEDto>.Failure("No tiene permisos para activar esta configuración.");

        var activas = await _configRepo.ListarPorEmpresaAsync(empresaId, cancellationToken);
        foreach (var a in activas.Where(x => x.EsConfiguracionActiva && x.Id != config.Id))
        {
            a.EsConfiguracionActiva = false;
        }

        config.EsConfiguracionActiva = true;
        await _configRepo.ActualizarAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ConfiguracionFEDto>.Success(MapToDto(config));
    }

    public async Task<Result<bool>> DesactivarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = ObtenerEmpresaId();
        var config = await _configRepo.ObtenerPorIdAsync(id, cancellationToken);
        if (config == null)
            return Result<bool>.Failure("Configuración FE no encontrada.");

        if (config.EmpresaId != empresaId)
            return Result<bool>.Failure("No tiene permisos para desactivar esta configuración.");

        config.EsConfiguracionActiva = false;
        await _configRepo.ActualizarAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<IReadOnlyList<ProveedorFEDto>>> ListarProveedoresAsync(
        CancellationToken cancellationToken = default)
    {
        var proveedores = await _proveedorRepo.ListarActivosAsync(cancellationToken);
        var dtos = proveedores.Select(p => new ProveedorFEDto
        {
            Id = p.Id,
            Codigo = p.Codigo,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            RequierePosToken = p.RequierePosToken,
            SoportaAnulacion = p.SoportaAnulacion,
            SoportaConsultaEstado = p.SoportaConsultaEstado,
            SoportaModoOffline = p.SoportaModoOffline,
            Activo = p.Activo
        }).ToList();

        return Result<IReadOnlyList<ProveedorFEDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<AmbienteFEDto>>> ListarAmbientesAsync(
        CancellationToken cancellationToken = default)
    {
        var ambientes = await _ambienteRepo.ListarActivosAsync(cancellationToken);
        var dtos = ambientes.Select(a => new AmbienteFEDto
        {
            Id = a.Id,
            Codigo = a.Codigo,
            Nombre = a.Nombre,
            Descripcion = a.Descripcion,
            EsProduccion = a.EsProduccion,
            Activo = a.Activo
        }).ToList();

        return Result<IReadOnlyList<AmbienteFEDto>>.Success(dtos);
    }

    // ── Métodos privados ─────────────────────────────────────────────────────

    private int ObtenerEmpresaId()
    {
        if (!_currentUser.EmpresaId.HasValue)
            throw new InvalidOperationException("No se pudo determinar la empresa activa.");
        return _currentUser.EmpresaId.Value;
    }

    private void ValidarEmpresaId()
    {
        if (!_currentUser.EmpresaId.HasValue)
            throw new InvalidOperationException("No se pudo determinar la empresa activa.");
    }

    private static void ValidarRequest(CrearConfiguracionFERequestDto request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.NombreConfiguracion, nameof(request.NombreConfiguracion));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.ProveedorFacturacionElectronicaId, nameof(request.ProveedorFacturacionElectronicaId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.AmbienteFacturacionElectronicaId, nameof(request.AmbienteFacturacionElectronicaId));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ClientId, nameof(request.ClientId));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TokenUrl, nameof(request.TokenUrl));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ApiManagementUrl, nameof(request.ApiManagementUrl));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ApiBillingUrl, nameof(request.ApiBillingUrl));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ActivityCode, nameof(request.ActivityCode));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.NitEmisor, nameof(request.NitEmisor));

        if (!request.TokenUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("TokenUrl debe iniciar con https://.", nameof(request.TokenUrl));

        if (!request.ApiManagementUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("ApiManagementUrl debe iniciar con https://.", nameof(request.ApiManagementUrl));

        if (!request.ApiBillingUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("ApiBillingUrl debe iniciar con https://.", nameof(request.ApiBillingUrl));

        if (request.TimeoutSegundos < 5 || request.TimeoutSegundos > 60)
            throw new ArgumentException("TimeoutSegundos debe estar entre 5 y 60.", nameof(request.TimeoutSegundos));
    }

    private static ConfiguracionFEDto MapToDto(ConfiguracionFacturacionElectronica config)
    {
        return new ConfiguracionFEDto
        {
            Id = config.Id,
            EmpresaId = config.EmpresaId,
            NombreConfiguracion = config.NombreConfiguracion,
            ProveedorFacturacionElectronicaId = config.ProveedorFacturacionElectronicaId,
            ProveedorCodigo = config.ProveedorFacturacionElectronica?.Codigo ?? string.Empty,
            ProveedorNombre = config.ProveedorFacturacionElectronica?.Nombre ?? string.Empty,
            AmbienteFacturacionElectronicaId = config.AmbienteFacturacionElectronicaId,
            AmbienteCodigo = config.AmbienteFacturacionElectronica?.Codigo ?? string.Empty,
            AmbienteNombre = config.AmbienteFacturacionElectronica?.Nombre ?? string.Empty,
            EsProduccion = config.AmbienteFacturacionElectronica?.EsProduccion ?? false,
            ClientId = config.ClientId,
            TieneClientSecret = !string.IsNullOrWhiteSpace(config.ClientSecretEncrypted),
            TokenUrl = config.TokenUrl,
            ApiManagementUrl = config.ApiManagementUrl,
            ApiBillingUrl = config.ApiBillingUrl,
            TienePosToken = !string.IsNullOrWhiteSpace(config.PosTokenEncrypted),
            SucursalFiscal = config.SucursalFiscal,
            PuntoVentaFiscal = config.PuntoVentaFiscal,
            ActivityCode = config.ActivityCode,
            NitEmisor = config.NitEmisor,
            TimeoutSegundos = config.TimeoutSegundos,
            EsConfiguracionActiva = config.EsConfiguracionActiva,
            Observaciones = config.Observaciones,
            Activo = config.Activo
        };
    }
}
