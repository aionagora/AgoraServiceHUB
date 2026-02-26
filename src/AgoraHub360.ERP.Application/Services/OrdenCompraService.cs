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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public OrdenCompraService(
        IRepository<OrdenCompra> ocRepo,
        IRepository<OrdenCompraLinea> lineaRepo,
        IRepository<Proveedor> proveedorRepo,
        IRepository<Almacen> almacenRepo,
        IRepository<CompanyProduct> companyProductRepo,
        IRepository<NumeracionDocumento> numeracionRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _ocRepo = ocRepo;
        _lineaRepo = lineaRepo;
        _proveedorRepo = proveedorRepo;
        _almacenRepo = almacenRepo;
        _companyProductRepo = companyProductRepo;
        _numeracionRepo = numeracionRepo;
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

        // Validate proveedor
        var proveedor = await _proveedorRepo.GetByIdAsync(dto.ProveedorId, ct);
        if (proveedor is null || proveedor.EmpresaId != empresaId.Value)
            return Result<OrdenCompraDto>.Failure("Supplier not found or does not belong to your company.");

        // Validate almacen
        var almacen = await _almacenRepo.GetByIdAsync(dto.AlmacenDestinoId, ct);
        if (almacen is null || almacen.EmpresaId != empresaId.Value)
            return Result<OrdenCompraDto>.Failure("Warehouse not found or does not belong to your company.");

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
            Observaciones = dto.Observaciones,
            ReferenciaExterna = dto.ReferenciaExterna,
            Activo = true
        };

        await _ocRepo.AddAsync(oc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Add lines
        if (dto.Lineas?.Count > 0)
        {
            int lineNum = 1;
            foreach (var lineaDto in dto.Lineas)
            {
                var cp = await _companyProductRepo.GetByIdAsync(lineaDto.CompanyProductId, ct);
                if (cp is null || cp.EmpresaId != empresaId.Value)
                    return Result<OrdenCompraDto>.Failure($"Product {lineaDto.CompanyProductId} not found.");

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
                await _lineaRepo.AddAsync(linea, ct);
            }
        }

        // Recalculate totals
        var lineasGuardadas = await _lineaRepo.FindAsync(l => l.OrdenCompraId == oc.OrdenCompraId, ct);
        oc.Lineas = lineasGuardadas.ToList();
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

        // Validate transitions
        var valid = (oc.Estado, estadoTarget) switch
        {
            (EstadoDocumento.Borrador, EstadoDocumento.Confirmado) => true,
            (EstadoDocumento.Confirmado, EstadoDocumento.Aprobado) => true,
            (EstadoDocumento.Borrador, EstadoDocumento.Anulado) => true,
            (EstadoDocumento.Confirmado, EstadoDocumento.Anulado) => true,
            _ => false
        };

        if (!valid)
            return Result<OrdenCompraDto>.Failure(
                $"Cannot transition from {oc.Estado} to {estadoTarget}.");

        // Validaciones adicionales
        if (estadoTarget == EstadoDocumento.Confirmado)
        {
            var lineas = await _lineaRepo.FindAsync(l => l.OrdenCompraId == id, ct);
            if (lineas.Count == 0)
                return Result<OrdenCompraDto>.Failure("Cannot confirm an order without lines.");
        }

        oc.Estado = estadoTarget;
        await _ocRepo.UpdateAsync(oc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<OrdenCompraDto>.Success(await BuildFullDto(oc, ct));
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

    // ?? Private helpers ??

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
            oc.Observaciones,
            oc.ReferenciaExterna,
            oc.Subtotal,
            oc.Descuento,
            oc.Impuesto,
            oc.Total,
            oc.Activo,
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
