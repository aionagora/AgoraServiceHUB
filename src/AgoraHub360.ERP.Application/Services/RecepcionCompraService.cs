namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.CMP;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.INV;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Compras;

public class RecepcionCompraService : IRecepcionCompraService
{
    private readonly IRepository<RecepcionCompra> _recRepo;
    private readonly IRepository<RecepcionCompraLinea> _recLineaRepo;
    private readonly IRepository<OrdenCompra> _ocRepo;
    private readonly IRepository<OrdenCompraLinea> _ocLineaRepo;
    private readonly IRepository<Almacen> _almacenRepo;
    private readonly IRepository<CompanyProduct> _cpRepo;
    private readonly IRepository<NumeracionDocumento> _numRepo;
    private readonly IRepository<MovimientoInventario> _movRepo;
    private readonly IRepository<StockProducto> _stockRepo;
    private readonly IContabilizacionService _contabilizacion;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RecepcionCompraService(
        IRepository<RecepcionCompra> recRepo,
        IRepository<RecepcionCompraLinea> recLineaRepo,
        IRepository<OrdenCompra> ocRepo,
        IRepository<OrdenCompraLinea> ocLineaRepo,
        IRepository<Almacen> almacenRepo,
        IRepository<CompanyProduct> cpRepo,
        IRepository<NumeracionDocumento> numRepo,
        IRepository<MovimientoInventario> movRepo,
        IRepository<StockProducto> stockRepo,
        IContabilizacionService contabilizacion,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _recRepo = recRepo;
        _recLineaRepo = recLineaRepo;
        _ocRepo = ocRepo;
        _ocLineaRepo = ocLineaRepo;
        _almacenRepo = almacenRepo;
        _cpRepo = cpRepo;
        _numRepo = numRepo;
        _movRepo = movRepo;
        _stockRepo = stockRepo;
        _contabilizacion = contabilizacion;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<RecepcionCompraDto>>> GetAllAsync(
        long? ordenCompraId, DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<RecepcionCompraDto>>.Failure("No active company.");

        var recepciones = await _recRepo.FindAsync(
            r => r.EmpresaId == empresaId.Value && r.Activo
                && (!ordenCompraId.HasValue || r.OrdenCompraId == ordenCompraId.Value)
                && (!fechaDesde.HasValue || r.FechaRecepcion >= fechaDesde.Value)
                && (!fechaHasta.HasValue || r.FechaRecepcion <= fechaHasta.Value.AddDays(1).AddSeconds(-1)),
            ct);

        var dtos = new List<RecepcionCompraDto>();
        foreach (var rec in recepciones.OrderByDescending(r => r.FechaRecepcion).ThenByDescending(r => r.RecepcionCompraId))
        {
            dtos.Add(await BuildDto(rec, ct));
        }

        return Result<IReadOnlyList<RecepcionCompraDto>>.Success(dtos.AsReadOnly());
    }

    public async Task<Result<RecepcionCompraDto>> GetByIdAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<RecepcionCompraDto>.Failure("No active company.");

        var rec = await _recRepo.GetByIdAsync(id, ct);
        if (rec is null || rec.EmpresaId != empresaId.Value || !rec.Activo)
            return Result<RecepcionCompraDto>.Failure("Reception not found.");

        return Result<RecepcionCompraDto>.Success(await BuildDto(rec, ct));
    }

