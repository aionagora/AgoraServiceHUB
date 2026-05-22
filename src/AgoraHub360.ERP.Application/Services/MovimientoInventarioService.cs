namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.INV;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Inventario;

public class MovimientoInventarioService : IMovimientoInventarioService
{
    private readonly IRepository<MovimientoInventario> _movimientoRepo;
    private readonly IRepository<StockProducto> _stockRepo;
    private readonly IRepository<CompanyProduct> _companyProductRepo;
    private readonly IRepository<Almacen> _almacenRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public MovimientoInventarioService(
        IRepository<MovimientoInventario> movimientoRepo,
        IRepository<StockProducto> stockRepo,
        IRepository<CompanyProduct> companyProductRepo,
        IRepository<Almacen> almacenRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _movimientoRepo = movimientoRepo;
        _stockRepo = stockRepo;
        _companyProductRepo = companyProductRepo;
        _almacenRepo = almacenRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<MovimientoInventarioDto>>> GetAllAsync(
        int? productoId = null,
        int? almacenId = null,
        string? tipoMovimiento = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<MovimientoInventarioDto>>.Failure("No active company.");

        var movimientos = await _movimientoRepo.FindAsync(
            m => m.EmpresaId == empresaId.Value
                 && (tipoMovimiento == null || m.MovementType == tipoMovimiento)
                 && (!fechaDesde.HasValue || m.MovementDate >= fechaDesde.Value)
                 && (!fechaHasta.HasValue || m.MovementDate <= fechaHasta.Value.AddDays(1).AddSeconds(-1)),
            ct);

        var companyProducts = await _companyProductRepo.FindAsync(p => p.EmpresaId == empresaId.Value, ct);
        var almacenes = await _almacenRepo.FindAsync(a => a.EmpresaId == empresaId.Value, ct);

        var cpMap = companyProducts.ToDictionary(p => p.CompanyProductId, p => p.Sku);
        var almMap = almacenes.ToDictionary(a => a.Id, a => a.Nombre);

        var dtos = movimientos
            .OrderByDescending(m => m.MovementDate)
            .ThenByDescending(m => m.Id)
            .Select(m => MapToDto(m, cpMap, almMap))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<MovimientoInventarioDto>>.Success(dtos);
    }

    public async Task<Result<MovimientoInventarioDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<MovimientoInventarioDto>.Failure("No active company.");

        var movimiento = await _movimientoRepo.GetByIdAsync(id, ct);
        if (movimiento is null)
            return Result<MovimientoInventarioDto>.Failure($"Movement {id} not found.");
        if (movimiento.EmpresaId != empresaId.Value)
            return Result<MovimientoInventarioDto>.Failure("Access denied.");

        var companyProducts = await _companyProductRepo.FindAsync(p => p.EmpresaId == empresaId.Value, ct);
        var almacenes = await _almacenRepo.FindAsync(a => a.EmpresaId == empresaId.Value, ct);

        return Result<MovimientoInventarioDto>.Success(
            MapToDto(movimiento,
                companyProducts.ToDictionary(p => p.CompanyProductId, p => p.Sku),
                almacenes.ToDictionary(a => a.Id, a => a.Nombre)));
    }

