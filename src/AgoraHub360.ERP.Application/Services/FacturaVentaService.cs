namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Entities.CXC;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;

public class FacturaVentaService : IFacturaVentaService
{
    private readonly IRepository<FacturaVenta> _facturaRepo;
    private readonly IRepository<FacturaVentaDetalle> _facturaDetalleRepo;
    private readonly IRepository<Venta> _ventaRepo;
    private readonly IRepository<VentaDetalle> _ventaDetalleRepo;
    private readonly IRepository<VentaFacturacionDatos> _ventaFacturacionRepo;
    private readonly IRepository<VentaPago> _ventaPagoRepo;
    private readonly IRepository<SiatMetodoPago> _siatMetodoPagoRepo;
    private readonly IRepository<Cliente> _clienteRepo;
    private readonly IRepository<ClientePerfilFiscal> _clientePerfilFiscalRepo;
    private readonly IRepository<CompanyProduct> _companyProductRepo;
    private readonly IRepository<Uom> _uomRepo;
        private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICuentasPorCobrarService _cuentasPorCobrarService;
    private readonly IRepository<CuentaPorCobrar> _cxcRepo;

    public FacturaVentaService(
        IRepository<FacturaVenta> facturaRepo,
        IRepository<FacturaVentaDetalle> facturaDetalleRepo,
        IRepository<Venta> ventaRepo,
        IRepository<VentaDetalle> ventaDetalleRepo,
        IRepository<VentaFacturacionDatos> ventaFacturacionRepo,
        IRepository<VentaPago> ventaPagoRepo,
        IRepository<SiatMetodoPago> siatMetodoPagoRepo,
        IRepository<Cliente> clienteRepo,
        IRepository<ClientePerfilFiscal> clientePerfilFiscalRepo,
        IRepository<CompanyProduct> companyProductRepo,
        IRepository<Uom> uomRepo,
                ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        ICuentasPorCobrarService cuentasPorCobrarService,
        IRepository<CuentaPorCobrar> cxcRepo)
    {
        _facturaRepo = facturaRepo;
        _facturaDetalleRepo = facturaDetalleRepo;
        _ventaRepo = ventaRepo;
        _ventaDetalleRepo = ventaDetalleRepo;
        _ventaFacturacionRepo = ventaFacturacionRepo;
        _ventaPagoRepo = ventaPagoRepo;
        _siatMetodoPagoRepo = siatMetodoPagoRepo;
        _clienteRepo = clienteRepo;
        _clientePerfilFiscalRepo = clientePerfilFiscalRepo;
        _companyProductRepo = companyProductRepo;
        _uomRepo = uomRepo;
                _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _cuentasPorCobrarService = cuentasPorCobrarService;
        _cxcRepo = cxcRepo;
    }

