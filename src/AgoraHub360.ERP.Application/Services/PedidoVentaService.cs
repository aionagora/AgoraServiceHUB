using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.INV;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Ventas;
using AgoraHub360.ERP.Shared.DTOs;
using DomainResult = AgoraHub360.ERP.Domain.Common.Result;

namespace AgoraHub360.ERP.Application.Services;

public class PedidoVentaService : IPedidoVentaService
{
    private readonly IRepository<PedidoVenta> _pedidoRepo;
    private readonly IRepository<PedidoVentaDetalle> _pedidoDetalleRepo;
    private readonly IRepository<StockProducto> _stockProductoRepo;
    private readonly IRepository<MovimientoInventario> _movimientoInventarioRepo;
    private readonly IRepository<CompanyProduct> _companyProductRepo;
    private readonly IRepository<Cliente> _clienteRepo;
    private readonly IRepository<ClienteSucursal> _clienteSucursalRepo;
    private readonly IRepository<Sucursal> _sucursalRepo;
    private readonly IRepository<Almacen> _almacenRepo;
    // Asumimos un servicio o repositorio de inventario para la validación de stock
    // private readonly IInventarioService _inventarioService;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public PedidoVentaService(
        IRepository<PedidoVenta> pedidoRepo,
        IRepository<PedidoVentaDetalle> pedidoDetalleRepo,
        IRepository<StockProducto> stockProductoRepo,
        IRepository<MovimientoInventario> movimientoInventarioRepo,
        IRepository<CompanyProduct> companyProductRepo,
        IRepository<Cliente> clienteRepo,
        IRepository<ClienteSucursal> clienteSucursalRepo,
        IRepository<Sucursal> sucursalRepo,
        IRepository<Almacen> almacenRepo,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _pedidoRepo = pedidoRepo;
        _pedidoDetalleRepo = pedidoDetalleRepo;
        _stockProductoRepo = stockProductoRepo;
        _movimientoInventarioRepo = movimientoInventarioRepo;
        _companyProductRepo = companyProductRepo;
        _clienteRepo = clienteRepo;
        _clienteSucursalRepo = clienteSucursalRepo;
        _sucursalRepo = sucursalRepo;
        _almacenRepo = almacenRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PedidoVentaDto>>> GetAllAsync(CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<IReadOnlyList<PedidoVentaDto>>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var pedidos = await _pedidoRepo.FindAsync(
            x => x.EmpresaId == empresaId && x.Activo,
            ct);

        var data = pedidos
            .OrderByDescending(x => x.FechaEmision)
            .ThenByDescending(x => x.Id)
            .Select(x => new PedidoVentaDto
            {
                Id = x.Id,
                Numero = x.Numero,
                FechaEmision = x.FechaEmision,
                FechaEntregaEsperada = x.FechaEntregaEsperada,
                SucursalId = x.SucursalId,
                AlmacenId = x.AlmacenId,
                VendedorId = x.VendedorId,
                ClienteId = x.ClienteId,
                ClienteSucursalId = x.ClienteSucursalId,
                Prioridad = x.Prioridad,
                Estado = x.Estado,
                Subtotal = x.Subtotal,
                Impuestos = x.Impuestos,
                Total = x.Total,
                Observaciones = x.Observaciones
            })
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<PedidoVentaDto>>.Success(data);
    }