    public async Task<Result<RecepcionCompraDto>> CreateAsync(CreateRecepcionCompraDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<RecepcionCompraDto>.Failure("No active company.");

        // ?? Validar OC ??
        var oc = await _ocRepo.GetByIdAsync(dto.OrdenCompraId, ct);
        if (oc is null || oc.EmpresaId != empresaId.Value || !oc.Activo)
            return Result<RecepcionCompraDto>.Failure("Purchase order not found.");

        if (oc.Estado is not (EstadoDocumento.Aprobado or EstadoDocumento.RecepcionParcial))
            return Result<RecepcionCompraDto>.Failure(
                $"Only approved or partially received orders can be received. Current status: {oc.Estado}.");

        // ?? Validar almacén ??
        var almacenId = dto.AlmacenId ?? oc.AlmacenDestinoId;
        var almacen = await _almacenRepo.GetByIdAsync(almacenId, ct);
        if (almacen is null || almacen.EmpresaId != empresaId.Value)
            return Result<RecepcionCompraDto>.Failure("Warehouse not found.");

        // ?? Validar líneas ??
        if (dto.Lineas is null || dto.Lineas.Count == 0)
            return Result<RecepcionCompraDto>.Failure("At least one reception line is required.");

        var ocLineas = await _ocLineaRepo.FindAsync(l => l.OrdenCompraId == dto.OrdenCompraId, ct);
        var ocLineasMap = ocLineas.ToDictionary(l => l.OrdenCompraLineaId);

        foreach (var lineaDto in dto.Lineas)
        {
            if (!ocLineasMap.TryGetValue(lineaDto.OrdenCompraLineaId, out var ocLinea))
                return Result<RecepcionCompraDto>.Failure($"OC line {lineaDto.OrdenCompraLineaId} not found.");

            if (lineaDto.CantidadRecibida <= 0)
                return Result<RecepcionCompraDto>.Failure(
                    $"Received quantity must be greater than zero (line {ocLinea.NumeroLinea}).");

            var pendiente = ocLinea.Cantidad - ocLinea.CantidadRecepcionada;
            if (lineaDto.CantidadRecibida > pendiente)
                return Result<RecepcionCompraDto>.Failure(
                    $"Received quantity ({lineaDto.CantidadRecibida:N2}) exceeds pending ({pendiente:N2}) on line {ocLinea.NumeroLinea}.");
        }

        // ?? Generar número ??
        var numero = await GenerarNumeroAsync(empresaId.Value, ct);

        // ?? Crear recepción ??
        var recepcion = new RecepcionCompra
        {
            EmpresaId = empresaId.Value,
            Numero = numero,
            OrdenCompraId = dto.OrdenCompraId,
            FechaRecepcion = dto.FechaRecepcion,
            AlmacenId = almacenId,
            DocumentoProveedor = dto.DocumentoProveedor,
            Observaciones = dto.Observaciones,
            TieneDiferencias = dto.TieneDiferencias,
            TipoDiferencia = dto.TipoDiferencia,
            ActaDiferencias = dto.ActaDiferencias,
            NumeroReclamo = dto.NumeroReclamo,
            EnCuarentena = dto.EnCuarentena,
            UbicacionCuarentena = dto.UbicacionCuarentena,
            ResultadoControlCalidad = dto.ResultadoControlCalidad,
            Confirmada = true,
            Activo = true
        };

        await _recRepo.AddAsync(recepcion, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // ?? Crear líneas + movimientos de inventario ??
        foreach (var lineaDto in dto.Lineas)
        {
            var ocLinea = ocLineasMap[lineaDto.OrdenCompraLineaId];
            var costoUnit = lineaDto.CostoUnitario ?? ocLinea.PrecioUnitario;

            // Línea de recepción
            var recLinea = new RecepcionCompraLinea
            {
                RecepcionCompraId = recepcion.RecepcionCompraId,
                OrdenCompraLineaId = lineaDto.OrdenCompraLineaId,
                CantidadRecibida = lineaDto.CantidadRecibida,
                CantidadDañada = lineaDto.CantidadDañada,
                CantidadSobrante = lineaDto.CantidadSobrante,
                CantidadFaltante = lineaDto.CantidadFaltante,
                CostoUnitario = costoUnit,
                Notas = lineaDto.Notas,
                Activo = true
            };
            await _recLineaRepo.AddAsync(recLinea, ct);

            // Actualizar CantidadRecepcionada en la línea de OC
            ocLinea.CantidadRecepcionada += lineaDto.CantidadRecibida;
            await _ocLineaRepo.UpdateAsync(ocLinea, ct);

            // Crear movimiento de inventario tipo Receipt
            var movNumber = await GenerarNumeroMovimientoAsync(empresaId.Value, dto.FechaRecepcion, ct);
            var movimiento = new MovimientoInventario
            {
                EmpresaId = empresaId.Value,
                Number = movNumber,
                MovementType = "Receipt",
                MovementDate = dto.FechaRecepcion,
                CompanyProductId = ocLinea.CompanyProductId,
                WarehouseId = almacenId,
                Quantity = lineaDto.CantidadRecibida,
                UnitCost = costoUnit,
                TotalCost = costoUnit * lineaDto.CantidadRecibida,
                Reference = $"{oc.Numero} ? {numero}",
                Notes = $"Recepción automática de OC {oc.Numero}, línea {ocLinea.NumeroLinea}",
                Activo = true
            };
            await _movRepo.AddAsync(movimiento, ct);

            // Actualizar stock + WAC
            var (stock, isNew) = await GetOrCreateStock(empresaId.Value, ocLinea.CompanyProductId, almacenId, ct);
            UpdateAverageCost(stock, lineaDto.CantidadRecibida, costoUnit);
            stock.CurrentStock += lineaDto.CantidadRecibida;
            stock.LastUpdated = DateTime.UtcNow;
            if (!isNew)
                await _stockRepo.UpdateAsync(stock, ct);
        }

        // ?? Actualizar estado de la OC ??
        var todasOcLineas = await _ocLineaRepo.FindAsync(l => l.OrdenCompraId == dto.OrdenCompraId, ct);
        var todasCompletas = todasOcLineas.All(l => l.CantidadRecepcionada >= l.Cantidad);

        if (todasCompletas)
            oc.Estado = dto.TieneDiferencias ? EstadoDocumento.RecepcionConDiferencias : EstadoDocumento.Cerrado;
        else
            oc.Estado = EstadoDocumento.RecepcionParcial;

        await _ocRepo.UpdateAsync(oc, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        // ?? Contabilización automática ??
        var subtotal = dto.Lineas.Sum(l =>
        {
            var ocL = ocLineasMap[l.OrdenCompraLineaId];
            var costo = l.CostoUnitario ?? ocL.PrecioUnitario;
            return costo * l.CantidadRecibida;
        });
        var impuesto = subtotal * 0.13m; // IVA 13% — configurable via plantilla
        var totalRecepcion = subtotal + impuesto;

        await _contabilizacion.ContabilizarDocumentoAsync(
            tipoDocumento: "Recepcion",
            montos: new Dictionary<string, decimal>
            {
                ["Subtotal"] = subtotal,
                ["Impuesto"] = impuesto,
                ["Total"] = totalRecepcion
            },
            origenId: recepcion.RecepcionCompraId,
            origenReferencia: recepcion.Numero,
            fecha: recepcion.FechaRecepcion,
            glosaExtra: oc.Numero,
            ct: ct);

        return Result<RecepcionCompraDto>.Success(await BuildDto(recepcion, ct));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No active company.");

        var rec = await _recRepo.GetByIdAsync(id, ct);
        if (rec is null || rec.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("Reception not found.");

        if (rec.Confirmada)
            return Result<bool>.Failure("Confirmed receptions cannot be deleted. They have already generated inventory movements.");

        rec.Activo = false;
        await _recRepo.UpdateAsync(rec, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    // ?? Private helpers ??

    private async Task<string> GenerarNumeroAsync(int empresaId, CancellationToken ct)
    {
        var numeraciones = await _numRepo.FindAsync(
            n => n.EmpresaId == empresaId && n.TipoDocumento == "REC", ct);

        var num = numeraciones.FirstOrDefault();
        if (num is not null)
        {
            var numero = num.GenerarSiguiente();
            await _numRepo.UpdateAsync(num, ct);
            return numero;
        }

        var existentes = await _recRepo.FindAsync(r => r.EmpresaId == empresaId, ct);
        return $"REC-{(existentes.Count + 1):D6}";
    }

    private async Task<string> GenerarNumeroMovimientoAsync(int empresaId, DateTime fecha, CancellationToken ct)
    {
        var movimientos = await _movRepo.FindAsync(m => m.EmpresaId == empresaId, ct);
        return $"MOV-{fecha.Year}-{(movimientos.Count + 1):D5}";
    }

    private async Task<(StockProducto stock, bool isNew)> GetOrCreateStock(int empresaId, long companyProductId, int almacenId, CancellationToken ct)
    {
        var stocks = await _stockRepo.FindAsync(
            s => s.EmpresaId == empresaId
              && s.CompanyProductId == companyProductId
              && s.AlmacenId == almacenId, ct);

        if (stocks.Any())
            return (stocks.First(), false);

        var nuevo = new StockProducto
        {
            EmpresaId = empresaId,
            CompanyProductId = companyProductId,
            AlmacenId = almacenId,
            CurrentStock = 0,
            AverageCost = 0,
            LastUpdated = DateTime.UtcNow,
            Activo = true
        };
        await _stockRepo.AddAsync(nuevo, ct);
        return (nuevo, true);
    }

    private static void UpdateAverageCost(StockProducto stock, decimal inQty, decimal newCost)
    {
        if (stock.CurrentStock <= 0)
        {
            stock.AverageCost = newCost;
            return;
        }
        stock.AverageCost =
            (stock.CurrentStock * stock.AverageCost + inQty * newCost)
            / (stock.CurrentStock + inQty);
    }

    private async Task<RecepcionCompraDto> BuildDto(RecepcionCompra rec, CancellationToken ct)
    {
        var oc = await _ocRepo.GetByIdAsync(rec.OrdenCompraId, ct);
        var almacenes = await _almacenRepo.FindAsync(a => a.Id == rec.AlmacenId, ct);
        var recLineas = await _recLineaRepo.FindAsync(l => l.RecepcionCompraId == rec.RecepcionCompraId, ct);
        var ocLineaIds = recLineas.Select(l => l.OrdenCompraLineaId).Distinct().ToList();
        var ocLineas = ocLineaIds.Count > 0
            ? await _ocLineaRepo.FindAsync(l => ocLineaIds.Contains(l.OrdenCompraLineaId), ct)
            : new List<OrdenCompraLinea>();
        var cpIds = ocLineas.Select(l => l.CompanyProductId).Distinct().ToList();
        var cps = cpIds.Count > 0
            ? await _cpRepo.FindAsync(p => cpIds.Contains(p.CompanyProductId), ct)
            : new List<CompanyProduct>();

        var ocLineaMap = ocLineas.ToDictionary(l => l.OrdenCompraLineaId);
        var cpMap = cps.ToDictionary(p => p.CompanyProductId, p => p.Sku);

        return new RecepcionCompraDto(
            rec.RecepcionCompraId,
            rec.EmpresaId,
            rec.Numero,
            rec.OrdenCompraId,
            oc?.Numero ?? "—",
            rec.FechaRecepcion,
            rec.AlmacenId,
            almacenes.FirstOrDefault()?.Nombre ?? "—",
            rec.DocumentoProveedor,
            rec.Observaciones,
            rec.Confirmada,
            rec.TieneDiferencias,
            rec.TipoDiferencia,
            rec.ActaDiferencias,
            rec.NumeroReclamo,
            rec.EnCuarentena,
            rec.UbicacionCuarentena,
            rec.ResultadoControlCalidad,
            recLineas.Select(l =>
            {
                var ocl = ocLineaMap.GetValueOrDefault(l.OrdenCompraLineaId);
                return new RecepcionCompraLineaDto(
                    l.RecepcionCompraLineaId,
                    l.OrdenCompraLineaId,
                    ocl?.NumeroLinea ?? 0,
                    cpMap.GetValueOrDefault(ocl?.CompanyProductId ?? 0, "—"),
                    ocl?.Descripcion ?? "—",
                    ocl?.UnidadMedida ?? "UND",
                    ocl?.Cantidad ?? 0,
                    ocl?.CantidadPendiente ?? 0,
                    l.CantidadRecibida,
                    l.CantidadDañada,
                    l.CantidadSobrante,
                    l.CantidadFaltante,
                    l.CantidadAceptada,
                    l.CostoUnitario,
                    l.Notas);
            }).ToList());
    }
}
