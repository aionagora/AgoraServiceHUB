using System.Diagnostics;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.FE;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;
using Microsoft.Extensions.Logging;

namespace AgoraHub360.ERP.Application.Services;

/// <summary>
/// Servicio de aplicación para operaciones de Facturación Electrónica.
/// Orquesta: configuración activa → resolución de provider → emisión/anulación/consulta → auditoría.
/// </summary>
public class FacturacionElectronicaService : IFacturacionElectronicaService
{
    private readonly IConfiguracionFERepository _configRepo;
    private readonly IAuditoriaFERepository _auditoriaRepo;
    private readonly IEnumerable<IFacturacionElectronicaProvider> _providers;
    private readonly IRepository<FacturaVenta> _facturaRepo;
    private readonly IRepository<FacturaVentaDetalle> _facturaDetalleRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FacturacionElectronicaService> _logger;

    public FacturacionElectronicaService(
        IConfiguracionFERepository configRepo,
        IAuditoriaFERepository auditoriaRepo,
        IEnumerable<IFacturacionElectronicaProvider> providers,
        IRepository<FacturaVenta> facturaRepo,
        IRepository<FacturaVentaDetalle> facturaDetalleRepo,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        ILogger<FacturacionElectronicaService> logger)
    {
        _configRepo = configRepo;
        _auditoriaRepo = auditoriaRepo;
        _providers = providers;
        _facturaRepo = facturaRepo;
        _facturaDetalleRepo = facturaDetalleRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  EmitirAsync
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<Result<EmisionFacturaResultDto>> EmitirAsync(
        long facturaVentaId,
        CancellationToken cancellationToken = default)
    {
        var empresaId = ObtenerEmpresaId();

        // 1. Cargar FacturaVenta
        var factura = await _facturaRepo.GetByIdAsync(facturaVentaId, cancellationToken);
        if (factura == null)
            return Result<EmisionFacturaResultDto>.Failure($"FacturaVenta {facturaVentaId} no encontrada.");
        if (factura.EmpresaId != empresaId)
            return Result<EmisionFacturaResultDto>.Failure("No tiene permisos sobre esta factura.");

        // 2. Validar estado emitible
        // Permitir emisión si está en Borrador o Generada (sin CUF aún)
        var esBorrador = factura.EstadoFactura == EstadoFacturaVentaComercial.Borrador;
        var esGenerada = factura.EstadoFactura == EstadoFacturaVentaComercial.Generada;

        if (!esBorrador && !esGenerada)
            return Result<EmisionFacturaResultDto>.Failure(
                $"La factura está en estado '{factura.EstadoFactura}'. Solo se pueden emitir facturas en estado Borrador o Generada.");

        if (esGenerada && !string.IsNullOrWhiteSpace(factura.Cuf))
            return Result<EmisionFacturaResultDto>.Failure(
                "La factura ya tiene un CUF asignado. No es posible emitirla nuevamente.");

        if (factura.EstadoSiat != EstadoSiatFactura.NoEnviada && factura.EstadoSiat != EstadoSiatFactura.Pendiente)
            return Result<EmisionFacturaResultDto>.Failure(
                $"La factura tiene estado SIAT '{factura.EstadoSiat}'. No es posible emitirla.");

        // 3. Cargar detalles
        var detalles = await _facturaDetalleRepo.FindAsync(
            d => d.FacturaVentaId == facturaVentaId && d.Activo, cancellationToken);

        if (detalles.Count == 0)
            return Result<EmisionFacturaResultDto>.Failure("La factura no tiene detalles. No se puede emitir.");

        // 4. Validar ItemCode
        var sinItemCode = detalles.Where(d => string.IsNullOrWhiteSpace(d.ItemCode)).ToList();
        if (sinItemCode.Count != 0)
        {
            var lineas = string.Join(", ", sinItemCode.Select(d => $"DetalleId={d.Id}"));
            return Result<EmisionFacturaResultDto>.Failure(
                $"Los siguientes detalles no tienen ItemCode: {lineas}. Complete los códigos antes de emitir.");
        }

        // 5. Obtener configuración activa
        var config = await _configRepo.ObtenerActivaPorEmpresaAsync(empresaId, cancellationToken);
        if (config == null)
            return Result<EmisionFacturaResultDto>.Failure("No existe configuración activa de facturación electrónica para esta empresa.");

        if (config.ProveedorFacturacionElectronica == null)
        {
            // Recargar con include si es necesario
            config = await _configRepo.ObtenerPorIdAsync(config.Id, cancellationToken);
            if (config?.ProveedorFacturacionElectronica == null)
                return Result<EmisionFacturaResultDto>.Failure("La configuración FE no tiene un proveedor asignado.");
        }

        // 6. Resolver provider por CodigoProveedor
        var provider = ResolverProvider(config.ProveedorFacturacionElectronica.Codigo);
        if (provider == null)
            return Result<EmisionFacturaResultDto>.Failure(
                $"No se encontró un proveedor FE registrado para el código '{config.ProveedorFacturacionElectronica.Codigo}'.");

        // 7. Idempotencia: BillUuid
        if (string.IsNullOrWhiteSpace(factura.BillUuid))
        {
            factura.BillUuid = Guid.NewGuid().ToString();
        }

        // 8. Construir request
        var request = ConstruirEmitirRequest(factura, detalles, config);

        // 9. Ejecutar emisión
        var stopwatch = Stopwatch.StartNew();
        EmisionFacturaResultDto result;

        try
        {
            result = await provider.EmitirFacturaAsync(config, request, cancellationToken);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error al emitir factura {FacturaVentaId} con proveedor {Proveedor}",
                facturaVentaId, config.ProveedorFacturacionElectronica.Codigo);

            result = new EmisionFacturaResultDto
            {
                Exitoso = false,
                EstadoSiat = factura.EstadoSiat.ToString(),
                Mensaje = $"Error de comunicación con el proveedor: {ex.Message}"
            };
        }
        finally
        {
            stopwatch.Stop();
        }

        // 10. Actualizar factura según resultado
        if (result.Exitoso)
        {
            if (!string.IsNullOrWhiteSpace(result.Cuf))
                factura.Cuf = result.Cuf;
            if (!string.IsNullOrWhiteSpace(result.Cufd))
                factura.Cufd = result.Cufd;
            if (!string.IsNullOrWhiteSpace(result.SiatQr))
                factura.SiatQr = result.SiatQr;
            if (!string.IsNullOrWhiteSpace(result.EnlacePdf))
                factura.EnlacePdf = result.EnlacePdf;
            if (!string.IsNullOrWhiteSpace(result.EnlaceXml))
                factura.EnlaceXml = result.EnlaceXml;

            factura.EstadoSiat = MapearEstadoSiat(result.EstadoSiat);
            factura.EstadoFactura = EstadoFacturaVentaComercial.Generada;
        }
        else
        {
            factura.EstadoSiat = MapearEstadoSiat(result.EstadoSiat);
        }

        // 11. Registrar auditoría (incluso en fallo)
        var auditoria = new AuditoriaFacturacion
        {
            EmpresaId = empresaId,
            FacturaVentaId = facturaVentaId,
            ProveedorCodigo = config.ProveedorFacturacionElectronica.Codigo,
            ProveedorNombre = config.ProveedorFacturacionElectronica.Nombre,
            AmbienteCodigo = config.AmbienteFacturacionElectronica?.Codigo ?? string.Empty,
            AmbienteNombre = config.AmbienteFacturacionElectronica?.Nombre ?? string.Empty,
            BillUuid = factura.BillUuid ?? string.Empty,
            Cuf = factura.Cuf,
            EstadoSiat = factura.EstadoSiat,
            MensajeError = result.Mensaje,
            UsuarioId = _currentUser.UserId ?? "unknown",
            FechaHora = DateTime.UtcNow,
            TiempoRespuestaMs = stopwatch.ElapsedMilliseconds,
            CodigoRespuestaProveedor = result.CodigoRespuestaProveedor,
            DescripcionRespuestaProveedor = result.DescripcionRespuestaProveedor,
            Exitoso = result.Exitoso
        };

        await _auditoriaRepo.RegistrarAsync(auditoria, cancellationToken);

        // 12. Guardar
        await _facturaRepo.UpdateAsync(factura, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<EmisionFacturaResultDto>.Success(result);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  AnularAsync
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<Result<AnulacionFacturaResultDto>> AnularAsync(
        long facturaVentaId,
        string motivoAnulacion,
        CancellationToken cancellationToken = default)
    {
        var empresaId = ObtenerEmpresaId();

        if (string.IsNullOrWhiteSpace(motivoAnulacion) || motivoAnulacion.Trim().Length < 5)
            return Result<AnulacionFacturaResultDto>.Failure("El motivo de anulación debe tener al menos 5 caracteres.");

        var factura = await _facturaRepo.GetByIdAsync(facturaVentaId, cancellationToken);
        if (factura == null)
            return Result<AnulacionFacturaResultDto>.Failure($"FacturaVenta {facturaVentaId} no encontrada.");
        if (factura.EmpresaId != empresaId)
            return Result<AnulacionFacturaResultDto>.Failure("No tiene permisos sobre esta factura.");

        if (string.IsNullOrWhiteSpace(factura.Cuf))
            return Result<AnulacionFacturaResultDto>.Failure("La factura no tiene CUF. No es posible anularla.");

        var config = await _configRepo.ObtenerActivaPorEmpresaAsync(empresaId, cancellationToken);
        if (config?.ProveedorFacturacionElectronica == null)
        {
            config = await _configRepo.ObtenerPorIdAsync(config?.Id ?? 0, cancellationToken);
            if (config?.ProveedorFacturacionElectronica == null)
                return Result<AnulacionFacturaResultDto>.Failure("No existe configuración activa de FE o no tiene proveedor asignado.");
        }

        var provider = ResolverProvider(config.ProveedorFacturacionElectronica.Codigo);
        if (provider == null)
            return Result<AnulacionFacturaResultDto>.Failure(
                $"No se encontró un proveedor FE registrado para el código '{config.ProveedorFacturacionElectronica.Codigo}'.");

        var stopwatch = Stopwatch.StartNew();
        AnulacionFacturaResultDto result;

        try
        {
            result = await provider.AnularFacturaAsync(config, factura.Cuf, motivoAnulacion, cancellationToken);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error al anular factura {FacturaVentaId}", facturaVentaId);
            result = new AnulacionFacturaResultDto
            {
                Exitoso = false,
                Mensaje = $"Error de comunicación con el proveedor: {ex.Message}"
            };
        }
        finally
        {
            stopwatch.Stop();
        }

        if (result.Exitoso)
        {
            factura.EstadoSiat = EstadoSiatFactura.Anulada;
            factura.EstadoFactura = EstadoFacturaVentaComercial.Anulada;
            factura.MotivoAnulacion = motivoAnulacion;
            factura.FechaAnulacion = DateTime.UtcNow;
        }

        var auditoria = new AuditoriaFacturacion
        {
            EmpresaId = empresaId,
            FacturaVentaId = facturaVentaId,
            ProveedorCodigo = config.ProveedorFacturacionElectronica.Codigo,
            ProveedorNombre = config.ProveedorFacturacionElectronica.Nombre,
            AmbienteCodigo = config.AmbienteFacturacionElectronica?.Codigo ?? string.Empty,
            AmbienteNombre = config.AmbienteFacturacionElectronica?.Nombre ?? string.Empty,
            BillUuid = factura.BillUuid ?? string.Empty,
            Cuf = factura.Cuf,
            EstadoSiat = factura.EstadoSiat,
            MensajeError = result.Mensaje,
            UsuarioId = _currentUser.UserId ?? "unknown",
            FechaHora = DateTime.UtcNow,
            TiempoRespuestaMs = stopwatch.ElapsedMilliseconds,
            CodigoRespuestaProveedor = result.CodigoRespuestaProveedor,
            DescripcionRespuestaProveedor = result.DescripcionRespuestaProveedor,
            Exitoso = result.Exitoso
        };

        await _auditoriaRepo.RegistrarAsync(auditoria, cancellationToken);
        await _facturaRepo.UpdateAsync(factura, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AnulacionFacturaResultDto>.Success(result);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  VerificarEstadoAsync
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<Result<EstadoFacturaResultDto>> VerificarEstadoAsync(
        long facturaVentaId,
        CancellationToken cancellationToken = default)
    {
        var empresaId = ObtenerEmpresaId();

        var factura = await _facturaRepo.GetByIdAsync(facturaVentaId, cancellationToken);
        if (factura == null)
            return Result<EstadoFacturaResultDto>.Failure($"FacturaVenta {facturaVentaId} no encontrada.");
        if (factura.EmpresaId != empresaId)
            return Result<EstadoFacturaResultDto>.Failure("No tiene permisos sobre esta factura.");

        if (string.IsNullOrWhiteSpace(factura.Cuf))
            return Result<EstadoFacturaResultDto>.Failure("La factura no tiene CUF. No es posible consultar estado.");

        var config = await _configRepo.ObtenerActivaPorEmpresaAsync(empresaId, cancellationToken);
        if (config?.ProveedorFacturacionElectronica == null)
            return Result<EstadoFacturaResultDto>.Failure("No existe configuración activa de FE.");

        var provider = ResolverProvider(config.ProveedorFacturacionElectronica.Codigo);
        if (provider == null)
            return Result<EstadoFacturaResultDto>.Failure(
                $"No se encontró un proveedor FE registrado para el código '{config.ProveedorFacturacionElectronica.Codigo}'.");

        var stopwatch = Stopwatch.StartNew();
        EstadoFacturaResultDto result;

        try
        {
            result = await provider.VerificarEstadoAsync(config, factura.Cuf, cancellationToken);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error al consultar estado de factura {FacturaVentaId}", facturaVentaId);
            result = new EstadoFacturaResultDto
            {
                Exitoso = false,
                Mensaje = $"Error de comunicación con el proveedor: {ex.Message}"
            };
        }
        finally
        {
            stopwatch.Stop();
        }

        if (result.Exitoso)
        {
            factura.EstadoSiat = MapearEstadoSiat(result.EstadoSiat);
            await _facturaRepo.UpdateAsync(factura, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<EstadoFacturaResultDto>.Success(result);
    }

    // ── Métodos privados ─────────────────────────────────────────────────────

    private int ObtenerEmpresaId()
    {
        if (!_currentUser.EmpresaId.HasValue)
            throw new InvalidOperationException("No se pudo determinar la empresa activa.");
        return _currentUser.EmpresaId.Value;
    }

    private IFacturacionElectronicaProvider? ResolverProvider(string codigoProveedor)
    {
        var codigoNormalizado = codigoProveedor.Trim().ToUpperInvariant();
        return _providers.FirstOrDefault(p =>
            string.Equals(p.CodigoProveedor.Trim().ToUpperInvariant(), codigoNormalizado, StringComparison.Ordinal));
    }

    private static EmitirFacturaRequestDto ConstruirEmitirRequest(
        FacturaVenta factura,
        IReadOnlyList<FacturaVentaDetalle> detalles,
        ConfiguracionFacturacionElectronica config)
    {
        var request = new EmitirFacturaRequestDto
        {
            FacturaVentaId = factura.Id,
            VentaId = factura.VentaId,
            EmpresaId = factura.EmpresaId,
            BillUuid = factura.BillUuid ?? string.Empty,
            ActivityCode = factura.ActivityCode ?? config.ActivityCode,
            NitEmisor = config.NitEmisor,
            BeneficiaryDocNumber = factura.NitFactura,
            DocNumberComplement = factura.Complemento,
            IdentityDocTypeCode = factura.IdentityDocTypeCode ?? "NIT",
            BillingName = factura.RazonSocialFactura,
            BeneficiaryName = factura.BeneficiaryName ?? factura.RazonSocialFactura,
            BeneficiaryEmail = factura.EmailFactura,
            PaymentMethodCode = factura.PaymentMethodCode ?? "1",
            CardNumber = factura.CardNumber,
            AdditionalDiscount = factura.AdditionalDiscount ?? 0,
            GiftCardAmount = factura.GiftCardAmount ?? 0,
            Items = detalles.Select(d => new FacturacionFEItemDto
            {
                ItemCode = d.ItemCode ?? string.Empty,
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                DescuentoMonto = d.DescuentoMonto,
                DetalleAdicional = d.DetalleAdicional
            }).ToList()
        };

        return request;
    }

    private static EstadoSiatFactura MapearEstadoSiat(string estadoSiat)
    {
        if (string.IsNullOrWhiteSpace(estadoSiat))
            return EstadoSiatFactura.NoEnviada;

        var upper = estadoSiat.Trim().ToUpperInvariant();

        return upper switch
        {
            "VALIDATED" or "VALIDADA" or "VALIDADO" or "ACEPTADA" or "ACEPTADO" => EstadoSiatFactura.Validada,
            "PENDING" or "PENDIENTE" or "OFFLINE" => EstadoSiatFactura.Pendiente,
            "REJECTED" or "RECHAZADA" or "RECHAZADO" => EstadoSiatFactura.Rechazada,
            "ANNULLED" or "ANULADA" or "ANULADO" => EstadoSiatFactura.Anulada,
            _ => EstadoSiatFactura.NoEnviada
        };
    }
}
