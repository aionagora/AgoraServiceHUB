namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Ventas;

public class FacturaVentaService : IFacturaVentaService
{
    private readonly IRepository<FacturaVenta> _facturaRepo;
    private readonly IRepository<FacturaVentaDetalle> _facturaDetalleRepo;
    private readonly IRepository<Venta> _ventaRepo;
    private readonly IRepository<VentaDetalle> _ventaDetalleRepo;
    private readonly IRepository<VentaFacturacionDatos> _ventaFacturacionRepo;
    private readonly IRepository<Cliente> _clienteRepo;
    private readonly IRepository<ClientePerfilFiscal> _clientePerfilFiscalRepo;
    private readonly IRepository<CompanyProduct> _companyProductRepo;
    private readonly IRepository<Uom> _uomRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public FacturaVentaService(
        IRepository<FacturaVenta> facturaRepo,
        IRepository<FacturaVentaDetalle> facturaDetalleRepo,
        IRepository<Venta> ventaRepo,
        IRepository<VentaDetalle> ventaDetalleRepo,
        IRepository<VentaFacturacionDatos> ventaFacturacionRepo,
        IRepository<Cliente> clienteRepo,
        IRepository<ClientePerfilFiscal> clientePerfilFiscalRepo,
        IRepository<CompanyProduct> companyProductRepo,
        IRepository<Uom> uomRepo,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _facturaRepo = facturaRepo;
        _facturaDetalleRepo = facturaDetalleRepo;
        _ventaRepo = ventaRepo;
        _ventaDetalleRepo = ventaDetalleRepo;
        _ventaFacturacionRepo = ventaFacturacionRepo;
        _clienteRepo = clienteRepo;
        _clientePerfilFiscalRepo = clientePerfilFiscalRepo;
        _companyProductRepo = companyProductRepo;
        _uomRepo = uomRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<FacturaVentaResumenDto>>> GetAllAsync(CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<IReadOnlyList<FacturaVentaResumenDto>>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var facturas = await _facturaRepo.FindAsync(f => f.EmpresaId == empresaId, ct);

        var result = facturas
            .OrderByDescending(f => f.FechaEmision)
            .ThenByDescending(f => f.Id)
            .Select(MapToResumenDto)
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<FacturaVentaResumenDto>>.Success(result);
    }

    public async Task<Result<FacturaVentaDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<FacturaVentaDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;
        var factura = await _facturaRepo.GetByIdAsync(id, ct);

        if (factura is null)
            return Result<FacturaVentaDto>.Failure("Factura de venta no encontrada.");

        if (factura.EmpresaId != empresaId)
            return Result<FacturaVentaDto>.Failure("La factura no pertenece a la empresa activa.");

        var detalles = await _facturaDetalleRepo.FindAsync(d => d.EmpresaId == empresaId && d.FacturaVentaId == factura.Id, ct);

        return Result<FacturaVentaDto>.Success(MapToDto(factura, detalles));
    }

    public async Task<Result<FacturaVentaDto>> GetByVentaIdAsync(long ventaId, CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<FacturaVentaDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var factura = (await _facturaRepo.FindAsync(f => f.EmpresaId == empresaId && f.VentaId == ventaId, ct))
            .OrderByDescending(f => f.Id)
            .FirstOrDefault();

        if (factura is null)
            return Result<FacturaVentaDto>.Failure("No existe factura para la venta indicada.");

        var detalles = await _facturaDetalleRepo.FindAsync(d => d.EmpresaId == empresaId && d.FacturaVentaId == factura.Id, ct);

        return Result<FacturaVentaDto>.Success(MapToDto(factura, detalles));
    }

