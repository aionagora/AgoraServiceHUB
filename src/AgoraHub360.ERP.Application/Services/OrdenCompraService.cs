namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.CMP;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Compras;

public class OrdenCompraService : IOrdenCompraService
{
    private readonly IRepository<OrdenCompra> _ocRepo;
    private readonly IRepository<OrdenCompraLinea> _lineaRepo;
    private readonly IRepository<Proveedor> _proveedorRepo;
    private readonly IRepository<Almacen> _almacenRepo;
    private readonly IRepository<CompanyProduct> _companyProductRepo;
    private readonly IRepository<NumeracionDocumento> _numeracionRepo;
    private readonly IRepository<ConfirmacionProveedor> _confProvRepo;
    private readonly IRepository<PagoOrdenCompra> _pagoRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public OrdenCompraService(
        IRepository<OrdenCompra> ocRepo,
        IRepository<OrdenCompraLinea> lineaRepo,
        IRepository<Proveedor> proveedorRepo,
        IRepository<Almacen> almacenRepo,
        IRepository<CompanyProduct> companyProductRepo,
        IRepository<NumeracionDocumento> numeracionRepo,
        IRepository<ConfirmacionProveedor> confProvRepo,
        IRepository<PagoOrdenCompra> pagoRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _ocRepo = ocRepo;
        _lineaRepo = lineaRepo;
        _proveedorRepo = proveedorRepo;
        _almacenRepo = almacenRepo;
        _companyProductRepo = companyProductRepo;
        _numeracionRepo = numeracionRepo;
        _confProvRepo = confProvRepo;
        _pagoRepo = pagoRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<OrdenCompraDto>>> GetAllAsync(
        int? proveedorId, string? estado, DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<OrdenCompraDto>>.Failure("No active company.");

        var ordenes = await _ocRepo.FindAsync(
            o => o.EmpresaId == empresaId.Value && o.Activo
                && (!proveedorId.HasValue || o.ProveedorId == proveedorId.Value)
                && (!fechaDesde.HasValue || o.FechaEmision >= fechaDesde.Value)
                && (!fechaHasta.HasValue || o.FechaEmision <= fechaHasta.Value.AddDays(1).AddSeconds(-1)),
            ct);

        if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoDocumento>(estado, true, out var estadoEnum))
            ordenes = ordenes.Where(o => o.Estado == estadoEnum).ToList();

        // Load lookups
        var proveedores = await _proveedorRepo.FindAsync(p => p.EmpresaId == empresaId.Value, ct);
        var almacenes = await _almacenRepo.FindAsync(a => a.EmpresaId == empresaId.Value, ct);
        var lineas = await _lineaRepo.FindAsync(l => ordenes.Select(o => o.OrdenCompraId).Contains(l.OrdenCompraId), ct);
        var cpIds = lineas.Select(l => l.CompanyProductId).Distinct().ToList();
        var cps = await _companyProductRepo.FindAsync(p => cpIds.Contains(p.CompanyProductId), ct);

        var provMap = proveedores.ToDictionary(p => p.Id, p => p.RazonSocial);
        var almMap = almacenes.ToDictionary(a => a.Id, a => a.Nombre);
        var cpMap = cps.ToDictionary(p => p.CompanyProductId, p => p.Sku);

        var dtos = ordenes
            .OrderByDescending(o => o.FechaEmision)
            .ThenByDescending(o => o.OrdenCompraId)
            .Select(o => MapToDto(o,
                lineas.Where(l => l.OrdenCompraId == o.OrdenCompraId).ToList(),
                provMap, almMap, cpMap))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<OrdenCompraDto>>.Success(dtos);
    }

    public async Task<Result<OrdenCompraDto>> GetByIdAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<OrdenCompraDto>.Failure("No active company.");

        var oc = await _ocRepo.GetByIdAsync(id, ct);
        if (oc is null || oc.EmpresaId != empresaId.Value || !oc.Activo)
            return Result<OrdenCompraDto>.Failure($"Purchase order {id} not found.");

