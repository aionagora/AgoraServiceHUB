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

public class HojaImportacionService : IHojaImportacionService
{
    private readonly IRepository<HojaImportacion> _hojaRepo;
    private readonly IRepository<GastoImportacion> _gastoRepo;
    private readonly IRepository<ImportacionLinea> _impLineaRepo;
    private readonly IRepository<OrdenCompra> _ocRepo;
    private readonly IRepository<OrdenCompraLinea> _ocLineaRepo;
    private readonly IRepository<CompanyProduct> _cpRepo;
    private readonly IRepository<NumeracionDocumento> _numRepo;
    private readonly IRepository<StockProducto> _stockRepo;
    private readonly IRepository<MovimientoInventario> _movRepo;
    private readonly IRepository<Almacen> _almacenRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public HojaImportacionService(
        IRepository<HojaImportacion> hojaRepo,
        IRepository<GastoImportacion> gastoRepo,
        IRepository<ImportacionLinea> impLineaRepo,
        IRepository<OrdenCompra> ocRepo,
        IRepository<OrdenCompraLinea> ocLineaRepo,
        IRepository<CompanyProduct> cpRepo,
        IRepository<NumeracionDocumento> numRepo,
        IRepository<StockProducto> stockRepo,
        IRepository<MovimientoInventario> movRepo,
        IRepository<Almacen> almacenRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _hojaRepo = hojaRepo;
        _gastoRepo = gastoRepo;
        _impLineaRepo = impLineaRepo;
        _ocRepo = ocRepo;
        _ocLineaRepo = ocLineaRepo;
        _cpRepo = cpRepo;
        _numRepo = numRepo;
        _stockRepo = stockRepo;
        _movRepo = movRepo;
        _almacenRepo = almacenRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<HojaImportacionDto>>> GetAllAsync(
        long? ordenCompraId, DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<HojaImportacionDto>>.Failure("No active company.");

        var hojas = await _hojaRepo.FindAsync(
            h => h.EmpresaId == empresaId.Value && h.Activo
                && (!ordenCompraId.HasValue || h.OrdenCompraId == ordenCompraId.Value)
                && (!fechaDesde.HasValue || h.Fecha >= fechaDesde.Value)
                && (!fechaHasta.HasValue || h.Fecha <= fechaHasta.Value.AddDays(1).AddSeconds(-1)),
            ct);

        var dtos = new List<HojaImportacionDto>();
        foreach (var h in hojas.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.HojaImportacionId))
            dtos.Add(await BuildDto(h, ct));

        return Result<IReadOnlyList<HojaImportacionDto>>.Success(dtos.AsReadOnly());
    }

    public async Task<Result<HojaImportacionDto>> GetByIdAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<HojaImportacionDto>.Failure("No active company.");

        var hoja = await _hojaRepo.GetByIdAsync(id, ct);
        if (hoja is null || hoja.EmpresaId != empresaId.Value || !hoja.Activo)
            return Result<HojaImportacionDto>.Failure("Import sheet not found.");