    public async Task<Result<FacturaVentaDto>> GenerarDesdeVentaAsync(GenerarFacturaVentaRequestDto dto, CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<FacturaVentaDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var venta = await _ventaRepo.GetByIdAsync(dto.VentaId, ct);
        if (venta is null)
            return Result<FacturaVentaDto>.Failure("La venta no existe.");

        if (venta.EmpresaId != empresaId)
            return Result<FacturaVentaDto>.Failure("La venta no pertenece a la empresa activa.");

        if (venta.EstadoVenta == EstadoVenta.Anulada)
            return Result<FacturaVentaDto>.Failure("No se puede facturar una venta anulada.");

        var ventaDetalles = await _ventaDetalleRepo.FindAsync(d => d.EmpresaId == empresaId && d.VentaId == venta.Id && d.Activo, ct);
        if (ventaDetalles.Count == 0)
            return Result<FacturaVentaDto>.Failure("No se puede generar factura para una venta sin detalles.");

        // Fase 1/2A: una venta solo puede tener una factura, incluso anulada.
        var facturaExistente = (await _facturaRepo.FindAsync(f => f.EmpresaId == empresaId && f.VentaId == venta.Id, ct))
            .FirstOrDefault();
        if (facturaExistente is not null)
            return Result<FacturaVentaDto>.Failure("La venta ya tiene una FacturaVenta registrada en esta fase; no se permite refacturación.");

        if (!TryParseEnum(dto.TipoDocumentoFactura, out TipoDocumentoFactura tipoDocumentoFactura))
            return Result<FacturaVentaDto>.Failure($"TipoDocumentoFactura inválido: '{dto.TipoDocumentoFactura}'.");

        var fiscalData = await ResolveFiscalDataAsync(empresaId, venta, dto, ct);
        if (!fiscalData.IsSuccess)
            return Result<FacturaVentaDto>.Failure(fiscalData.Error!);

        var fiscal = fiscalData.Value!;

        if (string.IsNullOrWhiteSpace(fiscal.NitFactura))
            return Result<FacturaVentaDto>.Failure("No se pudo determinar NitFactura para generar la factura.");

        if (string.IsNullOrWhiteSpace(fiscal.RazonSocialFactura))
            return Result<FacturaVentaDto>.Failure("No se pudo determinar RazonSocialFactura para generar la factura.");

        var numeroFactura = await GenerateNumeroFacturaAsync(empresaId, ct);

        var factura = new FacturaVenta
        {
            EmpresaId = empresaId,
            VentaId = venta.Id,
            NumeroFactura = numeroFactura,
            FechaEmision = DateTime.Now,
            TipoDocumentoFactura = tipoDocumentoFactura,
            EstadoFactura = EstadoFacturaVentaComercial.Generada,
            EstadoSiat = EstadoSiatFactura.NoEnviada,
            ClientePerfilFiscalId = fiscal.ClientePerfilFiscalId,
            NitFactura = fiscal.NitFactura!,
            Complemento = fiscal.Complemento,
            RazonSocialFactura = fiscal.RazonSocialFactura!,
            EmailFactura = fiscal.EmailFactura,
            TelefonoFactura = fiscal.TelefonoFactura,
            MonedaCodigo = string.IsNullOrWhiteSpace(venta.MonedaCodigo) ? venta.MonedaId : venta.MonedaCodigo,
            TipoCambio = venta.TipoCambio <= 0 ? 1m : venta.TipoCambio,
            Subtotal = venta.Subtotal,
            DescuentoTotal = venta.DescuentoTotal,
            ImpuestoTotal = venta.ImpuestoTotal,
            Total = venta.Total,
            Observaciones = dto.Observaciones ?? venta.Observaciones,
            Activo = true
        };

        await _facturaRepo.AddAsync(factura, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var companyProductIds = ventaDetalles
            .Where(d => d.CompanyProductId.HasValue)
            .Select(d => d.CompanyProductId!.Value)
            .Distinct()
            .ToList();

        var companyProducts = companyProductIds.Count == 0
            ? new List<CompanyProduct>()
            : (await _companyProductRepo.FindAsync(cp => cp.EmpresaId == empresaId && companyProductIds.Contains(cp.CompanyProductId), ct)).ToList();

        var companyProductMap = companyProducts.ToDictionary(cp => cp.CompanyProductId, cp => cp);

        var unidadMedidaIds = ventaDetalles
            .Where(d => d.UnidadMedidaId.HasValue)
            .Select(d => d.UnidadMedidaId!.Value)
            .Distinct()
            .ToList();

        var unidades = unidadMedidaIds.Count == 0
            ? new List<Uom>()
            : (await _uomRepo.FindAsync(u => u.EmpresaId == empresaId && unidadMedidaIds.Contains(u.UomId), ct)).ToList();

        var unidadMap = unidades.ToDictionary(u => u.UomId, u => u);

        foreach (var vd in ventaDetalles)
        {
            companyProductMap.TryGetValue(vd.CompanyProductId ?? 0, out var cp);
            unidadMap.TryGetValue(vd.UnidadMedidaId ?? 0, out var um);

            var fd = new FacturaVentaDetalle
            {
                EmpresaId = empresaId,
                FacturaVentaId = factura.Id,
                VentaDetalleId = vd.Id,
                TipoItemVenta = vd.TipoItemVenta,
                CodigoProducto = cp?.Sku ?? cp?.CodigoInterno,
                Descripcion = vd.Descripcion,
                DetalleAdicional = vd.DetalleAdicional,
                Cantidad = vd.Cantidad,
                UnidadMedida = um?.Code,
                PrecioUnitario = vd.PrecioUnitario,
                DescuentoMonto = vd.DescuentoMonto,
                ImpuestoMonto = vd.ImpuestoMonto,
                TotalLinea = vd.TotalLinea,
                Activo = true
            };

            await _facturaDetalleRepo.AddAsync(fd, ct);
        }

        venta.FacturaGenerada = true;
        await _ventaRepo.UpdateAsync(venta, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        var detallesFactura = await _facturaDetalleRepo.FindAsync(d => d.EmpresaId == empresaId && d.FacturaVentaId == factura.Id, ct);
        return Result<FacturaVentaDto>.Success(MapToDto(factura, detallesFactura));
    }

    public async Task<Result<FacturaVentaDto>> AnularAsync(long id, AnularFacturaVentaRequestDto dto, CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<FacturaVentaDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var factura = await _facturaRepo.GetByIdAsync(id, ct);
        if (factura is null)
            return Result<FacturaVentaDto>.Failure("Factura de venta no encontrada.");

        if (factura.EmpresaId != empresaId)
            return Result<FacturaVentaDto>.Failure("La factura no pertenece a la empresa activa.");

        if (factura.EstadoFactura == EstadoFacturaVentaComercial.Anulada)
            return Result<FacturaVentaDto>.Failure("La factura ya está anulada.");

        if (string.IsNullOrWhiteSpace(dto.MotivoAnulacion))
            return Result<FacturaVentaDto>.Failure("El motivo de anulación es obligatorio.");

        factura.EstadoFactura = EstadoFacturaVentaComercial.Anulada;
        factura.FechaAnulacion = DateTime.Now;
        factura.MotivoAnulacion = dto.MotivoAnulacion.Trim();

        await _facturaRepo.UpdateAsync(factura, ct);

        var venta = await _ventaRepo.GetByIdAsync(factura.VentaId, ct);
        if (venta is not null && venta.EmpresaId == empresaId)
        {
            venta.FacturaGenerada = false;
            await _ventaRepo.UpdateAsync(venta, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var detalles = await _facturaDetalleRepo.FindAsync(d => d.EmpresaId == empresaId && d.FacturaVentaId == factura.Id, ct);
        return Result<FacturaVentaDto>.Success(MapToDto(factura, detalles));
    }

    private async Task<Result<FiscalData>> ResolveFiscalDataAsync(
        int empresaId,
        Venta venta,
        GenerarFacturaVentaRequestDto dto,
        CancellationToken ct)
    {
        var fromRequestNit = Normalize(dto.NitFactura);
        var fromRequestComplemento = Normalize(dto.Complemento);
        var fromRequestRazon = Normalize(dto.RazonSocialFactura);
        var fromRequestEmail = Normalize(dto.EmailFactura);
        var fromRequestTelefono = Normalize(dto.TelefonoFactura);

        var ventaFact = (await _ventaFacturacionRepo.FindAsync(
            f => f.EmpresaId == empresaId && f.VentaId == venta.Id && f.Activo, ct))
            .FirstOrDefault();

        Cliente? cliente = null;
        if (venta.ClienteId.HasValue)
            cliente = await _clienteRepo.GetByIdAsync(venta.ClienteId.Value, ct);

        ClientePerfilFiscal? perfilSeleccionado = null;
        if (venta.ClienteId.HasValue && ventaFact?.ClientePerfilFiscalId is long perfilSeleccionadoId)
        {
            var perfil = await _clientePerfilFiscalRepo.GetByIdAsync(perfilSeleccionadoId, ct);
            if (perfil is not null
                && perfil.EmpresaId == empresaId
                && perfil.ClienteId == venta.ClienteId.Value
                && perfil.Activo)
            {
                perfilSeleccionado = perfil;
            }
        }

        ClientePerfilFiscal? perfilPredeterminado = null;
        if (venta.ClienteId.HasValue)
        {
            perfilPredeterminado = (await _clientePerfilFiscalRepo.FindAsync(
                p => p.EmpresaId == empresaId
                     && p.ClienteId == venta.ClienteId.Value
                     && p.Activo
                     && p.EsPredeterminado,
                ct))
                .OrderBy(p => p.Id)
                .FirstOrDefault();
        }

        var nit = fromRequestNit
                  ?? Normalize(ventaFact?.NitFactura)
                  ?? Normalize(perfilSeleccionado?.NumeroDocumento)
                  ?? Normalize(perfilPredeterminado?.NumeroDocumento)
                  ?? Normalize(cliente?.NIT);

        var complemento = fromRequestComplemento
                          ?? Normalize(ventaFact?.Complemento)
                          ?? Normalize(perfilSeleccionado?.Complemento)
                          ?? Normalize(perfilPredeterminado?.Complemento);

        var razon = fromRequestRazon
                    ?? Normalize(ventaFact?.RazonSocialFactura)
                    ?? Normalize(perfilSeleccionado?.RazonSocial)
                    ?? Normalize(perfilPredeterminado?.RazonSocial)
                    ?? Normalize(cliente?.RazonSocial);

        var email = fromRequestEmail
                    ?? Normalize(ventaFact?.EmailFactura)
                    ?? Normalize(perfilSeleccionado?.EmailFactura)
                    ?? Normalize(perfilPredeterminado?.EmailFactura)
                    ?? Normalize(cliente?.Email);

        var telefono = fromRequestTelefono
                       ?? Normalize(ventaFact?.TelefonoFactura)
                       ?? Normalize(perfilSeleccionado?.TelefonoFactura)
                       ?? Normalize(perfilPredeterminado?.TelefonoFactura)
                       ?? Normalize(cliente?.Telefono);

        return Result<FiscalData>.Success(new FiscalData
        {
            ClientePerfilFiscalId = perfilSeleccionado?.Id ?? perfilPredeterminado?.Id,
            NitFactura = nit,
            Complemento = complemento,
            RazonSocialFactura = razon,
            EmailFactura = email,
            TelefonoFactura = telefono
        });
    }

    private async Task<string> GenerateNumeroFacturaAsync(int empresaId, CancellationToken ct)
    {
        // TODO: Integrar NumeracionDocumento para FACTURA en próxima fase.
        var hoy = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefijo = $"FAC-{hoy}-";

        var facturasHoy = await _facturaRepo.FindAsync(
            f => f.EmpresaId == empresaId && f.NumeroFactura.StartsWith(prefijo), ct);

        var maxSecuencia = 0;
        foreach (var f in facturasHoy)
        {
            var numero = f.NumeroFactura;
            if (numero.Length <= prefijo.Length) continue;

            var sufijo = numero[prefijo.Length..];
            if (int.TryParse(sufijo, out var seq) && seq > maxSecuencia)
                maxSecuencia = seq;
        }

        var siguiente = maxSecuencia + 1;
        return $"{prefijo}{siguiente:D4}";
    }

    private static bool TryParseEnum<TEnum>(string? value, out TEnum parsed) where TEnum : struct, Enum
    {
        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse(value, true, out parsed))
            return true;

        parsed = default;
        return false;
    }

    private static string? Normalize(string? value)
    {
        var t = value?.Trim();
        return string.IsNullOrWhiteSpace(t) ? null : t;
    }

    private static FacturaVentaResumenDto MapToResumenDto(FacturaVenta f)
        => new()
        {
            Id = f.Id,
            VentaId = f.VentaId,
            ClientePerfilFiscalId = f.ClientePerfilFiscalId,
            NumeroFactura = f.NumeroFactura,
            FechaEmision = f.FechaEmision,
            EstadoFactura = f.EstadoFactura.ToString(),
            EstadoSiat = f.EstadoSiat.ToString(),
            NitFactura = f.NitFactura,
            RazonSocialFactura = f.RazonSocialFactura,
            MonedaCodigo = f.MonedaCodigo,
            Total = f.Total,
            Activo = f.Activo
        };

    private static FacturaVentaDto MapToDto(FacturaVenta f, IReadOnlyList<FacturaVentaDetalle> detalles)
        => new()
        {
            Id = f.Id,
            VentaId = f.VentaId,
            ClientePerfilFiscalId = f.ClientePerfilFiscalId,
            NumeroFactura = f.NumeroFactura,
            NumeroAutorizacion = f.NumeroAutorizacion,
            FechaEmision = f.FechaEmision,
            TipoDocumentoFactura = f.TipoDocumentoFactura.ToString(),
            EstadoFactura = f.EstadoFactura.ToString(),
            EstadoSiat = f.EstadoSiat.ToString(),
            NitFactura = f.NitFactura,
            Complemento = f.Complemento,
            RazonSocialFactura = f.RazonSocialFactura,
            EmailFactura = f.EmailFactura,
            TelefonoFactura = f.TelefonoFactura,
            MonedaCodigo = f.MonedaCodigo,
            TipoCambio = f.TipoCambio,
            Subtotal = f.Subtotal,
            DescuentoTotal = f.DescuentoTotal,
            ImpuestoTotal = f.ImpuestoTotal,
            Total = f.Total,
            Observaciones = f.Observaciones,
            MotivoAnulacion = f.MotivoAnulacion,
            FechaAnulacion = f.FechaAnulacion,
            Cuf = f.Cuf,
            Cufd = f.Cufd,
            Cuis = f.Cuis,
            CodigoControl = f.CodigoControl,
            CodigoRecepcion = f.CodigoRecepcion,
            CodigoExcepcion = f.CodigoExcepcion,
            Leyenda = f.Leyenda,
            Activo = f.Activo,
            Detalles = detalles
                .OrderBy(d => d.Id)
                .Select(d => new FacturaVentaDetalleDto
                {
                    Id = d.Id,
                    FacturaVentaId = d.FacturaVentaId,
                    VentaDetalleId = d.VentaDetalleId,
                    TipoItemVenta = d.TipoItemVenta.ToString(),
                    CodigoProducto = d.CodigoProducto,
                    Descripcion = d.Descripcion,
                    DetalleAdicional = d.DetalleAdicional,
                    Cantidad = d.Cantidad,
                    UnidadMedida = d.UnidadMedida,
                    PrecioUnitario = d.PrecioUnitario,
                    DescuentoMonto = d.DescuentoMonto,
                    ImpuestoMonto = d.ImpuestoMonto,
                    TotalLinea = d.TotalLinea
                })
                .ToList()
        };

    private sealed class FiscalData
    {
        public long? ClientePerfilFiscalId { get; set; }
        public string? NitFactura { get; set; }
        public string? Complemento { get; set; }
        public string? RazonSocialFactura { get; set; }
        public string? EmailFactura { get; set; }
        public string? TelefonoFactura { get; set; }
    }
}