        return Result<OrdenCompraDto>.Success(await BuildFullDto(oc, ct));
    }

    public async Task<Result<OrdenCompraDto>> CreateAsync(CreateOrdenCompraDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<OrdenCompraDto>.Failure("No active company.");

        // Validate at least one line
        if (dto.Lineas is null || dto.Lineas.Count == 0)
            return Result<OrdenCompraDto>.Failure("An order must have at least one line.");

        // Validate proveedor
        var proveedor = await _proveedorRepo.GetByIdAsync(dto.ProveedorId, ct);
        if (proveedor is null || proveedor.EmpresaId != empresaId.Value)
            return Result<OrdenCompraDto>.Failure("Supplier not found or does not belong to your company.");

        // Validate almacen
        var almacen = await _almacenRepo.GetByIdAsync(dto.AlmacenDestinoId, ct);
        if (almacen is null || almacen.EmpresaId != empresaId.Value)
            return Result<OrdenCompraDto>.Failure("Warehouse not found or does not belong to your company.");

        // Validate all products before creating
        foreach (var lineaDto in dto.Lineas)
        {
            var cp = await _companyProductRepo.GetByIdAsync(lineaDto.CompanyProductId, ct);
            if (cp is null || cp.EmpresaId != empresaId.Value)
                return Result<OrdenCompraDto>.Failure($"Product {lineaDto.CompanyProductId} not found.");
        }

        // Generate number from NumeracionDocumento
        var numero = await GenerarNumeroAsync(empresaId.Value, ct);

        var oc = new OrdenCompra
        {
            EmpresaId = empresaId.Value,
            Numero = numero,
            FechaEmision = dto.FechaEmision,
            FechaEntregaEstimada = dto.FechaEntregaEstimada,
            ProveedorId = dto.ProveedorId,
            AlmacenDestinoId = dto.AlmacenDestinoId,
            MonedaId = dto.MonedaId,
            TasaCambio = dto.TasaCambio,
            Estado = EstadoDocumento.Borrador,
            CondicionPago = dto.CondicionPago,
            Incoterm = dto.Incoterm,
            Observaciones = dto.Observaciones,
            ReferenciaExterna = dto.ReferenciaExterna,
            OrdenPedidoId = dto.OrdenPedidoId,
            Activo = true
        };

        await _ocRepo.AddAsync(oc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Add lines
        int lineNum = 1;
        foreach (var lineaDto in dto.Lineas)
        {
            var linea = new OrdenCompraLinea
            {
                OrdenCompraId = oc.OrdenCompraId,
                NumeroLinea = lineNum++,
                CompanyProductId = lineaDto.CompanyProductId,
                Descripcion = lineaDto.Descripcion,
                UnidadMedida = lineaDto.UnidadMedida,
                Cantidad = lineaDto.Cantidad,
                PrecioUnitario = lineaDto.PrecioUnitario,
                PorcentajeDescuento = lineaDto.PorcentajeDescuento,
                PorcentajeImpuesto = lineaDto.PorcentajeImpuesto,
                Activo = true
            };
            linea.Recalcular();
            oc.Lineas.Add(linea);
            await _lineaRepo.AddAsync(linea, ct);
        }

        // Recalculate totals using in-memory collection
        oc.RecalcularTotales();
        await _ocRepo.UpdateAsync(oc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<OrdenCompraDto>.Success(await BuildFullDto(oc, ct));
    }

    public async Task<Result<OrdenCompraDto>> UpdateAsync(long id, UpdateOrdenCompraDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<OrdenCompraDto>.Failure("No active company.");

        var oc = await _ocRepo.GetByIdAsync(id, ct);
        if (oc is null || oc.EmpresaId != empresaId.Value || !oc.Activo)
            return Result<OrdenCompraDto>.Failure("Purchase order not found.");
        if (oc.Estado != EstadoDocumento.Borrador)
            return Result<OrdenCompraDto>.Failure("Only draft orders can be edited.");

        // Validate proveedor
        var proveedor = await _proveedorRepo.GetByIdAsync(dto.ProveedorId, ct);
        if (proveedor is null || proveedor.EmpresaId != empresaId.Value)
            return Result<OrdenCompraDto>.Failure("Supplier not found.");

        // Validate almacen
        var almacen = await _almacenRepo.GetByIdAsync(dto.AlmacenDestinoId, ct);
        if (almacen is null || almacen.EmpresaId != empresaId.Value)
            return Result<OrdenCompraDto>.Failure("Warehouse not found.");

        oc.FechaEmision = dto.FechaEmision;
        oc.FechaEntregaEstimada = dto.FechaEntregaEstimada;
        oc.ProveedorId = dto.ProveedorId;
        oc.AlmacenDestinoId = dto.AlmacenDestinoId;
        oc.MonedaId = dto.MonedaId;
        oc.TasaCambio = dto.TasaCambio;
        oc.CondicionPago = dto.CondicionPago;
        oc.Incoterm = dto.Incoterm;
        oc.Observaciones = dto.Observaciones;
        oc.ReferenciaExterna = dto.ReferenciaExterna;

        await _ocRepo.UpdateAsync(oc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<OrdenCompraDto>.Success(await BuildFullDto(oc, ct));
    }

    public async Task<Result<OrdenCompraDto>> AddLineaAsync(long ordenId, AddOrdenCompraLineaDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<OrdenCompraDto>.Failure("No active company.");

        var oc = await _ocRepo.GetByIdAsync(ordenId, ct);
        if (oc is null || oc.EmpresaId != empresaId.Value || !oc.Activo)
            return Result<OrdenCompraDto>.Failure("Purchase order not found.");
        if (oc.Estado != EstadoDocumento.Borrador)
            return Result<OrdenCompraDto>.Failure("Lines can only be added to draft orders.");

        var cp = await _companyProductRepo.GetByIdAsync(dto.CompanyProductId, ct);
        if (cp is null || cp.EmpresaId != empresaId.Value)
            return Result<OrdenCompraDto>.Failure("Product not found.");

        var lineasExistentes = await _lineaRepo.FindAsync(l => l.OrdenCompraId == ordenId, ct);
        int maxLine = lineasExistentes.Count > 0 ? lineasExistentes.Max(l => l.NumeroLinea) : 0;

        var linea = new OrdenCompraLinea
        {
            OrdenCompraId = ordenId,
            NumeroLinea = maxLine + 1,
            CompanyProductId = dto.CompanyProductId,
            Descripcion = dto.Descripcion,
            UnidadMedida = dto.UnidadMedida,
            Cantidad = dto.Cantidad,
            PrecioUnitario = dto.PrecioUnitario,
            PorcentajeDescuento = dto.PorcentajeDescuento,
            PorcentajeImpuesto = dto.PorcentajeImpuesto,
            Activo = true
        };
        linea.Recalcular();

        await _lineaRepo.AddAsync(linea, ct);

        // Recalculate totals
        var todasLineas = await _lineaRepo.FindAsync(l => l.OrdenCompraId == ordenId, ct);
        oc.Lineas = todasLineas.ToList();
        oc.Lineas.Add(linea); // include the new one (might not be in the query yet)
        oc.RecalcularTotales();
        await _ocRepo.UpdateAsync(oc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<OrdenCompraDto>.Success(await BuildFullDto(oc, ct));
    }

    public async Task<Result<OrdenCompraDto>> UpdateLineaAsync(long ordenId, long lineaId, UpdateOrdenCompraLineaDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<OrdenCompraDto>.Failure("No active company.");

        var oc = await _ocRepo.GetByIdAsync(ordenId, ct);
        if (oc is null || oc.EmpresaId != empresaId.Value || !oc.Activo)
            return Result<OrdenCompraDto>.Failure("Purchase order not found.");
        if (oc.Estado != EstadoDocumento.Borrador)
            return Result<OrdenCompraDto>.Failure("Lines can only be edited on draft orders.");

        var linea = await _lineaRepo.GetByIdAsync(lineaId, ct);
        if (linea is null || linea.OrdenCompraId != ordenId)
            return Result<OrdenCompraDto>.Failure("Line not found.");

        linea.Descripcion = dto.Descripcion;
        linea.UnidadMedida = dto.UnidadMedida;
        linea.Cantidad = dto.Cantidad;
        linea.PrecioUnitario = dto.PrecioUnitario;
        linea.PorcentajeDescuento = dto.PorcentajeDescuento;
        linea.PorcentajeImpuesto = dto.PorcentajeImpuesto;
        linea.Recalcular();

        await _lineaRepo.UpdateAsync(linea, ct);

        var todasLineas = await _lineaRepo.FindAsync(l => l.OrdenCompraId == ordenId, ct);
        oc.Lineas = todasLineas.ToList();
        oc.RecalcularTotales();
        await _ocRepo.UpdateAsync(oc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<OrdenCompraDto>.Success(await BuildFullDto(oc, ct));
    }

    public async Task<Result<OrdenCompraDto>> RemoveLineaAsync(long ordenId, long lineaId, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<OrdenCompraDto>.Failure("No active company.");

        var oc = await _ocRepo.GetByIdAsync(ordenId, ct);
        if (oc is null || oc.EmpresaId != empresaId.Value || !oc.Activo)
            return Result<OrdenCompraDto>.Failure("Purchase order not found.");
        if (oc.Estado != EstadoDocumento.Borrador)
            return Result<OrdenCompraDto>.Failure("Lines can only be removed from draft orders.");

        var linea = await _lineaRepo.GetByIdAsync(lineaId, ct);
        if (linea is null || linea.OrdenCompraId != ordenId)
            return Result<OrdenCompraDto>.Failure("Line not found.");

        await _lineaRepo.DeleteAsync(linea, ct);

        var todasLineas = await _lineaRepo.FindAsync(l => l.OrdenCompraId == ordenId && l.OrdenCompraLineaId != lineaId, ct);
        oc.Lineas = todasLineas.ToList();
        oc.RecalcularTotales();
        await _ocRepo.UpdateAsync(oc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<OrdenCompraDto>.Success(await BuildFullDto(oc, ct));
    }

    public async Task<Result<OrdenCompraDto>> CambiarEstadoAsync(long id, string nuevoEstado, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<OrdenCompraDto>.Failure("No active company.");

        if (!Enum.TryParse<EstadoDocumento>(nuevoEstado, true, out var estadoTarget))
            return Result<OrdenCompraDto>.Failure($"Invalid status: {nuevoEstado}.");

        var oc = await _ocRepo.GetByIdAsync(id, ct);
        if (oc is null || oc.EmpresaId != empresaId.Value || !oc.Activo)
            return Result<OrdenCompraDto>.Failure("Purchase order not found.");

        // Full flowchart state machine
        var valid = (oc.Estado, estadoTarget) switch
        {
            // Borrador ? Confirmado (compras revisa y consolida)
            (EstadoDocumento.Borrador, EstadoDocumento.Confirmado) => true,
            // Confirmado ? PendienteAprobacion (requiere aprobación por monto/política)
            (EstadoDocumento.Confirmado, EstadoDocumento.PendienteAprobacion) => true,
            // Confirmado ? Aprobado (aprobación automática / sin gate)
            (EstadoDocumento.Confirmado, EstadoDocumento.Aprobado) => true,
            // PendienteAprobacion ? Aprobado (Finanzas/Dir aprueba)
            (EstadoDocumento.PendienteAprobacion, EstadoDocumento.Aprobado) => true,
            // PendienteAprobacion ? Rechazado (Finanzas/Dir rechaza)
            (EstadoDocumento.PendienteAprobacion, EstadoDocumento.Rechazado) => true,
            // Aprobado ? EnviadaProveedor (OC enviada al proveedor)
            (EstadoDocumento.Aprobado, EstadoDocumento.EnviadaProveedor) => true,
            // EnviadaProveedor ? EnNegociacion (proveedor negocia condiciones)
            (EstadoDocumento.EnviadaProveedor, EstadoDocumento.EnNegociacion) => true,
            // EnNegociacion ? Borrador (ajustar OC y reenviar)
            (EstadoDocumento.EnNegociacion, EstadoDocumento.Borrador) => true,
            // EnviadaProveedor ? ConfirmadaProveedor (PI aceptada)
            (EstadoDocumento.EnviadaProveedor, EstadoDocumento.ConfirmadaProveedor) => true,
            // EnNegociacion ? ConfirmadaProveedor (tras renegociación exitosa)
            (EstadoDocumento.EnNegociacion, EstadoDocumento.ConfirmadaProveedor) => true,
            // ConfirmadaProveedor ? PagoProgramado (anticipo/saldo programado)
            (EstadoDocumento.ConfirmadaProveedor, EstadoDocumento.PagoProgramado) => true,
            // PagoProgramado ? EnTransito (expediente abierto)
            (EstadoDocumento.PagoProgramado, EstadoDocumento.EnTransito) => true,
            // ConfirmadaProveedor ? EnTransito (sin pago explícito)
            (EstadoDocumento.ConfirmadaProveedor, EstadoDocumento.EnTransito) => true,
            // EnTransito ? RecepcionParcial / Cerrado vía RecepcionCompraService
            // Anulaciones permitidas hasta EnviadaProveedor
            (EstadoDocumento.Borrador, EstadoDocumento.Anulado) => true,
            (EstadoDocumento.Confirmado, EstadoDocumento.Anulado) => true,
            (EstadoDocumento.PendienteAprobacion, EstadoDocumento.Anulado) => true,
            (EstadoDocumento.Aprobado, EstadoDocumento.Anulado) => true,
            (EstadoDocumento.EnviadaProveedor, EstadoDocumento.Anulado) => true,
            _ => false
        };

        if (!valid)
            return Result<OrdenCompraDto>.Failure(
                $"Cannot transition from {oc.Estado} to {estadoTarget}.");

        // Guard: need lines before confirming/approving
        if (estadoTarget is EstadoDocumento.Confirmado or EstadoDocumento.PendienteAprobacion or EstadoDocumento.Aprobado)
        {
            var lineas = await _lineaRepo.FindAsync(l => l.OrdenCompraId == id, ct);
            if (lineas.Count == 0)
                return Result<OrdenCompraDto>.Failure("Cannot advance an order without lines.");
        }

        oc.Estado = estadoTarget;
        await _ocRepo.UpdateAsync(oc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<OrdenCompraDto>.Success(await BuildFullDto(oc, ct));
    }

    /// <summary>
    /// Aprueba o rechaza la OC (gate de Finanzas/Dirección).
    /// PendienteAprobacion ? Aprobado | Rechazado.
    /// </summary>
    public async Task<Result<OrdenCompraDto>> AprobarRechazarAsync(long id, AprobarRechazarOrdenCompraDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<OrdenCompraDto>.Failure("No active company.");

        var oc = await _ocRepo.GetByIdAsync(id, ct);
        if (oc is null || oc.EmpresaId != empresaId.Value || !oc.Activo)
            return Result<OrdenCompraDto>.Failure("Purchase order not found.");

        if (oc.Estado != EstadoDocumento.PendienteAprobacion)
            return Result<OrdenCompraDto>.Failure("Only orders in PendienteAprobacion can be approved/rejected.");

        if (dto.Aprobado)
        {
            oc.Estado = EstadoDocumento.Aprobado;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.MotivoRechazo))
                return Result<OrdenCompraDto>.Failure("A rejection reason is required.");
            oc.Estado = EstadoDocumento.Rechazado;
            oc.MotivoRechazo = dto.MotivoRechazo;
        }

        await _ocRepo.UpdateAsync(oc, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<OrdenCompraDto>.Success(await BuildFullDto(oc, ct));
    }

    /// <summary>
    /// Registra la confirmación o negociación del proveedor (PI / aceptación).
    /// Gate [4]: si CondicionesOK ? ConfirmadaProveedor, si no ? EnNegociacion.
    /// </summary>
    public async Task<Result<ConfirmacionProveedorDto>> RegistrarConfirmacionProveedorAsync(
        long id, RegistrarConfirmacionProveedorDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<ConfirmacionProveedorDto>.Failure("No active company.");

        var oc = await _ocRepo.GetByIdAsync(id, ct);
        if (oc is null || oc.EmpresaId != empresaId.Value || !oc.Activo)
            return Result<ConfirmacionProveedorDto>.Failure("Purchase order not found.");

        if (oc.Estado is not (EstadoDocumento.EnviadaProveedor or EstadoDocumento.EnNegociacion))
            return Result<ConfirmacionProveedorDto>.Failure(
                "Order must be in EnviadaProveedor or EnNegociacion state to register supplier confirmation.");

        // Count previous iterations
        var prevConf = await _confProvRepo.FindAsync(c => c.OrdenCompraId == id, ct);
        var iteracion = prevConf.Count + 1;

        var nuevoEstado = dto.CondicionesOK ? EstadoDocumento.ConfirmadaProveedor : EstadoDocumento.EnNegociacion;

        var conf = new ConfirmacionProveedor
        {
            EmpresaId = empresaId.Value,
            OrdenCompraId = id,
            NumeroProforma = dto.NumeroProforma,
            FechaConfirmacion = dto.FechaConfirmacion,
            CondicionesOK = dto.CondicionesOK,
            ObservacionesProveedor = dto.ObservacionesProveedor,
            FechaEntregaComprometida = dto.FechaEntregaComprometida,
            Estado = nuevoEstado,
            Observaciones = dto.Observaciones,
            IteracionNegociacion = iteracion,
            Activo = true
        };

        await _confProvRepo.AddAsync(conf, ct);

        // Update OC state and delivery date if confirmed
        oc.Estado = nuevoEstado;
        if (dto.CondicionesOK && dto.FechaEntregaComprometida.HasValue)
            oc.FechaEntregaEstimada = dto.FechaEntregaComprometida;

        await _ocRepo.UpdateAsync(oc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ConfirmacionProveedorDto>.Success(new ConfirmacionProveedorDto(
            conf.ConfirmacionProveedorId,
            conf.OrdenCompraId,
            oc.Numero,
            conf.NumeroProforma,
            conf.FechaConfirmacion,
            conf.CondicionesOK,
            conf.ObservacionesProveedor,
            conf.FechaEntregaComprometida,
            conf.Estado.ToString(),
            conf.Observaciones,
            conf.IteracionNegociacion));
    }

    /// <summary>
    /// Programa un pago (anticipo o saldo) asociado a la OC.
    /// La OC debe estar en ConfirmadaProveedor o posterior.
    /// </summary>
    public async Task<Result<PagoOrdenCompraDto>> ProgramarPagoAsync(long id, ProgramarPagoDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<PagoOrdenCompraDto>.Failure("No active company.");

        var oc = await _ocRepo.GetByIdAsync(id, ct);
        if (oc is null || oc.EmpresaId != empresaId.Value || !oc.Activo)
            return Result<PagoOrdenCompraDto>.Failure("Purchase order not found.");

        var estadosValidos = new[]
        {
            EstadoDocumento.Aprobado,
            EstadoDocumento.ConfirmadaProveedor,
            EstadoDocumento.PagoProgramado
        };
        if (!estadosValidos.Contains(oc.Estado))
            return Result<PagoOrdenCompraDto>.Failure(
                $"Cannot program payment for order in state {oc.Estado}.");

        if (!Enum.TryParse<TipoPagoImportacion>(dto.TipoPago, true, out var tipoPago))
            return Result<PagoOrdenCompraDto>.Failure($"Invalid payment type: {dto.TipoPago}. Valid values: Anticipo, Saldo, Total.");

        var pago = new PagoOrdenCompra
        {
            EmpresaId = empresaId.Value,
            OrdenCompraId = id,
            TipoPago = tipoPago,
            MontoProgramado = dto.MontoProgramado,
            MonedaId = dto.MonedaId,
            TasaCambio = dto.TasaCambio,
            FechaProgramada = dto.FechaProgramada,
            Observaciones = dto.Observaciones,
            Ejecutado = false,
            Activo = true
        };

        await _pagoRepo.AddAsync(pago, ct);

        if (oc.Estado == EstadoDocumento.ConfirmadaProveedor)
        {
            oc.Estado = EstadoDocumento.PagoProgramado;
            await _ocRepo.UpdateAsync(oc, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return Result<PagoOrdenCompraDto>.Success(new PagoOrdenCompraDto(
            pago.PagoOrdenCompraId, pago.OrdenCompraId, oc.Numero,
            pago.TipoPago.ToString(), pago.MontoProgramado, pago.MonedaId,
            pago.TasaCambio, pago.FechaProgramada, null, null,
            false, null, pago.Observaciones));
    }

    /// <summary>Registra la ejecución real de un pago programado.</summary>
    public async Task<Result<PagoOrdenCompraDto>> EjecutarPagoAsync(long id, long pagoId, EjecutarPagoDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<PagoOrdenCompraDto>.Failure("No active company.");

        var oc = await _ocRepo.GetByIdAsync(id, ct);
        if (oc is null || oc.EmpresaId != empresaId.Value || !oc.Activo)
            return Result<PagoOrdenCompraDto>.Failure("Purchase order not found.");

        var pago = await _pagoRepo.GetByIdAsync(pagoId, ct);
        if (pago is null || pago.OrdenCompraId != id)
            return Result<PagoOrdenCompraDto>.Failure("Payment not found.");

        if (pago.Ejecutado)
            return Result<PagoOrdenCompraDto>.Failure("Payment has already been executed.");

        pago.FechaEjecucion = dto.FechaEjecucion;
        pago.MontoEjecutado = dto.MontoEjecutado;
        pago.ReferenciaTransferencia = dto.ReferenciaTransferencia;
        pago.Ejecutado = true;
        if (!string.IsNullOrEmpty(dto.Observaciones))
            pago.Observaciones = dto.Observaciones;

        await _pagoRepo.UpdateAsync(pago, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<PagoOrdenCompraDto>.Success(new PagoOrdenCompraDto(
            pago.PagoOrdenCompraId, pago.OrdenCompraId, oc.Numero,
            pago.TipoPago.ToString(), pago.MontoProgramado, pago.MonedaId,
            pago.TasaCambio, pago.FechaProgramada, pago.FechaEjecucion,
            pago.MontoEjecutado, pago.Ejecutado, pago.ReferenciaTransferencia,
            pago.Observaciones));
    }

    /// <summary>Obtiene todos los pagos de una OC.</summary>
    public async Task<Result<IReadOnlyList<PagoOrdenCompraDto>>> GetPagosAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<PagoOrdenCompraDto>>.Failure("No active company.");

        var oc = await _ocRepo.GetByIdAsync(id, ct);
        if (oc is null || oc.EmpresaId != empresaId.Value || !oc.Activo)
            return Result<IReadOnlyList<PagoOrdenCompraDto>>.Failure("Purchase order not found.");

        var pagos = await _pagoRepo.FindAsync(p => p.OrdenCompraId == id, ct);
        var dtos = pagos.OrderBy(p => p.FechaProgramada).Select(p => new PagoOrdenCompraDto(
            p.PagoOrdenCompraId, p.OrdenCompraId, oc.Numero,
            p.TipoPago.ToString(), p.MontoProgramado, p.MonedaId,
            p.TasaCambio, p.FechaProgramada, p.FechaEjecucion,
            p.MontoEjecutado, p.Ejecutado, p.ReferenciaTransferencia,
            p.Observaciones)).ToList().AsReadOnly();

        return Result<IReadOnlyList<PagoOrdenCompraDto>>.Success(dtos);
    }

    /// <summary>Obtiene el historial de confirmaciones del proveedor para una OC.</summary>
    public async Task<Result<IReadOnlyList<ConfirmacionProveedorDto>>> GetConfirmacionesAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<ConfirmacionProveedorDto>>.Failure("No active company.");

        var oc = await _ocRepo.GetByIdAsync(id, ct);
        if (oc is null || oc.EmpresaId != empresaId.Value || !oc.Activo)
            return Result<IReadOnlyList<ConfirmacionProveedorDto>>.Failure("Purchase order not found.");

        var confs = await _confProvRepo.FindAsync(c => c.OrdenCompraId == id, ct);
        var dtos = confs.OrderBy(c => c.IteracionNegociacion).Select(c => new ConfirmacionProveedorDto(
            c.ConfirmacionProveedorId, c.OrdenCompraId, oc.Numero,
            c.NumeroProforma, c.FechaConfirmacion, c.CondicionesOK,
            c.ObservacionesProveedor, c.FechaEntregaComprometida,
            c.Estado.ToString(), c.Observaciones, c.IteracionNegociacion
        )).ToList().AsReadOnly();

        return Result<IReadOnlyList<ConfirmacionProveedorDto>>.Success(dtos);
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No active company.");

        var oc = await _ocRepo.GetByIdAsync(id, ct);
        if (oc is null || oc.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("Purchase order not found.");
        if (oc.Estado != EstadoDocumento.Borrador)
            return Result<bool>.Failure("Only draft orders can be deleted.");

        oc.Activo = false;
        await _ocRepo.UpdateAsync(oc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    // ?? Private helpers ????????????????????????????????????????????????????????

    private async Task<string> GenerarNumeroAsync(int empresaId, CancellationToken ct)
    {
        var numeraciones = await _numeracionRepo.FindAsync(
            n => n.EmpresaId == empresaId && n.TipoDocumento == "OC", ct);

        var num = numeraciones.FirstOrDefault();
        if (num is not null)
        {
            var numero = num.GenerarSiguiente();
            await _numeracionRepo.UpdateAsync(num, ct);
            return numero;
        }

        // Fallback: generate from existing count
        var existentes = await _ocRepo.FindAsync(o => o.EmpresaId == empresaId, ct);
        return $"OC-{(existentes.Count + 1):D6}";
    }

    private async Task<OrdenCompraDto> BuildFullDto(OrdenCompra oc, CancellationToken ct)
    {
        var lineas = await _lineaRepo.FindAsync(l => l.OrdenCompraId == oc.OrdenCompraId, ct);
        var proveedores = await _proveedorRepo.FindAsync(p => p.Id == oc.ProveedorId, ct);
        var almacenes = await _almacenRepo.FindAsync(a => a.Id == oc.AlmacenDestinoId, ct);
        var cpIds = lineas.Select(l => l.CompanyProductId).Distinct().ToList();
        var cps = cpIds.Count > 0
            ? await _companyProductRepo.FindAsync(p => cpIds.Contains(p.CompanyProductId), ct)
            : new List<CompanyProduct>();

        var provMap = proveedores.ToDictionary(p => p.Id, p => p.RazonSocial);
        var almMap = almacenes.ToDictionary(a => a.Id, a => a.Nombre);
        var cpMap = cps.ToDictionary(p => p.CompanyProductId, p => p.Sku);

        return MapToDto(oc, lineas.OrderBy(l => l.NumeroLinea).ToList(), provMap, almMap, cpMap);
    }

    private static OrdenCompraDto MapToDto(
        OrdenCompra oc,
        IList<OrdenCompraLinea> lineas,
        Dictionary<int, string> provMap,
        Dictionary<int, string> almMap,
        Dictionary<long, string> cpMap)
    {
        return new OrdenCompraDto(
            oc.OrdenCompraId,
            oc.EmpresaId,
            oc.Numero,
            oc.FechaEmision,
            oc.FechaEntregaEstimada,
            oc.ProveedorId,
            provMap.GetValueOrDefault(oc.ProveedorId, "—"),
            oc.AlmacenDestinoId,
            almMap.GetValueOrDefault(oc.AlmacenDestinoId, "—"),
            oc.MonedaId,
            oc.TasaCambio,
            oc.Estado.ToString(),
            oc.CondicionPago,
            oc.Incoterm,
            oc.Observaciones,
            oc.ReferenciaExterna,
            oc.MotivoRechazo,
            oc.Subtotal,
            oc.Descuento,
            oc.Impuesto,
            oc.Total,
            oc.Activo,
            oc.OrdenPedidoId,
            oc.ExpedienteImportacionId,
            lineas.Select(l => new OrdenCompraLineaDto(
                l.OrdenCompraLineaId,
                l.NumeroLinea,
                l.CompanyProductId,
                cpMap.GetValueOrDefault(l.CompanyProductId, "—"),
                l.Descripcion,
                l.UnidadMedida,
                l.Cantidad,
                l.PrecioUnitario,
                l.PorcentajeDescuento,
                l.MontoDescuento,
                l.Subtotal,
                l.PorcentajeImpuesto,
                l.MontoImpuesto,
                l.TotalLinea,
                l.CantidadRecepcionada,
                l.CantidadPendiente
            )).ToList());
    }
}
