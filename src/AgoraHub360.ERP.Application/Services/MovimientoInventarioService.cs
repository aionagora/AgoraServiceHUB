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
    private readonly IRepository<Producto> _productoRepo;
    private readonly IRepository<Almacen> _almacenRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public MovimientoInventarioService(
        IRepository<MovimientoInventario> movimientoRepo,
        IRepository<StockProducto> stockRepo,
        IRepository<Producto> productoRepo,
        IRepository<Almacen> almacenRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _movimientoRepo = movimientoRepo;
        _stockRepo = stockRepo;
        _productoRepo = productoRepo;
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
            return Result<IReadOnlyList<MovimientoInventarioDto>>.Failure("No se pudo determinar la empresa activa.");

        var movimientos = await _movimientoRepo.FindAsync(
            m => m.EmpresaId == empresaId.Value
                 && (!productoId.HasValue || m.ProductoId == productoId.Value)
                 && (!almacenId.HasValue || m.AlmacenId == almacenId.Value || m.AlmacenDestinoId == almacenId.Value)
                 && (tipoMovimiento == null || m.TipoMovimiento == tipoMovimiento)
                 && (!fechaDesde.HasValue || m.FechaMovimiento >= fechaDesde.Value)
                 && (!fechaHasta.HasValue || m.FechaMovimiento <= fechaHasta.Value.AddDays(1).AddSeconds(-1)),
            ct);

        var productos = await _productoRepo.FindAsync(p => p.EmpresaId == empresaId.Value, ct);
        var almacenes = await _almacenRepo.FindAsync(a => a.EmpresaId == empresaId.Value, ct);

        var prodMap = productos.ToDictionary(p => p.Id, p => new { p.Nombre, p.Codigo });
        var almMap = almacenes.ToDictionary(a => a.Id, a => a.Nombre);

        var dtos = movimientos
            .OrderByDescending(m => m.FechaMovimiento)
            .ThenByDescending(m => m.Id)
            .Select(m => MapToDto(m, prodMap, almMap))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<MovimientoInventarioDto>>.Success(dtos);
    }

    public async Task<Result<MovimientoInventarioDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<MovimientoInventarioDto>.Failure("No se pudo determinar la empresa activa.");

        var movimiento = await _movimientoRepo.GetByIdAsync(id, ct);
        if (movimiento is null)
            return Result<MovimientoInventarioDto>.Failure($"Movimiento con Id {id} no encontrado.");
        if (movimiento.EmpresaId != empresaId.Value)
            return Result<MovimientoInventarioDto>.Failure("No tiene permisos para acceder a este movimiento.");

        var productos = await _productoRepo.FindAsync(p => p.EmpresaId == empresaId.Value, ct);
        var almacenes = await _almacenRepo.FindAsync(a => a.EmpresaId == empresaId.Value, ct);

        return Result<MovimientoInventarioDto>.Success(
            MapToDto(movimiento,
                productos.ToDictionary(p => p.Id, p => new { p.Nombre, p.Codigo }),
                almacenes.ToDictionary(a => a.Id, a => a.Nombre)));
    }

    public async Task<Result<MovimientoInventarioDto>> CreateAsync(CreateMovimientoInventarioDto dto, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<MovimientoInventarioDto>.Failure("No se pudo determinar la empresa activa.");

        // ── Validar producto
        var producto = await _productoRepo.GetByIdAsync(dto.ProductoId, ct);
        if (producto is null || producto.EmpresaId != empresaId.Value)
            return Result<MovimientoInventarioDto>.Failure("Producto no encontrado o no pertenece a su empresa.");

        if (!producto.ControlStock && dto.TipoMovimiento != "Ajuste")
            return Result<MovimientoInventarioDto>.Failure("Este producto no tiene control de stock habilitado.");

        // ── Validar almacén origen
        var almacen = await _almacenRepo.GetByIdAsync(dto.AlmacenId, ct);
        if (almacen is null || almacen.EmpresaId != empresaId.Value)
            return Result<MovimientoInventarioDto>.Failure("Almacén no encontrado o no pertenece a su empresa.");

        // ── Validar almacén destino (solo Transferencias)
        Almacen? almacenDestino = null;
        if (dto.TipoMovimiento == "Transferencia")
        {
            if (!dto.AlmacenDestinoId.HasValue)
                return Result<MovimientoInventarioDto>.Failure("Para transferencias debe indicar el almacén destino.");
            if (dto.AlmacenDestinoId == dto.AlmacenId)
                return Result<MovimientoInventarioDto>.Failure("El almacén destino debe ser diferente al origen.");
            almacenDestino = await _almacenRepo.GetByIdAsync(dto.AlmacenDestinoId.Value, ct);
            if (almacenDestino is null || almacenDestino.EmpresaId != empresaId.Value)
                return Result<MovimientoInventarioDto>.Failure("Almacén destino no encontrado o no pertenece a su empresa.");
        }

        // ── Obtener o crear saldo de stock origen
        var stockOrigen = await GetOrCreateStock(empresaId.Value, dto.ProductoId, dto.AlmacenId, ct);

        // ── Validar stock suficiente para Salida / Transferencia
        if (dto.TipoMovimiento == "Salida" || dto.TipoMovimiento == "Transferencia")
        {
            if (stockOrigen.StockActual < dto.Cantidad)
                return Result<MovimientoInventarioDto>.Failure(
                    $"Stock insuficiente. Disponible: {stockOrigen.StockActual:N4} | Solicitado: {dto.Cantidad:N4}");
        }

        // ── Calcular costo unitario
        decimal costoUnitario = dto.TipoMovimiento switch
        {
            "Entrada" => dto.CostoUnitario > 0 ? dto.CostoUnitario : producto.CostoBase,
            "Salida" => stockOrigen.CostoPromedio > 0 ? stockOrigen.CostoPromedio : producto.CostoBase,
            "Transferencia" => stockOrigen.CostoPromedio > 0 ? stockOrigen.CostoPromedio : producto.CostoBase,
            "Ajuste" => dto.CostoUnitario > 0 ? dto.CostoUnitario : stockOrigen.CostoPromedio,
            _ => dto.CostoUnitario
        };

        decimal costoTotal = costoUnitario * dto.Cantidad;

        // ── Generar número de movimiento
        var totalMovs = await _movimientoRepo.FindAsync(m => m.EmpresaId == empresaId.Value, ct);
        string numero = $"MOV-{dto.FechaMovimiento.Year}-{(totalMovs.Count + 1):D5}";

        // ── Crear registro del movimiento
        var movimiento = new MovimientoInventario
        {
            EmpresaId = empresaId.Value,
            Numero = numero,
            TipoMovimiento = dto.TipoMovimiento,
            FechaMovimiento = dto.FechaMovimiento,
            ProductoId = dto.ProductoId,
            AlmacenId = dto.AlmacenId,
            AlmacenDestinoId = dto.AlmacenDestinoId,
            Cantidad = dto.Cantidad,
            CostoUnitario = costoUnitario,
            CostoTotal = costoTotal,
            Referencia = dto.Referencia,
            Observaciones = dto.Observaciones,
            Activo = true
        };

        await _movimientoRepo.AddAsync(movimiento, ct);

        // ── Actualizar stock origen
        switch (dto.TipoMovimiento)
        {
            case "Entrada":
                ActualizarCostoPromedio(stockOrigen, dto.Cantidad, costoUnitario);
                stockOrigen.StockActual += dto.Cantidad;
                break;
            case "Salida":
                stockOrigen.StockActual -= dto.Cantidad;
                break;
            case "Transferencia":
                stockOrigen.StockActual -= dto.Cantidad;
                // Actualizar stock destino
                var stockDestino = await GetOrCreateStock(empresaId.Value, dto.ProductoId, dto.AlmacenDestinoId!.Value, ct);
                ActualizarCostoPromedio(stockDestino, dto.Cantidad, costoUnitario);
                stockDestino.StockActual += dto.Cantidad;
                stockDestino.UltimaActualizacion = DateTime.UtcNow;
                await _stockRepo.UpdateAsync(stockDestino, ct);
                break;
            case "Ajuste":
                // El ajuste puede aumentar o disminuir (la cantidad del DTO siempre es positiva,
                // pero el costoUnitario indica si es un ajuste positivo)
                stockOrigen.StockActual += dto.Cantidad; // Ajuste siempre suma; si es negativo el usuario debe usar Salida
                if (dto.Cantidad > 0 && costoUnitario > 0)
                    ActualizarCostoPromedio(stockOrigen, dto.Cantidad, costoUnitario);
                break;
        }

        stockOrigen.UltimaActualizacion = DateTime.UtcNow;
        await _stockRepo.UpdateAsync(stockOrigen, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        var prodMap = new Dictionary<int, dynamic> { { producto.Id, new { producto.Nombre, producto.Codigo } } };
        var almMap = new Dictionary<int, string> { { almacen.Id, almacen.Nombre } };
        if (almacenDestino is not null) almMap[almacenDestino.Id] = almacenDestino.Nombre;

        return Result<MovimientoInventarioDto>.Success(MapToDto(movimiento, prodMap!, almMap));
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
            return Result<KardexDto>.Failure("No se pudo determinar la empresa activa.");

        var producto = await _productoRepo.GetByIdAsync(productoId, ct);
        if (producto is null || producto.EmpresaId != empresaId.Value)
            return Result<KardexDto>.Failure("Producto no encontrado.");

        var movimientos = await _movimientoRepo.FindAsync(
            m => m.EmpresaId == empresaId.Value
                 && m.ProductoId == productoId
                 && (!almacenId.HasValue || m.AlmacenId == almacenId.Value || m.AlmacenDestinoId == almacenId.Value)
                 && (!fechaDesde.HasValue || m.FechaMovimiento >= fechaDesde.Value)
                 && (!fechaHasta.HasValue || m.FechaMovimiento <= fechaHasta.Value.AddDays(1).AddSeconds(-1)),
            ct);

        var almacenes = await _almacenRepo.FindAsync(a => a.EmpresaId == empresaId.Value, ct);
        var almMap = almacenes.ToDictionary(a => a.Id, a => a.Nombre);

        // Calcular kardex con saldo corriente
        decimal saldoUnidades = 0;
        decimal costoPromedio = 0;
        decimal saldoValor = 0;

        var items = movimientos
            .OrderBy(m => m.FechaMovimiento)
            .ThenBy(m => m.Id)
            .Select(m =>
            {
                decimal entrada = 0, salida = 0;

                if (m.TipoMovimiento == "Entrada" || m.TipoMovimiento == "Ajuste")
                {
                    entrada = m.Cantidad;
                    // Recalcular costo promedio ponderado
                    if (saldoUnidades + entrada > 0)
                        costoPromedio = (saldoValor + m.CostoTotal) / (saldoUnidades + entrada);
                    saldoUnidades += entrada;
                }
                else if (m.TipoMovimiento == "Salida")
                {
                    salida = m.Cantidad;
                    saldoUnidades -= salida;
                }
                else if (m.TipoMovimiento == "Transferencia")
                {
                    // Para el kardex filtrado por almacén:
                    if (almacenId.HasValue)
                    {
                        if (m.AlmacenId == almacenId.Value)
                        {
                            salida = m.Cantidad;
                            saldoUnidades -= salida;
                        }
                        else if (m.AlmacenDestinoId == almacenId.Value)
                        {
                            entrada = m.Cantidad;
                            if (saldoUnidades + entrada > 0)
                                costoPromedio = (saldoValor + m.CostoTotal) / (saldoUnidades + entrada);
                            saldoUnidades += entrada;
                        }
                    }
                    else
                    {
                        // Sin filtro de almacén, no afecta el stock total de la empresa
                        salida = m.Cantidad;
                        entrada = m.Cantidad;
                    }
                }

                saldoValor = saldoUnidades * costoPromedio;

                return new KardexItemDto
                {
                    MovimientoId = m.Id,
                    Numero = m.Numero,
                    FechaMovimiento = m.FechaMovimiento,
                    TipoMovimiento = m.TipoMovimiento,
                    Referencia = m.Referencia,
                    Entrada = entrada,
                    Salida = salida,
                    CostoUnitario = m.CostoUnitario,
                    CostoTotal = m.CostoTotal,
                    SaldoUnidades = saldoUnidades,
                    CostoPromedio = costoPromedio,
                    SaldoValor = saldoValor,
                    Observaciones = m.Observaciones
                };
            })
            .ToList();

        // Obtener stock actual de la tabla StockProducto
        var stocks = await _stockRepo.FindAsync(
            s => s.EmpresaId == empresaId.Value
                 && s.ProductoId == productoId
                 && (!almacenId.HasValue || s.AlmacenId == almacenId.Value),
            ct);

        decimal stockActual = stocks.Sum(s => s.StockActual);
        decimal costoPromActual = stocks.Any() ? stocks.Average(s => s.CostoPromedio) : 0;

        string? almacenNombre = almacenId.HasValue && almMap.TryGetValue(almacenId.Value, out var an) ? an : null;

        return Result<KardexDto>.Success(new KardexDto
        {
            ProductoId = productoId,
            ProductoCodigo = producto.Codigo,
            ProductoNombre = producto.Nombre,
            AlmacenId = almacenId,
            AlmacenNombre = almacenNombre,
            StockActual = stockActual,
            CostoPromedio = costoPromActual,
            Movimientos = items
        });
    }

    public async Task<Result<IReadOnlyList<StockProductoDto>>> GetStockAsync(
        int? almacenId = null,
        CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<StockProductoDto>>.Failure("No se pudo determinar la empresa activa.");

        var stocks = await _stockRepo.FindAsync(
            s => s.EmpresaId == empresaId.Value
                 && (!almacenId.HasValue || s.AlmacenId == almacenId.Value),
            ct);

        var productos = await _productoRepo.FindAsync(p => p.EmpresaId == empresaId.Value, ct);
        var almacenes = await _almacenRepo.FindAsync(a => a.EmpresaId == empresaId.Value, ct);

        var prodMap = productos.ToDictionary(p => p.Id, p => new { p.Nombre, p.Codigo });
        var almMap = almacenes.ToDictionary(a => a.Id, a => a.Nombre);

        var dtos = stocks.Select(s => new StockProductoDto
        {
            Id = s.Id,
            ProductoId = s.ProductoId,
            ProductoCodigo = prodMap.TryGetValue(s.ProductoId, out var p) ? p.Codigo : $"#{s.ProductoId}",
            ProductoNombre = prodMap.TryGetValue(s.ProductoId, out var p2) ? p2.Nombre : $"Producto #{s.ProductoId}",
            AlmacenId = s.AlmacenId,
            AlmacenNombre = almMap.TryGetValue(s.AlmacenId, out var a) ? a : $"Almacén #{s.AlmacenId}",
            StockActual = s.StockActual,
            CostoPromedio = s.CostoPromedio,
            UltimaActualizacion = s.UltimaActualizacion
        }).ToList().AsReadOnly();

        return Result<IReadOnlyList<StockProductoDto>>.Success(dtos);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task<StockProducto> GetOrCreateStock(
        int empresaId, int productoId, int almacenId, CancellationToken ct)
    {
        var stocks = await _stockRepo.FindAsync(
            s => s.EmpresaId == empresaId && s.ProductoId == productoId && s.AlmacenId == almacenId, ct);

        if (stocks.Any())
            return stocks.First();

        var nuevo = new StockProducto
        {
            EmpresaId = empresaId,
            ProductoId = productoId,
            AlmacenId = almacenId,
            StockActual = 0,
            CostoPromedio = 0,
            UltimaActualizacion = DateTime.UtcNow,
            Activo = true
        };
        await _stockRepo.AddAsync(nuevo, ct);
        return nuevo;
    }

    private static void ActualizarCostoPromedio(StockProducto stock, decimal cantidadEntrada, decimal costoNuevo)
    {
        if (stock.StockActual <= 0)
        {
            stock.CostoPromedio = costoNuevo;
            return;
        }
        // Costo Promedio Ponderado
        stock.CostoPromedio =
            (stock.StockActual * stock.CostoPromedio + cantidadEntrada * costoNuevo)
            / (stock.StockActual + cantidadEntrada);
    }

    private static MovimientoInventarioDto MapToDto(
        MovimientoInventario m,
        Dictionary<int, dynamic> prodMap,
        Dictionary<int, string> almMap) => new()
    {
        Id = m.Id,
        Numero = m.Numero,
        TipoMovimiento = m.TipoMovimiento,
        FechaMovimiento = m.FechaMovimiento,
        ProductoId = m.ProductoId,
        ProductoNombre = prodMap.TryGetValue(m.ProductoId, out var p) ? (string)p.Nombre : $"Producto #{m.ProductoId}",
        ProductoCodigo = prodMap.TryGetValue(m.ProductoId, out var p2) ? (string)p2.Codigo : string.Empty,
        AlmacenId = m.AlmacenId,
        AlmacenNombre = almMap.TryGetValue(m.AlmacenId, out var a) ? a : $"Almacén #{m.AlmacenId}",
        AlmacenDestinoId = m.AlmacenDestinoId,
        AlmacenDestinoNombre = m.AlmacenDestinoId.HasValue && almMap.TryGetValue(m.AlmacenDestinoId.Value, out var ad) ? ad : null,
        Cantidad = m.Cantidad,
        CostoUnitario = m.CostoUnitario,
        CostoTotal = m.CostoTotal,
        Referencia = m.Referencia,
        Observaciones = m.Observaciones,
        Activo = m.Activo,
        EmpresaId = m.EmpresaId,
        FechaCreacion = m.FechaCreacion,
        CreadoPor = m.CreadoPor
    };
}