        return Result<HojaImportacionDto>.Success(await BuildDto(hoja, ct));
    }

    public async Task<Result<HojaImportacionDto>> CreateAsync(CreateHojaImportacionDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<HojaImportacionDto>.Failure("No active company.");

        // Validate OC — must be recepcionada (parcial o cerrada)
        var oc = await _ocRepo.GetByIdAsync(dto.OrdenCompraId, ct);
        if (oc is null || oc.EmpresaId != empresaId.Value || !oc.Activo)
            return Result<HojaImportacionDto>.Failure("Purchase order not found.");

        if (oc.Estado is not (EstadoDocumento.RecepcionParcial or EstadoDocumento.Cerrado or EstadoDocumento.Aprobado))
            return Result<HojaImportacionDto>.Failure(
                $"Import sheets can only be created for received orders. Current status: {oc.Estado}.");

        // Check OC has received lines
        var ocLineas = await _ocLineaRepo.FindAsync(l => l.OrdenCompraId == dto.OrdenCompraId, ct);
        var lineasRecibidas = ocLineas.Where(l => l.CantidadRecepcionada > 0).ToList();
        if (lineasRecibidas.Count == 0)
            return Result<HojaImportacionDto>.Failure("No received lines found on this purchase order.");

        // Validate gastos
        if (dto.Gastos is null || dto.Gastos.Count == 0)
            return Result<HojaImportacionDto>.Failure("At least one import expense is required.");

        var numero = await GenerarNumeroAsync(empresaId.Value, ct);

        var hoja = new HojaImportacion
        {
            EmpresaId = empresaId.Value,
            Numero = numero,
            OrdenCompraId = dto.OrdenCompraId,
            Fecha = dto.Fecha,
            ReferenciaAduanera = dto.ReferenciaAduanera,
            Observaciones = dto.Observaciones,
            MetodoDistribucion = dto.MetodoDistribucion,
            Liquidada = false,
            Activo = true
        };

        await _hojaRepo.AddAsync(hoja, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Add gastos
        decimal totalGastos = 0;
        foreach (var g in dto.Gastos)
        {
            var montoBase = g.Monto * g.TasaCambio;
            var gasto = new GastoImportacion
            {
                HojaImportacionId = hoja.HojaImportacionId,
                TipoGasto = g.TipoGasto,
                Descripcion = g.Descripcion,
                Monto = g.Monto,
                MonedaId = g.MonedaId,
                TasaCambio = g.TasaCambio,
                MontoBase = montoBase,
                Referencia = g.Referencia,
                Activo = true
            };
            totalGastos += montoBase;
            await _gastoRepo.AddAsync(gasto, ct);
        }

        hoja.TotalGastos = totalGastos;
        await _hojaRepo.UpdateAsync(hoja, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<HojaImportacionDto>.Success(await BuildDto(hoja, ct));
    }

    public async Task<Result<HojaImportacionDto>> AddGastoAsync(long hojaId, AddGastoImportacionDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<HojaImportacionDto>.Failure("No active company.");

        var hoja = await _hojaRepo.GetByIdAsync(hojaId, ct);
        if (hoja is null || hoja.EmpresaId != empresaId.Value || !hoja.Activo)
            return Result<HojaImportacionDto>.Failure("Import sheet not found.");
        if (hoja.Liquidada)
            return Result<HojaImportacionDto>.Failure("Cannot modify a settled import sheet.");

        var montoBase = dto.Monto * dto.TasaCambio;
        var gasto = new GastoImportacion
        {
            HojaImportacionId = hojaId,
            TipoGasto = dto.TipoGasto,
            Descripcion = dto.Descripcion,
            Monto = dto.Monto,
            MonedaId = dto.MonedaId,
            TasaCambio = dto.TasaCambio,
            MontoBase = montoBase,
            Referencia = dto.Referencia,
            Activo = true
        };
        await _gastoRepo.AddAsync(gasto, ct);

        // Recalculate total
        var gastos = await _gastoRepo.FindAsync(g => g.HojaImportacionId == hojaId, ct);
        hoja.TotalGastos = gastos.Sum(g => g.MontoBase) + montoBase;
        await _hojaRepo.UpdateAsync(hoja, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<HojaImportacionDto>.Success(await BuildDto(hoja, ct));
    }

    public async Task<Result<HojaImportacionDto>> RemoveGastoAsync(long hojaId, long gastoId, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<HojaImportacionDto>.Failure("No active company.");

        var hoja = await _hojaRepo.GetByIdAsync(hojaId, ct);
        if (hoja is null || hoja.EmpresaId != empresaId.Value || !hoja.Activo)
            return Result<HojaImportacionDto>.Failure("Import sheet not found.");
        if (hoja.Liquidada)
            return Result<HojaImportacionDto>.Failure("Cannot modify a settled import sheet.");

        var gasto = await _gastoRepo.GetByIdAsync(gastoId, ct);
        if (gasto is null || gasto.HojaImportacionId != hojaId)
            return Result<HojaImportacionDto>.Failure("Expense not found.");

        await _gastoRepo.DeleteAsync(gasto, ct);

        var gastosRestantes = await _gastoRepo.FindAsync(g => g.HojaImportacionId == hojaId && g.GastoImportacionId != gastoId, ct);
        hoja.TotalGastos = gastosRestantes.Sum(g => g.MontoBase);
        await _hojaRepo.UpdateAsync(hoja, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<HojaImportacionDto>.Success(await BuildDto(hoja, ct));
    }

    public async Task<Result<HojaImportacionDto>> LiquidarAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<HojaImportacionDto>.Failure("No active company.");

        var hoja = await _hojaRepo.GetByIdAsync(id, ct);
        if (hoja is null || hoja.EmpresaId != empresaId.Value || !hoja.Activo)
            return Result<HojaImportacionDto>.Failure("Import sheet not found.");
        if (hoja.Liquidada)
            return Result<HojaImportacionDto>.Failure("Import sheet is already settled.");

        var gastos = await _gastoRepo.FindAsync(g => g.HojaImportacionId == id, ct);
        if (gastos.Count == 0)
            return Result<HojaImportacionDto>.Failure("Cannot settle without expenses.");

        var totalGastos = gastos.Sum(g => g.MontoBase);

        // Get OC and received lines
        var oc = await _ocRepo.GetByIdAsync(hoja.OrdenCompraId, ct);
        if (oc is null)
            return Result<HojaImportacionDto>.Failure("Purchase order not found.");

        var ocLineas = await _ocLineaRepo.FindAsync(l => l.OrdenCompraId == hoja.OrdenCompraId, ct);
        var lineasRecibidas = ocLineas.Where(l => l.CantidadRecepcionada > 0).ToList();

        if (lineasRecibidas.Count == 0)
            return Result<HojaImportacionDto>.Failure("No received lines to distribute costs.");

        // ?? Calculate distribution factors ??
        decimal totalFactor = 0;
        var factores = new Dictionary<long, decimal>();

        foreach (var linea in lineasRecibidas)
        {
            decimal factor = hoja.MetodoDistribucion switch
            {
                1 => 0, // Peso — not implemented yet, fallback to value
                2 => 0, // Volumen — not implemented yet, fallback to value
                3 => linea.PrecioUnitario * linea.CantidadRecepcionada, // By FOB value
                4 => linea.CantidadRecepcionada, // By units
                _ => linea.PrecioUnitario * linea.CantidadRecepcionada
            };
            // Fallback to value if weight/volume not available
            if (factor == 0) factor = linea.PrecioUnitario * linea.CantidadRecepcionada;

            factores[linea.OrdenCompraLineaId] = factor;
            totalFactor += factor;
        }

        if (totalFactor == 0)
            return Result<HojaImportacionDto>.Failure("Cannot distribute: total factor is zero.");

        // ?? Create distribution lines + update stock ??
        // Remove previous distribution lines if any (re-liquidation scenario)
        var prevLineas = await _impLineaRepo.FindAsync(l => l.HojaImportacionId == id, ct);
        foreach (var prev in prevLineas)
            await _impLineaRepo.DeleteAsync(prev, ct);

        foreach (var ocLinea in lineasRecibidas)
        {
            var factor = factores[ocLinea.OrdenCompraLineaId];
            var proporcion = factor / totalFactor;
            var gastoAsignado = totalGastos * proporcion;
            var costoFobTotal = ocLinea.PrecioUnitario * ocLinea.CantidadRecepcionada;
            var costoLandedTotal = costoFobTotal + gastoAsignado;
            var costoLandedUnit = ocLinea.CantidadRecepcionada > 0
                ? costoLandedTotal / ocLinea.CantidadRecepcionada
                : 0;

            var impLinea = new ImportacionLinea
            {
                HojaImportacionId = id,
                OrdenCompraLineaId = ocLinea.OrdenCompraLineaId,
                CostoFobUnitario = ocLinea.PrecioUnitario,
                CostoFobTotal = costoFobTotal,
                FactorDistribucion = proporcion,
                GastoAsignado = gastoAsignado,
                CostoLandedUnitario = costoLandedUnit,
                CostoLandedTotal = costoLandedTotal,
                Activo = true
            };
            await _impLineaRepo.AddAsync(impLinea, ct);

            // ?? Update stock average cost with landed cost adjustment ??
            var stocks = await _stockRepo.FindAsync(
                s => s.EmpresaId == empresaId.Value
                  && s.CompanyProductId == ocLinea.CompanyProductId
                  && s.AlmacenId == oc.AlmacenDestinoId, ct);

            var stock = stocks.FirstOrDefault();
            if (stock is not null && stock.CurrentStock > 0)
            {
                // Adjust WAC: add the import expense portion
                // New WAC = (current stock value + gastoAsignado) / current stock qty
                var currentValue = stock.CurrentStock * stock.AverageCost;
                stock.AverageCost = (currentValue + gastoAsignado) / stock.CurrentStock;
                stock.LastUpdated = DateTime.UtcNow;
                await _stockRepo.UpdateAsync(stock, ct);
            }

            // Register adjustment movement for audit trail
            var movNumber = await GenerarNumeroMovimientoAsync(empresaId.Value, hoja.Fecha, ct);
            var mov = new MovimientoInventario
            {
                EmpresaId = empresaId.Value,
                Number = movNumber,
                MovementType = "Adjustment",
                MovementDate = hoja.Fecha,
                CompanyProductId = ocLinea.CompanyProductId,
                WarehouseId = oc.AlmacenDestinoId,
                Quantity = 0, // No stock change, only cost adjustment
                UnitCost = costoLandedUnit,
                TotalCost = gastoAsignado,
                Reference = $"{hoja.Numero} ? {oc.Numero}",
                Notes = $"Landed Cost adjustment: +{gastoAsignado:N2} ({proporcion:P1}) via {hoja.Numero}",
                Activo = true
            };
            await _movRepo.AddAsync(mov, ct);
        }

        hoja.TotalGastos = totalGastos;
        hoja.Liquidada = true;
        await _hojaRepo.UpdateAsync(hoja, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<HojaImportacionDto>.Success(await BuildDto(hoja, ct));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No active company.");

        var hoja = await _hojaRepo.GetByIdAsync(id, ct);
        if (hoja is null || hoja.EmpresaId != empresaId.Value)
            return Result<bool>.Failure("Import sheet not found.");
        if (hoja.Liquidada)
            return Result<bool>.Failure("Cannot delete a settled import sheet.");

        hoja.Activo = false;
        await _hojaRepo.UpdateAsync(hoja, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    // ?? Private helpers ??

    private async Task<string> GenerarNumeroAsync(int empresaId, CancellationToken ct)
    {
        var numeraciones = await _numRepo.FindAsync(
            n => n.EmpresaId == empresaId && n.TipoDocumento == "IMP", ct);
        var num = numeraciones.FirstOrDefault();
        if (num is not null)
        {
            var numero = num.GenerarSiguiente();
            await _numRepo.UpdateAsync(num, ct);
            return numero;
        }
        var existentes = await _hojaRepo.FindAsync(h => h.EmpresaId == empresaId, ct);
        return $"IMP-{(existentes.Count + 1):D6}";
    }

    private async Task<string> GenerarNumeroMovimientoAsync(int empresaId, DateTime fecha, CancellationToken ct)
    {
        var movimientos = await _movRepo.FindAsync(m => m.EmpresaId == empresaId, ct);
        return $"MOV-{fecha.Year}-{(movimientos.Count + 1):D5}";
    }

    private static string MetodoNombre(byte metodo) => metodo switch
    {
        1 => "Peso",
        2 => "Volumen",
        3 => "Valor FOB",
        4 => "Unidades",
        _ => "Valor FOB"
    };

    private async Task<HojaImportacionDto> BuildDto(HojaImportacion hoja, CancellationToken ct)
    {
        var oc = await _ocRepo.GetByIdAsync(hoja.OrdenCompraId, ct);
        var gastos = await _gastoRepo.FindAsync(g => g.HojaImportacionId == hoja.HojaImportacionId, ct);
        var impLineas = await _impLineaRepo.FindAsync(l => l.HojaImportacionId == hoja.HojaImportacionId, ct);
        var ocLineaIds = impLineas.Select(l => l.OrdenCompraLineaId).Distinct().ToList();
        var ocLineas = ocLineaIds.Count > 0
            ? await _ocLineaRepo.FindAsync(l => ocLineaIds.Contains(l.OrdenCompraLineaId), ct)
            : new List<OrdenCompraLinea>();
        var cpIds = ocLineas.Select(l => l.CompanyProductId).Distinct().ToList();
        var cps = cpIds.Count > 0
            ? await _cpRepo.FindAsync(p => cpIds.Contains(p.CompanyProductId), ct)
            : new List<CompanyProduct>();

        var ocLineaMap = ocLineas.ToDictionary(l => l.OrdenCompraLineaId);
        var cpMap = cps.ToDictionary(p => p.CompanyProductId, p => p.Sku);

        return new HojaImportacionDto(
            hoja.HojaImportacionId,
            hoja.EmpresaId,
            hoja.Numero,
            hoja.OrdenCompraId,
            oc?.Numero ?? "—",
            hoja.Fecha,
            hoja.ReferenciaAduanera,
            hoja.Observaciones,
            hoja.MetodoDistribucion,
            MetodoNombre(hoja.MetodoDistribucion),
            hoja.TotalGastos,
            hoja.Liquidada,
            gastos.Select(g => new GastoImportacionDto(
                g.GastoImportacionId, g.TipoGasto, g.Descripcion,
                g.Monto, g.MonedaId, g.TasaCambio, g.MontoBase, g.Referencia
            )).ToList(),
            impLineas.Select(l =>
            {
                var ocl = ocLineaMap.GetValueOrDefault(l.OrdenCompraLineaId);
                return new ImportacionLineaDto(
                    l.ImportacionLineaId,
                    l.OrdenCompraLineaId,
                    ocl?.NumeroLinea ?? 0,
                    cpMap.GetValueOrDefault(ocl?.CompanyProductId ?? 0, "—"),
                    ocl?.Descripcion ?? "—",
                    ocl?.CantidadRecepcionada ?? 0,
                    l.CostoFobUnitario, l.CostoFobTotal,
                    l.FactorDistribucion, l.GastoAsignado,
                    l.CostoLandedUnitario, l.CostoLandedTotal);
            }).ToList());
    }
}