    public async Task<Result<MovimientoInventarioDto>> CreateAsync(CreateMovimientoInventarioDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<MovimientoInventarioDto>.Failure("No active company.");

        Console.WriteLine($"[TEMP-LOG] MovimientoInventario.CreateAsync EmpresaId JWT: {empresaId.Value}");
        Console.WriteLine($"[TEMP-LOG] MovimientoInventario.CreateAsync CompanyProductId recibido: {dto.CompanyProductId}");
        Console.WriteLine($"[TEMP-LOG] MovimientoInventario.CreateAsync ProductId en DTO: N/A (el DTO usa CompanyProductId)");
        Console.WriteLine($"[TEMP-LOG] MovimientoInventario.CreateAsync WarehouseId recibido: {dto.WarehouseId}");
        Console.WriteLine($"[TEMP-LOG] MovimientoInventario.CreateAsync MovementType: {dto.MovementType}");

        // Validate CompanyProduct
        var companyProduct = await _companyProductRepo.GetByIdAsync(dto.CompanyProductId, ct);
        Console.WriteLine(companyProduct is null
            ? "[TEMP-LOG] MovimientoInventario.CreateAsync resultado búsqueda CompanyProduct: NO ENCONTRADO"
            : $"[TEMP-LOG] MovimientoInventario.CreateAsync resultado búsqueda CompanyProduct: ENCONTRADO CompanyProductId={companyProduct.CompanyProductId}, ProductId={companyProduct.ProductId}, EmpresaId={companyProduct.EmpresaId}");
        if (companyProduct is null || companyProduct.EmpresaId != empresaId.Value)
            return Result<MovimientoInventarioDto>.Failure("Company product not found or does not belong to your company.");

        // Validate source warehouse
        var almacen = await _almacenRepo.GetByIdAsync(dto.WarehouseId, ct);
        Console.WriteLine(almacen is null
            ? "[TEMP-LOG] MovimientoInventario.CreateAsync resultado búsqueda Warehouse: NO ENCONTRADO"
            : $"[TEMP-LOG] MovimientoInventario.CreateAsync resultado búsqueda Warehouse: ENCONTRADO WarehouseId={almacen.Id}, EmpresaId={almacen.EmpresaId}");
        if (almacen is null || almacen.EmpresaId != empresaId.Value)
            return Result<MovimientoInventarioDto>.Failure("Warehouse not found or does not belong to your company.");

        // Validate destination warehouse (Transfers only)
        Almacen? almacenDestino = null;
        if (dto.MovementType == "Transfer")
        {
            if (!dto.DestinationWarehouseId.HasValue)
                return Result<MovimientoInventarioDto>.Failure("Destination warehouse is required for transfers.");
            if (dto.DestinationWarehouseId == dto.WarehouseId)
                return Result<MovimientoInventarioDto>.Failure("Destination warehouse must differ from source.");
            almacenDestino = await _almacenRepo.GetByIdAsync(dto.DestinationWarehouseId.Value, ct);
            Console.WriteLine(almacenDestino is null
                ? "[TEMP-LOG] MovimientoInventario.CreateAsync resultado búsqueda DestinationWarehouse: NO ENCONTRADO"
                : $"[TEMP-LOG] MovimientoInventario.CreateAsync resultado búsqueda DestinationWarehouse: ENCONTRADO WarehouseId={almacenDestino.Id}, EmpresaId={almacenDestino.EmpresaId}");
            if (almacenDestino is null || almacenDestino.EmpresaId != empresaId.Value)
                return Result<MovimientoInventarioDto>.Failure("Destination warehouse not found or does not belong to your company.");
        }

        // Get or create source stock
        var stockOrigen = await GetOrCreateStock(empresaId.Value, dto.CompanyProductId, dto.WarehouseId, ct);

        // Validate sufficient stock for Issue/Transfer
        if (dto.MovementType is "Issue" or "Transfer")
        {
            if (stockOrigen.CurrentStock < dto.Quantity)
                return Result<MovimientoInventarioDto>.Failure(
                    $"Insufficient stock. Available: {stockOrigen.CurrentStock:N4} | Requested: {dto.Quantity:N4}");
        }

        // Calculate unit cost
        decimal unitCost = dto.MovementType switch
        {
            "Receipt" => dto.UnitCost > 0 ? dto.UnitCost : stockOrigen.AverageCost,
            "Issue" => stockOrigen.AverageCost,
            "Transfer" => stockOrigen.AverageCost,
            "Adjustment" => dto.UnitCost > 0 ? dto.UnitCost : stockOrigen.AverageCost,
            _ => dto.UnitCost
        };

        decimal totalCost = unitCost * dto.Quantity;

        // Generate movement number
        var totalMovs = await _movimientoRepo.FindAsync(m => m.EmpresaId == empresaId.Value, ct);
        string number = $"MOV-{dto.MovementDate.Year}-{(totalMovs.Count + 1):D5}";

        var movimiento = new MovimientoInventario
        {
            EmpresaId = empresaId.Value,
            Number = number,
            MovementType = dto.MovementType,
            MovementDate = dto.MovementDate,
            CompanyProductId = dto.CompanyProductId,
            WarehouseId = dto.WarehouseId,
            DestinationWarehouseId = dto.DestinationWarehouseId,
            Quantity = dto.Quantity,
            UnitCost = unitCost,
            TotalCost = totalCost,
            Reference = dto.Reference,
            Notes = dto.Notes,
            Activo = true
        };

        await _movimientoRepo.AddAsync(movimiento, ct);

        // Update stock
        switch (dto.MovementType)
        {
            case "Receipt":
                UpdateAverageCost(stockOrigen, dto.Quantity, unitCost);
                stockOrigen.CurrentStock += dto.Quantity;
                break;
            case "Issue":
                stockOrigen.CurrentStock -= dto.Quantity;
                break;
            case "Transfer":
                stockOrigen.CurrentStock -= dto.Quantity;
                var stockDestino = await GetOrCreateStock(empresaId.Value, dto.CompanyProductId, dto.DestinationWarehouseId!.Value, ct);
                UpdateAverageCost(stockDestino, dto.Quantity, unitCost);
                stockDestino.CurrentStock += dto.Quantity;
                stockDestino.LastUpdated = DateTime.UtcNow;
                if (stockDestino.Id != 0)
                    await _stockRepo.UpdateAsync(stockDestino, ct);
                break;
            case "Adjustment":
                stockOrigen.CurrentStock += dto.Quantity;
                if (dto.Quantity > 0 && unitCost > 0)
                    UpdateAverageCost(stockOrigen, dto.Quantity, unitCost);
                break;
        }

        stockOrigen.LastUpdated = DateTime.UtcNow;
        if (stockOrigen.Id != 0)
            await _stockRepo.UpdateAsync(stockOrigen, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var cpMap = new Dictionary<long, string> { { companyProduct.CompanyProductId, companyProduct.Sku } };
        var almMap = new Dictionary<int, string> { { almacen.Id, almacen.Nombre } };
        if (almacenDestino is not null) almMap[almacenDestino.Id] = almacenDestino.Nombre;

        return Result<MovimientoInventarioDto>.Success(MapToDto(movimiento, cpMap, almMap));
    }

    public async Task<Result<KardexDto>> GetKardexAsync(
        int productoId,
        int? almacenId = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<KardexDto>.Failure("No active company.");

        var companyProduct = await _companyProductRepo.GetByIdAsync((long)productoId, ct);
        if (companyProduct is null || companyProduct.EmpresaId != empresaId.Value)
            return Result<KardexDto>.Failure("Company product not found.");

        var movimientos = await _movimientoRepo.FindAsync(
            m => m.EmpresaId == empresaId.Value
                 && m.CompanyProductId == companyProduct.CompanyProductId
                 && (!almacenId.HasValue || m.WarehouseId == almacenId.Value || m.DestinationWarehouseId == almacenId.Value)
                 && (!fechaDesde.HasValue || m.MovementDate >= fechaDesde.Value)
                 && (!fechaHasta.HasValue || m.MovementDate <= fechaHasta.Value.AddDays(1).AddSeconds(-1)),
            ct);

        var almacenes = await _almacenRepo.FindAsync(a => a.EmpresaId == empresaId.Value, ct);
        var almMap = almacenes.ToDictionary(a => a.Id, a => a.Nombre);

        decimal balanceUnits = 0;
        decimal averageCost = 0;
        decimal balanceValue = 0;

        var items = movimientos
            .OrderBy(m => m.MovementDate)
            .ThenBy(m => m.Id)
            .Select(m =>
            {
                decimal inQty = 0, outQty = 0;

                if (m.MovementType is "Receipt" or "Adjustment")
                {
                    inQty = m.Quantity;
                    if (balanceUnits + inQty > 0)
                        averageCost = (balanceValue + m.TotalCost) / (balanceUnits + inQty);
                    balanceUnits += inQty;
                }
                else if (m.MovementType == "Issue")
                {
                    outQty = m.Quantity;
                    balanceUnits -= outQty;
                }
                else if (m.MovementType == "Transfer")
                {
                    if (almacenId.HasValue)
                    {
                        if (m.WarehouseId == almacenId.Value)
                        {
                            outQty = m.Quantity;
                            balanceUnits -= outQty;
                        }
                        else if (m.DestinationWarehouseId == almacenId.Value)
                        {
                            inQty = m.Quantity;
                            if (balanceUnits + inQty > 0)
                                averageCost = (balanceValue + m.TotalCost) / (balanceUnits + inQty);
                            balanceUnits += inQty;
                        }
                    }
                    else
                    {
                        outQty = m.Quantity;
                        inQty = m.Quantity;
                    }
                }

                balanceValue = balanceUnits * averageCost;

                return new KardexItemDto
                {
                    MovementId = m.Id,
                    Number = m.Number,
                    MovementDate = m.MovementDate,
                    MovementType = m.MovementType,
                    Reference = m.Reference,
                    In = inQty,
                    Out = outQty,
                    UnitCost = m.UnitCost,
                    TotalCost = m.TotalCost,
                    BalanceUnits = balanceUnits,
                    AverageCost = averageCost,
                    BalanceValue = balanceValue,
                    Notes = m.Notes
                };
            })
            .ToList();

        var stocks = await _stockRepo.FindAsync(
            s => s.EmpresaId == empresaId.Value
                 && s.CompanyProductId == companyProduct.CompanyProductId
                 && (!almacenId.HasValue || s.AlmacenId == almacenId.Value),
            ct);

        decimal stockActual = stocks.Sum(s => s.CurrentStock);
        decimal costoPromActual = stocks.Any() ? stocks.Average(s => s.AverageCost) : 0;

        string? almacenNombre = almacenId.HasValue && almMap.TryGetValue(almacenId.Value, out var an) ? an : null;

        return Result<KardexDto>.Success(new KardexDto
        {
            CompanyProductId = companyProduct.CompanyProductId,
            ProductSku = companyProduct.Sku,
            ProductName = companyProduct.Sku,
            WarehouseId = almacenId,
            WarehouseName = almacenNombre,
            CurrentStock = stockActual,
            AverageCost = costoPromActual,
            Movements = items
        });
    }

    public async Task<Result<IReadOnlyList<StockProductoDto>>> GetStockAsync(
        int? almacenId = null,
        CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<StockProductoDto>>.Failure("No active company.");

        var stocks = await _stockRepo.FindAsync(
            s => s.EmpresaId == empresaId.Value
                 && (!almacenId.HasValue || s.AlmacenId == almacenId.Value),
            ct);

        var companyProducts = await _companyProductRepo.FindAsync(p => p.EmpresaId == empresaId.Value, ct);
        var almacenes = await _almacenRepo.FindAsync(a => a.EmpresaId == empresaId.Value, ct);

        var cpMap = companyProducts.ToDictionary(p => p.CompanyProductId, p => p.Sku);
        var almMap = almacenes.ToDictionary(a => a.Id, a => a.Nombre);

        var dtos = stocks.Select(s => new StockProductoDto
        {
            Id = s.Id,
            CompanyProductId = s.CompanyProductId,
            ProductSku = cpMap.TryGetValue(s.CompanyProductId, out var sku) ? sku : $"#{s.CompanyProductId}",
            ProductName = cpMap.TryGetValue(s.CompanyProductId, out var name) ? name : $"Product #{s.CompanyProductId}",
            WarehouseId = s.AlmacenId,
            WarehouseName = almMap.TryGetValue(s.AlmacenId, out var wn) ? wn : $"Warehouse #{s.AlmacenId}",
            CurrentStock = s.CurrentStock,
            AverageCost = s.AverageCost,
            LastUpdated = s.LastUpdated
        }).ToList().AsReadOnly();

        return Result<IReadOnlyList<StockProductoDto>>.Success(dtos);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task<StockProducto> GetOrCreateStock(
        int empresaId, long companyProductId, int almacenId, CancellationToken ct)
    {
        var stocks = await _stockRepo.FindAsync(
            s => s.EmpresaId == empresaId
              && s.CompanyProductId == companyProductId
              && s.AlmacenId == almacenId, ct);

        if (stocks.Any())
            return stocks.First();

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
        return nuevo;
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

    private static MovimientoInventarioDto MapToDto(
        MovimientoInventario m,
        Dictionary<long, string> cpMap,
        Dictionary<int, string> almMap) => new()
    {
        Id = m.Id,
        Number = m.Number,
        MovementType = m.MovementType,
        MovementDate = m.MovementDate,
        CompanyProductId = m.CompanyProductId,
        ProductName = cpMap.TryGetValue(m.CompanyProductId, out var n) ? n : $"#{m.CompanyProductId}",
        ProductSku = cpMap.TryGetValue(m.CompanyProductId, out var s) ? s : string.Empty,
        WarehouseId = m.WarehouseId,
        WarehouseName = almMap.TryGetValue(m.WarehouseId, out var a) ? a : $"Warehouse #{m.WarehouseId}",
        DestinationWarehouseId = m.DestinationWarehouseId,
        DestinationWarehouseName = m.DestinationWarehouseId.HasValue && almMap.TryGetValue(m.DestinationWarehouseId.Value, out var da) ? da : null,
        Quantity = m.Quantity,
        UnitCost = m.UnitCost,
        TotalCost = m.TotalCost,
        Reference = m.Reference,
        Notes = m.Notes,
        Activo = m.Activo,
        EmpresaId = m.EmpresaId,
        FechaCreacion = m.FechaCreacion,
        CreadoPor = m.CreadoPor
    };
}