    public async Task<Result<PaginatedResultDto<PedidoVentaDto>>> GetPagedAsync(
        PedidoVentaFilterDto filter,
        CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<PaginatedResultDto<PedidoVentaDto>>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var pedidos = await _pedidoRepo.FindAsync(
            x => x.EmpresaId == empresaId && x.Activo,
            ct);

        var filtered = pedidos.AsEnumerable();

        // ── Búsqueda general ─────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(filter.Busqueda))
        {
            var term = filter.Busqueda.Trim();
            filtered = filtered.Where(x =>
                x.Numero.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (x.Observaciones != null && x.Observaciones.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        // ── Filtro Nro pedido ────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(filter.NumeroPedido))
        {
            filtered = filtered.Where(x =>
                x.Numero.Contains(filter.NumeroPedido, StringComparison.OrdinalIgnoreCase));
        }

        // ── Filtro Cliente ───────────────────────────────────────────────────
        if (filter.ClienteId.HasValue)
        {
            filtered = filtered.Where(x => x.ClienteId == filter.ClienteId.Value);
        }

        // ── Filtro Estado ────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(filter.EstadoPedido) &&
            !string.Equals(filter.EstadoPedido, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(x =>
                string.Equals(x.Estado, filter.EstadoPedido, StringComparison.OrdinalIgnoreCase));
        }

        // ── Filtro Prioridad ─────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(filter.Prioridad) &&
            !string.Equals(filter.Prioridad, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(x =>
                string.Equals(x.Prioridad, filter.Prioridad, StringComparison.OrdinalIgnoreCase));
        }

        // ── Filtro FechaDesde ────────────────────────────────────────────────
        if (filter.FechaDesde.HasValue)
        {
            filtered = filtered.Where(x => x.FechaEmision.Date >= filter.FechaDesde.Value.Date);
        }

        // ── Filtro FechaHasta ────────────────────────────────────────────────
        if (filter.FechaHasta.HasValue)
        {
            filtered = filtered.Where(x => x.FechaEmision.Date <= filter.FechaHasta.Value.Date);
        }

        // ── Ordenar ──────────────────────────────────────────────────────────
        var sorted = filtered
            .OrderByDescending(x => x.FechaEmision)
            .ThenByDescending(x => x.Id)
            .Select(x => new PedidoVentaDto
            {
                Id = x.Id,
                Numero = x.Numero,
                FechaEmision = x.FechaEmision,
                FechaEntregaEsperada = x.FechaEntregaEsperada,
                SucursalId = x.SucursalId,
                AlmacenId = x.AlmacenId,
                VendedorId = x.VendedorId,
                ClienteId = x.ClienteId,
                ClienteSucursalId = x.ClienteSucursalId,
                Prioridad = x.Prioridad,
                Estado = x.Estado,
                Subtotal = x.Subtotal,
                Impuestos = x.Impuestos,
                Total = x.Total,
                Observaciones = x.Observaciones
            });

        var top = filter.Top;
        if (top > 0)
        {
            sorted = sorted.Take(top);
        }

        var sortedList = sorted.ToList();
        var totalItems = sortedList.Count;

        var pagina = filter.Page ?? filter.Pagina;
        var tamanoPagina = filter.PageSize ?? filter.TamanoPagina;

        var items = sortedList
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();

        var paginatedResult = new PaginatedResultDto<PedidoVentaDto>
        {
            Items = items,
            TotalItems = totalItems,
            Pagina = pagina,
            TamanoPagina = tamanoPagina
        };

        return Result<PaginatedResultDto<PedidoVentaDto>>.Success(paginatedResult);
    }

    public async Task<Result<PedidoVentaDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return Result<PedidoVentaDto>.Success(new PedidoVentaDto());
    }

    public async Task<Result<PedidoVentaDto>> CreateAsync(CreatePedidoVentaDto dto, CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<PedidoVentaDto>.Failure("No se pudo determinar la empresa activa.");

        if (!_currentUser.UserIdInt.HasValue || _currentUser.UserIdInt.Value <= 0)
            return Result<PedidoVentaDto>.Failure("No se pudo determinar el usuario vendedor actual.");

        var tenantId = _currentUser.EmpresaId.Value;
        var vendedorId = _currentUser.UserIdInt.Value;

        Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync EmpresaId JWT: {tenantId}");
        Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync ClienteId: {dto.ClienteId}");
        Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync ClienteSucursalId: {(dto.ClienteSucursalId.HasValue ? dto.ClienteSucursalId.Value : 0)}");
        Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync SucursalDespachoId / SucursalId: {dto.SucursalId}");
        Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync AlmacenId recibido: {dto.AlmacenId}");
        Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync VendedorId JWT: {vendedorId}");

        if (dto.Detalles is null || dto.Detalles.Count == 0)
            return Result<PedidoVentaDto>.Failure("Debe registrar al menos un detalle para el pedido.");

        foreach (var detalle in dto.Detalles.Select((value, index) => new { value, index }))
        {
            Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync detalle {detalle.index + 1} CompanyProductId recibido: {detalle.value.CompanyProductId}");
            Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync detalle {detalle.index + 1} Cantidad: {detalle.value.CantidadSolicitada}");
            Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync detalle {detalle.index + 1} PrecioUnitario: {detalle.value.PrecioUnitario}");
            Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync detalle {detalle.index + 1} Subtotal: {detalle.value.CantidadSolicitada * detalle.value.PrecioUnitario}");

            if (detalle.value.CompanyProductId <= 0)
                return Result<PedidoVentaDto>.Failure($"El detalle #{detalle.index + 1} tiene CompanyProductId inválido.");

            if (detalle.value.CantidadSolicitada <= 0)
                return Result<PedidoVentaDto>.Failure($"El detalle #{detalle.index + 1} debe tener una cantidad mayor a cero.");

            if (detalle.value.PrecioUnitario < 0)
                return Result<PedidoVentaDto>.Failure($"El detalle #{detalle.index + 1} no puede tener precio unitario negativo.");

            var companyProduct = await _companyProductRepo.GetByIdAsync(detalle.value.CompanyProductId, ct);
            if (companyProduct is null)
            {
                Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync detalle {detalle.index + 1} búsqueda CompanyProduct: NO ENCONTRADO");
                return Result<PedidoVentaDto>.Failure($"El CompanyProduct {detalle.value.CompanyProductId} no existe.");
            }
            else
            {
                Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync detalle {detalle.index + 1} búsqueda CompanyProduct: ENCONTRADO CompanyProductId={companyProduct.CompanyProductId}, ProductId={companyProduct.ProductId}, EmpresaId={companyProduct.EmpresaId}");
                Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync detalle {detalle.index + 1} CompanyProduct.EmpresaId: {companyProduct.EmpresaId}");
            }

            if (companyProduct.EmpresaId != tenantId)
                return Result<PedidoVentaDto>.Failure($"El CompanyProduct {detalle.value.CompanyProductId} no pertenece a la empresa activa.");
        }

        // 1. Validar Cliente
        var cliente = await _clienteRepo.GetByIdAsync(dto.ClienteId, ct);
        if (cliente == null || cliente.EmpresaId != tenantId)
            return Result<PedidoVentaDto>.Failure("Cliente inválido o no pertenece a la empresa actual.");

        // Regla: Usuario cliente no puede crear pedidos para otro cliente
        // asumiendo que existan estas propiedades en ICurrentUserService
        // if (_currentUser.EsUsuarioCliente && _currentUser.ClienteId != dto.ClienteId)
        //     return Result<PedidoVentaDto>.Failure("No tiene permisos para crear pedidos a nombre de este cliente.");

        // 2. Validar ClienteSucursal
        if (dto.ClienteSucursalId.HasValue)
        {
            var clienteSucursal = await _clienteSucursalRepo.GetByIdAsync(dto.ClienteSucursalId.Value, ct);
            if (clienteSucursal == null)
                return Result<PedidoVentaDto>.Failure("Sucursal de cliente no encontrada.");
            if (clienteSucursal.ClienteId != dto.ClienteId)
                return Result<PedidoVentaDto>.Failure("La sucursal destino no pertenece al cliente seleccionado.");
            if (clienteSucursal.EmpresaId != tenantId)
                return Result<PedidoVentaDto>.Failure("La sucursal de cliente no pertenece a la empresa actual.");
        }

        // 3. Validar SucursalEmpresa
        var sucursal = await _sucursalRepo.GetByIdAsync(dto.SucursalId, ct);
        if (sucursal == null || sucursal.EmpresaId != tenantId)
            return Result<PedidoVentaDto>.Failure("Sucursal de empresa inválida.");

        // 4. Validar Almacén
        var almacen = await _almacenRepo.GetByIdAsync(dto.AlmacenId, ct);
        if (almacen == null || almacen.EmpresaId != tenantId)
            return Result<PedidoVentaDto>.Failure("Almacén inválido.");

        if (almacen.SucursalId != dto.SucursalId)
            return Result<PedidoVentaDto>.Failure("El almacén no está asociado a la sucursal seleccionada.");

        // 5. Validar Stock (requiere servicio de inventario real)
        // foreach(var det in dto.Detalles)
        // {
        //     var stock = await _inventarioService.GetStockAsync(dto.AlmacenId, det.CompanyProductId, ct);
        //     if(stock < det.CantidadSolicitada)
        //         return Result<PedidoVentaDto>.Failure($"Stock insuficiente para el producto {det.CompanyProductId}.");
        // }

        var entity = new PedidoVenta
        {
            EmpresaId = tenantId,
            Numero = $"PV-{DateTime.Now:yyyyMMddHHmmss}",
            FechaEmision = DateTime.UtcNow,
            FechaEntregaEsperada = dto.FechaEntregaEsperada,
            SucursalId = dto.SucursalId,
            AlmacenId = dto.AlmacenId,
            VendedorId = vendedorId,
            ClienteId = dto.ClienteId,
            ClienteSucursalId = dto.ClienteSucursalId,
            Prioridad = dto.Prioridad,
            Estado = "Borrador",
            Observaciones = dto.Observaciones,
            Detalles = dto.Detalles.Select(d => new PedidoVentaDetalle
            {
                EmpresaId = tenantId,
                CompanyProductId = d.CompanyProductId,
                CantidadSolicitada = d.CantidadSolicitada,
                PrecioUnitario = d.PrecioUnitario,
                Impuestos = d.Impuestos,
                Subtotal = d.CantidadSolicitada * d.PrecioUnitario,
                Total = (d.CantidadSolicitada * d.PrecioUnitario) + d.Impuestos,
                Observaciones = d.Observaciones
            }).ToList()
        };

        entity.Subtotal = entity.Detalles.Sum(x => x.Subtotal);
        entity.Impuestos = entity.Detalles.Sum(x => x.Impuestos);
        entity.Total = entity.Detalles.Sum(x => x.Total);

        Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync Total: {entity.Total}");
        Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync entidad antes SaveChanges: EmpresaId={entity.EmpresaId}, ClienteId={entity.ClienteId}, ClienteSucursalId={(entity.ClienteSucursalId.HasValue ? entity.ClienteSucursalId.Value : 0)}, SucursalId={entity.SucursalId}, AlmacenId={entity.AlmacenId}, VendedorId={entity.VendedorId}, Total={entity.Total}, Detalles={entity.Detalles.Count}");

        try
        {
            await _pedidoRepo.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync SaveChanges Exception: {ex.Message}");
            Console.WriteLine($"[TEMP-LOG] PedidoVentaService.CreateAsync SaveChanges InnerException: {ex.InnerException?.Message}");

            var innerMessage = ex.InnerException?.Message;
            var mensaje = !string.IsNullOrWhiteSpace(innerMessage)
                ? $"No se pudo guardar el pedido. Detalle SQL: {innerMessage}"
                : $"No se pudo guardar el pedido. Detalle: {ex.Message}";

            return Result<PedidoVentaDto>.Failure(mensaje);
        }

        return Result<PedidoVentaDto>.Success(new PedidoVentaDto { Id = entity.Id, Numero = entity.Numero });
    }

    public async Task<Result<PedidoVentaDto>> UpdateAsync(long id, UpdatePedidoVentaDto dto, CancellationToken ct = default)
    {
        var entity = await _pedidoRepo.GetByIdAsync(id, ct);
        if (entity == null)
            return Result<PedidoVentaDto>.Failure("Pedido de venta no encontrado.");

        if (!_currentUser.EmpresaId.HasValue || entity.EmpresaId != _currentUser.EmpresaId.Value)
            return Result<PedidoVentaDto>.Failure("No tiene permisos para modificar este pedido.");

        // Only allow updates in certain states (e.g., Borrador, Confirmado)
        if (entity.Estado == "Despachado" || entity.Estado == "Entregado")
            return Result<PedidoVentaDto>.Failure("No se puede modificar un pedido que ya fue despachado/entregado.");

        entity.FechaEntregaEsperada = dto.FechaEntregaEsperada;
        entity.SucursalId = dto.SucursalId;
        entity.AlmacenId = dto.AlmacenId;
        entity.ClienteId = dto.ClienteId;
        entity.ClienteSucursalId = dto.ClienteSucursalId;
        entity.Prioridad = dto.Prioridad;
        entity.Observaciones = dto.Observaciones;
        entity.Estado = dto.Estado ?? entity.Estado;

        // Replace detalles: remove existing and add new
        entity.Detalles.Clear();
        foreach (var d in dto.Detalles)
        {
            entity.Detalles.Add(new PedidoVentaDetalle
            {
                EmpresaId = entity.EmpresaId,
                CompanyProductId = d.CompanyProductId,
                CantidadSolicitada = d.CantidadSolicitada,
                PrecioUnitario = d.PrecioUnitario,
                Impuestos = d.Impuestos,
                Subtotal = d.CantidadSolicitada * d.PrecioUnitario,
                Total = (d.CantidadSolicitada * d.PrecioUnitario) + d.Impuestos,
                Observaciones = d.Observaciones
            });
        }

        entity.Subtotal = entity.Detalles.Sum(x => x.Subtotal);
        entity.Impuestos = entity.Detalles.Sum(x => x.Impuestos);
        entity.Total = entity.Detalles.Sum(x => x.Total);

        await _pedidoRepo.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<PedidoVentaDto>.Success(new PedidoVentaDto { Id = entity.Id, Numero = entity.Numero });
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _pedidoRepo.GetByIdAsync(id, ct);
        if (entity == null)
            return Result<bool>.Failure("Pedido de venta no encontrado.");

        if (!_currentUser.EmpresaId.HasValue || entity.EmpresaId != _currentUser.EmpresaId.Value)
            return Result<bool>.Failure("No tiene permisos para eliminar este pedido.");

        // Soft-delete by setting Activo = false
        entity.Activo = false;
        await _pedidoRepo.UpdateAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ConfirmAsync(long id, CancellationToken ct = default)
    {
        try
        {
            if (!_currentUser.EmpresaId.HasValue)
                return Result<bool>.Failure("No existe empresa activa en la sesión.");

            var empresaId = _currentUser.EmpresaId.Value;
            Console.WriteLine($"[TEMP-LOG] ConfirmAsync pedidoId: {id}");
            Console.WriteLine($"[TEMP-LOG] ConfirmAsync EmpresaId JWT: {empresaId}");

            var pedido = await _pedidoRepo.GetByIdAsync(id, ct);
            if (pedido is null)
                return Result<bool>.Failure("Pedido de venta no encontrado.");

            var detalles = await _pedidoDetalleRepo.FindAsync(x => x.PedidoVentaId == id, ct);
            pedido.Detalles = detalles.ToList();

            if (pedido.EmpresaId != empresaId)
                return Result<bool>.Failure("El pedido no pertenece a la empresa activa.");

            if (!string.Equals(pedido.Estado, "Borrador", StringComparison.OrdinalIgnoreCase))
                return Result<bool>.Failure("Solo se pueden confirmar pedidos en estado Borrador.");

            if (pedido.ReservaAplicada || pedido.InventarioDescontado)
                return Result<bool>.Failure("El pedido ya fue confirmado o procesado.");

            var almacen = await _almacenRepo.GetByIdAsync(pedido.AlmacenId, ct);
            if (almacen is null || almacen.EmpresaId != empresaId)
                return Result<bool>.Failure("El almacén no existe o no pertenece a la empresa activa.");

            Console.WriteLine($"[TEMP-LOG] ConfirmAsync AlmacenId: {pedido.AlmacenId}");

            foreach (var detalle in pedido.Detalles)
            {
                if (detalle.CompanyProductId <= 0)
                    return Result<bool>.Failure("El detalle contiene un CompanyProductId inválido.");

                var companyProduct = await _companyProductRepo.GetByIdAsync(detalle.CompanyProductId, ct);
                if (companyProduct is null)
                    return Result<bool>.Failure("El producto del detalle no existe.");

                if (companyProduct.EmpresaId != empresaId)
                    return Result<bool>.Failure("El producto del detalle no pertenece a la empresa activa.");

                if (detalle.CantidadSolicitada <= 0)
                    return Result<bool>.Failure("La cantidad solicitada debe ser mayor a cero.");

                var stock = (await _stockProductoRepo.FindAsync(
                    x => x.EmpresaId == empresaId
                         && x.CompanyProductId == detalle.CompanyProductId
                         && x.AlmacenId == pedido.AlmacenId,
                    ct)).FirstOrDefault();

                if (stock is null)
                    return Result<bool>.Failure("No existe stock para el producto en el almacén seleccionado.");

                var disponible = stock.CurrentStock - stock.ReservedStock;
                Console.WriteLine($"[TEMP-LOG] ConfirmAsync linea CompanyProductId={detalle.CompanyProductId}, CurrentStock={stock.CurrentStock}, ReservedStock={stock.ReservedStock}, Disponible={disponible}, CantidadSolicitada={detalle.CantidadSolicitada}");

                if (disponible < detalle.CantidadSolicitada)
                    return Result<bool>.Failure($"Stock disponible insuficiente para el producto {detalle.CompanyProductId}.");

                stock.ReservedStock += detalle.CantidadSolicitada;
                detalle.CantidadReservada = detalle.CantidadSolicitada;
                detalle.CantidadConfirmada = detalle.CantidadSolicitada;

                await _stockProductoRepo.UpdateAsync(stock, ct);
                await _pedidoDetalleRepo.UpdateAsync(detalle, ct);
            }

            pedido.Estado = "Confirmado";
            pedido.ReservaAplicada = true;
            pedido.FechaConfirmacion = DateTime.UtcNow;

            await _pedidoRepo.UpdateAsync(pedido, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"No se pudo confirmar el pedido. {ex.Message}");
        }
    }

    public async Task<Result<bool>> DispatchAsync(long id, CancellationToken ct = default)
    {
        try
        {
            if (!_currentUser.EmpresaId.HasValue)
                return Result<bool>.Failure("No existe empresa activa en la sesión.");

            var empresaId = _currentUser.EmpresaId.Value;
            Console.WriteLine($"[TEMP-LOG] DispatchAsync pedidoId: {id}");
            Console.WriteLine($"[TEMP-LOG] DispatchAsync EmpresaId JWT: {empresaId}");

            var pedido = await _pedidoRepo.GetByIdAsync(id, ct);
            if (pedido is null)
                return Result<bool>.Failure("Pedido de venta no encontrado.");

            var detalles = await _pedidoDetalleRepo.FindAsync(x => x.PedidoVentaId == id, ct);
            pedido.Detalles = detalles.ToList();

            if (pedido.EmpresaId != empresaId)
                return Result<bool>.Failure("El pedido no pertenece a la empresa activa.");

            if (!string.Equals(pedido.Estado, "Confirmado", StringComparison.OrdinalIgnoreCase))
                return Result<bool>.Failure("Solo se pueden despachar pedidos en estado Confirmado.");

            if (!pedido.ReservaAplicada || pedido.InventarioDescontado)
                return Result<bool>.Failure("El pedido ya fue despachado o no tiene reserva aplicada.");

            var almacen = await _almacenRepo.GetByIdAsync(pedido.AlmacenId, ct);
            if (almacen is null || almacen.EmpresaId != empresaId)
                return Result<bool>.Failure("El almacén no existe o no pertenece a la empresa activa.");

            Console.WriteLine($"[TEMP-LOG] DispatchAsync AlmacenId: {pedido.AlmacenId}");

            var now = DateTime.UtcNow;
            var lineaIndex = 1;
            foreach (var detalle in pedido.Detalles)
            {
                if (detalle.CompanyProductId <= 0)
                    return Result<bool>.Failure("El detalle contiene un CompanyProductId inválido.");

                var companyProduct = await _companyProductRepo.GetByIdAsync(detalle.CompanyProductId, ct);
                if (companyProduct is null)
                    return Result<bool>.Failure("El producto del detalle no existe.");

                if (companyProduct.EmpresaId != empresaId)
                    return Result<bool>.Failure("El producto del detalle no pertenece a la empresa activa.");

                if (detalle.CantidadReservada <= 0)
                    return Result<bool>.Failure($"La línea del producto {detalle.CompanyProductId} no tiene cantidad reservada válida.");

                var stock = (await _stockProductoRepo.FindAsync(
                    x => x.EmpresaId == empresaId
                         && x.CompanyProductId == detalle.CompanyProductId
                         && x.AlmacenId == pedido.AlmacenId,
                    ct)).FirstOrDefault();

                if (stock is null)
                    return Result<bool>.Failure("No existe stock para el producto en el almacén seleccionado.");

                if (stock.ReservedStock < detalle.CantidadReservada || stock.CurrentStock < detalle.CantidadReservada)
                    return Result<bool>.Failure($"Stock reservado/físico insuficiente para el producto {detalle.CompanyProductId}.");

                stock.CurrentStock -= detalle.CantidadReservada;
                stock.ReservedStock -= detalle.CantidadReservada;
                detalle.CantidadDespachada = detalle.CantidadReservada;

                Console.WriteLine($"[TEMP-LOG] DispatchAsync linea CompanyProductId={detalle.CompanyProductId}, CurrentStock={stock.CurrentStock}, ReservedStock={stock.ReservedStock}, CantidadReservada={detalle.CantidadReservada}, CantidadDespachada={detalle.CantidadDespachada}");

                var movimiento = new MovimientoInventario
                {
                    EmpresaId = empresaId,
                    Number = $"MOV-DSP-{id}-{lineaIndex}",
                    MovementType = "Issue",
                    MovementDate = now,
                    CompanyProductId = detalle.CompanyProductId,
                    WarehouseId = pedido.AlmacenId,
                    Quantity = detalle.CantidadReservada,
                    UnitCost = stock.AverageCost,
                    TotalCost = stock.AverageCost * detalle.CantidadReservada,
                    Reference = pedido.Numero,
                    Notes = "Despacho automático de pedido de venta " + pedido.Numero,
                    Activo = true
                };

                await _movimientoInventarioRepo.AddAsync(movimiento, ct);
                await _stockProductoRepo.UpdateAsync(stock, ct);
                await _pedidoDetalleRepo.UpdateAsync(detalle, ct);
                lineaIndex++;
            }

            pedido.Estado = "Despachado";
            pedido.InventarioDescontado = true;
            pedido.FechaDespacho = now;

            await _pedidoRepo.UpdateAsync(pedido, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"No se pudo despachar el pedido. {ex.Message}");
        }
    }

    public async Task<Result<bool>> CancelAsync(long id, CancellationToken ct = default)
    {
        return Result<bool>.Failure("Pendiente de implementar.");
    }

    public async Task<Result<bool>> MarkDeliveredAsync(long id, CancellationToken ct = default)
    {
        return Result<bool>.Failure("Pendiente de implementar.");
    }
}
