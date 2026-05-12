using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Ventas;

namespace AgoraHub360.ERP.Application.Services;

public class PedidoVentaService : IPedidoVentaService
{
    private readonly IRepository<PedidoVenta> _pedidoRepo;
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
        IRepository<Cliente> clienteRepo,
        IRepository<ClienteSucursal> clienteSucursalRepo,
        IRepository<Sucursal> sucursalRepo,
        IRepository<Almacen> almacenRepo,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _pedidoRepo = pedidoRepo;
        _clienteRepo = clienteRepo;
        _clienteSucursalRepo = clienteSucursalRepo;
        _sucursalRepo = sucursalRepo;
        _almacenRepo = almacenRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<PedidoVentaDto>>> GetAllAsync(CancellationToken ct = default)
    {
        return Result<IReadOnlyList<PedidoVentaDto>>.Success(new List<PedidoVentaDto>());
    }

    public async Task<Result<PedidoVentaDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return Result<PedidoVentaDto>.Success(new PedidoVentaDto());
    }

    public async Task<Result<PedidoVentaDto>> CreateAsync(CreatePedidoVentaDto dto, CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<PedidoVentaDto>.Failure("No se pudo determinar la empresa activa.");

        var tenantId = _currentUser.EmpresaId.Value;

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
            if (clienteSucursal == null || clienteSucursal.ClienteId != dto.ClienteId)
                return Result<PedidoVentaDto>.Failure("La sucursal destino no pertenece al cliente seleccionado.");
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
            // VendedorId = _currentUser.UserId,
            ClienteId = dto.ClienteId,
            ClienteSucursalId = dto.ClienteSucursalId,
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

        await _pedidoRepo.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

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
}