    public async Task<Result<PaginatedResultDto<FacturaVentaResumenDto>>> GetAllAsync(
        FacturaVentaFilterDto? filter = null,
        CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<PaginatedResultDto<FacturaVentaResumenDto>>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var facturas = await _facturaRepo.FindAsync(f => f.EmpresaId == empresaId, ct);

        var filteredList = facturas.AsEnumerable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.NumeroFactura))
            {
                filteredList = filteredList.Where(f => f.NumeroFactura.Contains(filter.NumeroFactura, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Busqueda))
            {
                var term = filter.Busqueda;
                var ventasConNumeroVenta = await _ventaRepo.FindAsync(v => v.EmpresaId == empresaId && v.NumeroVenta.Contains(term), ct);
                var ventaIdsConNumeroVenta = ventasConNumeroVenta.Select(v => v.Id).ToHashSet();

                filteredList = filteredList.Where(f => 
                    f.NumeroFactura.Contains(term, StringComparison.OrdinalIgnoreCase) || 
                    f.NitFactura.Contains(term, StringComparison.OrdinalIgnoreCase) || 
                    f.RazonSocialFactura.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    ventaIdsConNumeroVenta.Contains(f.VentaId));
            }

            if (!string.IsNullOrWhiteSpace(filter.EstadoFactura) && !string.Equals(filter.EstadoFactura, "Todos", StringComparison.OrdinalIgnoreCase))
            {
                filteredList = filteredList.Where(f => string.Equals(f.EstadoFactura.ToString(), filter.EstadoFactura, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.ClienteId.HasValue)
            {
                var ventas = await _ventaRepo.FindAsync(v => v.EmpresaId == empresaId && v.ClienteId == filter.ClienteId.Value, ct);
                var ventaIds = ventas.Select(v => v.Id).ToHashSet();
                filteredList = filteredList.Where(f => ventaIds.Contains(f.VentaId));
            }

            if (!string.IsNullOrWhiteSpace(filter.Moneda) && !string.Equals(filter.Moneda, "Todos", StringComparison.OrdinalIgnoreCase))
            {
                filteredList = filteredList.Where(f => string.Equals(f.MonedaCodigo, filter.Moneda, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.FechaEmisionDesde.HasValue)
            {
                filteredList = filteredList.Where(f => f.FechaEmision.Date >= filter.FechaEmisionDesde.Value.Date);
            }

            if (filter.FechaEmisionHasta.HasValue)
            {
                filteredList = filteredList.Where(f => f.FechaEmision.Date <= filter.FechaEmisionHasta.Value.Date);
            }
        }

        var sorted = filteredList
            .OrderByDescending(f => f.FechaEmision)
            .ThenByDescending(f => f.Id)
            .Select(MapToResumenDto);

        var top = filter?.Top ?? 100;
        if (top > 0)
        {
            sorted = sorted.Take(top);
        }

        var sortedList = sorted.ToList();
        var totalItems = sortedList.Count;

        var pagina = filter?.Page ?? filter?.Pagina ?? 1;
        var tamanoPagina = filter?.PageSize ?? filter?.TamanoPagina ?? 50;

        var items = sortedList
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();

        if (items.Count > 0)
        {
            var facturaIds = items.Select(f => f.Id).ToList();
            var cxcs = await _cxcRepo.FindAsync(c => c.EmpresaId == empresaId && facturaIds.Contains(c.FacturaVentaId), ct);
            var cxcMap = cxcs.ToDictionary(c => c.FacturaVentaId, c => c);

            foreach (var item in items)
            {
                if (cxcMap.TryGetValue(item.Id, out var cxc))
                {
                    item.TotalPagado = cxc.TotalPagado;
                    item.SaldoPendiente = cxc.SaldoPendiente;
                    item.FechaVencimiento = cxc.FechaVencimiento;
                    item.EstadoCobro = cxc.Estado.ToString();
                    item.DiasVencidos = CalculateDiasVencidos(cxc.FechaVencimiento);
                }
                else
                {
                    item.TotalPagado = 0;
                    item.SaldoPendiente = item.Total;
                    item.FechaVencimiento = null;
                    item.EstadoCobro = item.EstadoFactura == "Anulada" || item.EstadoFactura == "Anulado" ? "Anulada" : "Pendiente";
                    item.DiasVencidos = 0;
                }
            }
        }

        var paginatedResult = new PaginatedResultDto<FacturaVentaResumenDto>
        {
            Items = items,
            TotalItems = totalItems,
            Pagina = pagina,
            TamanoPagina = tamanoPagina
        };

        return Result<PaginatedResultDto<FacturaVentaResumenDto>>.Success(paginatedResult);
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

        var mappedDto = MapToDto(factura, detalles);
        await PopulateCxCFieldsAsync(mappedDto, empresaId, ct);
        return Result<FacturaVentaDto>.Success(mappedDto);
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

        var mappedDto = MapToDto(factura, detalles);
        await PopulateCxCFieldsAsync(mappedDto, empresaId, ct);
        return Result<FacturaVentaDto>.Success(mappedDto);
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
        var paymentMethodCode = await ResolvePaymentMethodCode(venta, ct);

        var factura = new FacturaVenta
        {
            EmpresaId = empresaId,
            VentaId = venta.Id,
            NumeroFactura = numeroFactura,
            BillUuid = null,
            FechaEmision = DateTime.Now,
            ActivityCode = null,
            TipoDocumentoFactura = tipoDocumentoFactura,
            IdentityDocTypeCode = fiscal.TipoDocumentoIdentidad,
            EstadoFactura = EstadoFacturaVentaComercial.Generada,
            EstadoSiat = EstadoSiatFactura.NoEnviada,
            ClientePerfilFiscalId = fiscal.ClientePerfilFiscalId,
            NitFactura = fiscal.NitFactura!,
            Complemento = fiscal.Complemento,
            RazonSocialFactura = fiscal.RazonSocialFactura!,
            BeneficiaryName = fiscal.RazonSocialFactura,
            EmailFactura = fiscal.EmailFactura,
            TelefonoFactura = fiscal.TelefonoFactura,
            PaymentMethodCode = paymentMethodCode,
            CardNumber = null,
            GiftCardAmount = null,
            AdditionalDiscount = venta.DescuentoTotal > 0 ? venta.DescuentoTotal : null,
            PieLey = null,
            EnlaceXml = null,
            EnlacePdf = null,
            SiatQr = null,
            Origen = null,
            DescripcionFC = null,
            IdDosificacion = null,
            CodDePago = null,
            IdTipo = null,
            Revertido = false,
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
            var codigoProducto = cp?.CodigoInterno ?? cp?.Sku;
            var itemCode = cp?.Sku ?? cp?.CodigoInterno ?? codigoProducto;

            var fd = new FacturaVentaDetalle
            {
                EmpresaId = empresaId,
                FacturaVentaId = factura.Id,
                VentaDetalleId = vd.Id,
                TipoItemVenta = vd.TipoItemVenta,
                CodigoProducto = codigoProducto,
                ItemCode = itemCode,
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

        // ── Generar Cuenta por Cobrar automáticamente ──────────────────────────
        var cxcResult = await _cuentasPorCobrarService.GenerarDesdeFacturaAsync(factura.Id, ct);
        if (!cxcResult.IsSuccess)
        {
            // No falla la factura si la CxC tiene problemas; solo se registra.
            // En futura fase se podría loguear.
        }

        var detallesFactura = await _facturaDetalleRepo.FindAsync(d => d.EmpresaId == empresaId && d.FacturaVentaId == factura.Id, ct);
        var mappedDto = MapToDto(factura, detallesFactura);
        await PopulateCxCFieldsAsync(mappedDto, empresaId, ct);
        return Result<FacturaVentaDto>.Success(mappedDto);
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
        var mappedDto = MapToDto(factura, detalles);
        await PopulateCxCFieldsAsync(mappedDto, empresaId, ct);
        return Result<FacturaVentaDto>.Success(mappedDto);
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

        var tipoDocumentoIdentidad = Normalize(ventaFact?.TipoDocumentoIdentidad)
                                     ?? Normalize(perfilSeleccionado?.TipoDocumentoIdentidad)
                                     ?? Normalize(perfilPredeterminado?.TipoDocumentoIdentidad);

        return Result<FiscalData>.Success(new FiscalData
        {
            ClientePerfilFiscalId = perfilSeleccionado?.Id ?? perfilPredeterminado?.Id,
            TipoDocumentoIdentidad = tipoDocumentoIdentidad,
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

    private async Task<string> ResolvePaymentMethodCode(Venta venta, CancellationToken ct)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return "1";

        var empresaId = _currentUser.EmpresaId.Value;
        var metodosActivos = await _siatMetodoPagoRepo.FindAsync(
            x => x.EmpresaId == empresaId && x.Activo,
            ct);

        var pagoSeleccionado = (await _ventaPagoRepo.FindAsync(
                p => p.EmpresaId == empresaId
                     && p.VentaId == venta.Id
                     && p.Activo
                     && p.EstadoPago != EstadoPagoVenta.Anulado,
                ct))
            .OrderByDescending(p => p.Monto)
            .ThenByDescending(p => p.Id)
            .FirstOrDefault();

        SiatMetodoPago? metodoSeleccionado = null;

        if (pagoSeleccionado is not null)
        {
            var modoPagoNombre = pagoSeleccionado.ModoPago.ToString();
            metodoSeleccionado = metodosActivos.FirstOrDefault(x =>
                string.Equals(x.ModoPago, modoPagoNombre, StringComparison.OrdinalIgnoreCase));

            if (metodoSeleccionado is null && pagoSeleccionado.ModoPago == ModoPago.Deposito)
            {
                metodoSeleccionado = metodosActivos.FirstOrDefault(x =>
                    string.Equals(x.ModoPago, ModoPago.Transferencia.ToString(), StringComparison.OrdinalIgnoreCase));
            }
        }

        metodoSeleccionado ??= metodosActivos.FirstOrDefault(x => x.EsPredeterminado);

        return string.IsNullOrWhiteSpace(metodoSeleccionado?.Codigo)
            ? "1"
            : metodoSeleccionado.Codigo;
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

    private static int CalculateDiasVencidos(DateTime? fechaVencimiento)
    {
        if (!fechaVencimiento.HasValue) return 0;
        var today = DateTime.Today;
        if (fechaVencimiento.Value.Date >= today) return 0;
        return (today - fechaVencimiento.Value.Date).Days;
    }

    private async Task PopulateCxCFieldsAsync(FacturaVentaDto dto, int empresaId, CancellationToken ct)
    {
        var cxcList = await _cxcRepo.FindAsync(c => c.EmpresaId == empresaId && c.FacturaVentaId == dto.Id, ct);
        var cxc = cxcList.FirstOrDefault();

        if (cxc != null)
        {
            dto.TotalPagado = cxc.TotalPagado;
            dto.SaldoPendiente = cxc.SaldoPendiente;
            dto.FechaVencimiento = cxc.FechaVencimiento;
            dto.EstadoCobro = cxc.Estado.ToString();
            dto.DiasVencidos = CalculateDiasVencidos(cxc.FechaVencimiento);
        }
        else
        {
            dto.TotalPagado = 0;
            dto.SaldoPendiente = dto.Total;
            dto.FechaVencimiento = null;
            dto.EstadoCobro = dto.EstadoFactura == "Anulada" || dto.EstadoFactura == "Anulado" ? "Anulada" : "Pendiente";
            dto.DiasVencidos = 0;
        }
    }

    private static FacturaVentaResumenDto MapToResumenDto(FacturaVenta f)
        => new()
        {
            Id = f.Id,
            VentaId = f.VentaId,
            ClientePerfilFiscalId = f.ClientePerfilFiscalId,
            NumeroFactura = f.NumeroFactura,
            BillUuid = f.BillUuid,
            FechaEmision = f.FechaEmision,
            ActivityCode = f.ActivityCode,
            IdentityDocTypeCode = f.IdentityDocTypeCode,
            EstadoFactura = f.EstadoFactura.ToString(),
            EstadoSiat = f.EstadoSiat.ToString(),
            NitFactura = f.NitFactura,
            RazonSocialFactura = f.RazonSocialFactura,
            BeneficiaryName = f.BeneficiaryName,
            MonedaCodigo = f.MonedaCodigo,
            Total = f.Total,
            AdditionalDiscount = f.AdditionalDiscount,
            Revertido = f.Revertido,
            Cuf = f.Cuf,
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
            BillUuid = f.BillUuid,
            FechaEmision = f.FechaEmision,
            ActivityCode = f.ActivityCode,
            TipoDocumentoFactura = f.TipoDocumentoFactura.ToString(),
            IdentityDocTypeCode = f.IdentityDocTypeCode,
            EstadoFactura = f.EstadoFactura.ToString(),
            EstadoSiat = f.EstadoSiat.ToString(),
            NitFactura = f.NitFactura,
            Complemento = f.Complemento,
            RazonSocialFactura = f.RazonSocialFactura,
            BeneficiaryName = f.BeneficiaryName,
            EmailFactura = f.EmailFactura,
            TelefonoFactura = f.TelefonoFactura,
            PaymentMethodCode = f.PaymentMethodCode,
            CardNumber = f.CardNumber,
            GiftCardAmount = f.GiftCardAmount,
            AdditionalDiscount = f.AdditionalDiscount,
            PieLey = f.PieLey,
            EnlaceXml = f.EnlaceXml,
            EnlacePdf = f.EnlacePdf,
            SiatQr = f.SiatQr,
            Origen = f.Origen,
            DescripcionFC = f.DescripcionFC,
            IdDosificacion = f.IdDosificacion,
            CodDePago = f.CodDePago,
            IdTipo = f.IdTipo,
            Revertido = f.Revertido,
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
                    ItemCode = d.ItemCode,
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
        public string? TipoDocumentoIdentidad { get; set; }
        public string? NitFactura { get; set; }
        public string? Complemento { get; set; }
        public string? RazonSocialFactura { get; set; }
        public string? EmailFactura { get; set; }
        public string? TelefonoFactura { get; set; }
    }
}
