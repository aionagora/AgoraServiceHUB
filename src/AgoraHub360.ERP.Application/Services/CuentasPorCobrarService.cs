using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.CXC;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.CxC;

namespace AgoraHub360.ERP.Application.Services;

public class CuentasPorCobrarService : ICuentasPorCobrarService
{
    private readonly IRepository<CuentaPorCobrar> _cxcRepo;
    private readonly IRepository<FacturaVenta> _facturaRepo;
    private readonly IRepository<VentaPago> _pagoRepo;
    private readonly IRepository<Venta> _ventaRepo;
    private readonly IRepository<Cliente> _clienteRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CuentasPorCobrarService(
        IRepository<CuentaPorCobrar> cxcRepo,
        IRepository<FacturaVenta> facturaRepo,
        IRepository<VentaPago> pagoRepo,
        IRepository<Venta> ventaRepo,
        IRepository<Cliente> clienteRepo,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _cxcRepo = cxcRepo;
        _facturaRepo = facturaRepo;
        _pagoRepo = pagoRepo;
        _ventaRepo = ventaRepo;
        _clienteRepo = clienteRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedResultDto<CuentaPorCobrarResumenDto>>> GetAllAsync(
        CuentaPorCobrarFilterDto? filter = null,
        CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<PaginatedResultDto<CuentaPorCobrarResumenDto>>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var cxcList = await _cxcRepo.FindAsync(c =>
            c.EmpresaId == empresaId &&
            c.Activo,
            ct);

        // Aplicar filtros
        var filteredList = cxcList.AsEnumerable();

        if (filter is not null)
        {
            if (!string.IsNullOrWhiteSpace(filter.NumeroFactura))
                filteredList = filteredList.Where(c => c.NumeroFactura.Contains(filter.NumeroFactura, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(filter.NumeroVenta))
                filteredList = filteredList.Where(c => c.NumeroVenta != null && c.NumeroVenta.Contains(filter.NumeroVenta, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(filter.Busqueda))
            {
                var term = filter.Busqueda;
                filteredList = filteredList.Where(c => 
                    c.NumeroFactura.Contains(term, StringComparison.OrdinalIgnoreCase) || 
                    (c.NumeroVenta != null && c.NumeroVenta.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (c.ClienteNombre != null && c.ClienteNombre.Contains(term, StringComparison.OrdinalIgnoreCase)));
            }

            if (filter.ClienteId.HasValue)
                filteredList = filteredList.Where(c => c.ClienteId == filter.ClienteId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Estado) && !string.Equals(filter.Estado, "Todos", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<EstadoCuentaPorCobrar>(filter.Estado, true, out var estadoFilter))
                {
                    filteredList = filteredList.Where(c => GetDynamicEstado(c) == estadoFilter);
                }
            }

            if (filter.FechaDesde.HasValue)
                filteredList = filteredList.Where(c => c.FechaEmision.Date >= filter.FechaDesde.Value.Date);

            if (filter.FechaHasta.HasValue)
                filteredList = filteredList.Where(c => c.FechaEmision.Date <= filter.FechaHasta.Value.Date);

            if (filter.FechaVencimientoDesde.HasValue)
                filteredList = filteredList.Where(c => c.FechaVencimiento.HasValue && c.FechaVencimiento.Value.Date >= filter.FechaVencimientoDesde.Value.Date);

            if (filter.FechaVencimientoHasta.HasValue)
                filteredList = filteredList.Where(c => c.FechaVencimiento.HasValue && c.FechaVencimiento.Value.Date <= filter.FechaVencimientoHasta.Value.Date);
        }

        var listToDtd = filteredList.ToList();
        var facturaIds = listToDtd.Select(c => c.FacturaVentaId).Distinct().ToList();
        var facturas = await _facturaRepo.FindAsync(f => facturaIds.Contains(f.Id) && f.EmpresaId == empresaId, ct);
        var facturaVentaMap = facturas.ToDictionary(f => f.Id, f => f.VentaId);

        var sorted = listToDtd
            .OrderByDescending(c => c.FechaVencimiento)
            .ThenByDescending(c => c.Id)
            .Select(c => MapToResumen(c, c.FacturaVentaId.HasValue ? facturaVentaMap.GetValueOrDefault(c.FacturaVentaId.Value) : 0));

        // Cargar Top
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

        var paginatedResult = new PaginatedResultDto<CuentaPorCobrarResumenDto>
        {
            Items = items,
            TotalItems = totalItems,
            Pagina = pagina,
            TamanoPagina = tamanoPagina
        };

        return Result<PaginatedResultDto<CuentaPorCobrarResumenDto>>.Success(paginatedResult);
    }

    public async Task<Result<CuentaPorCobrarDetalleDto>> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<CuentaPorCobrarDetalleDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var cxc = (await _cxcRepo.FindAsync(
            c => c.Id == id && c.EmpresaId == empresaId && c.Activo, ct))
            .FirstOrDefault();

        if (cxc is null)
            return Result<CuentaPorCobrarDetalleDto>.Failure("Cuenta por cobrar no encontrada.");

        // Obtener pagos de la venta asociada
        // Si la CxC se generó desde venta directa (sin factura), FacturaVentaId es null
        // y se consulta sin factura — eso es válido.
        FacturaVenta? factura = null;
        if (cxc.FacturaVentaId.HasValue)
        {
            factura = await _facturaRepo.GetByIdAsync(cxc.FacturaVentaId.Value, ct);
            // Si la factura referenciada no existe, no bloqueamos — continuamos sin ella
        }
        // Si no tiene FacturaVentaId, es una CxC originada desde venta directa — válido

        var ventaId = factura?.VentaId ?? cxc.VentaId ?? 0;
        var pagos = await _pagoRepo.FindAsync(
            p => p.VentaId == ventaId
                 && p.EmpresaId == empresaId
                 && p.Activo, ct);

        var dto = new CuentaPorCobrarDetalleDto
        {
            Id = cxc.Id,
            FacturaVentaId = cxc.FacturaVentaId,
            VentaId = cxc.VentaId ?? factura?.VentaId,
            TipoDocumentoOrigen = cxc.TipoDocumentoOrigen,
            NumeroFactura = cxc.NumeroFactura,
            NumeroVenta = cxc.NumeroVenta,
            ClienteId = cxc.ClienteId,
            ClienteNombre = cxc.ClienteNombre,
            ClienteNit = cxc.ClienteNit,
            FechaEmision = cxc.FechaEmision,
            FechaVencimiento = cxc.FechaVencimiento,
            MonedaCodigo = cxc.MonedaCodigo,
            TipoCambio = cxc.TipoCambio,
            TotalFactura = cxc.TotalFactura,
            TotalPagado = cxc.TotalPagado,
            SaldoPendiente = cxc.SaldoPendiente,
            Estado = GetDynamicEstado(cxc).ToString(),
            Pagos = pagos.Select(p => new PagoAplicadoDto
            {
                PagoId = p.Id,
                FechaPago = p.FechaPago,
                TipoPago = p.TipoPago.ToString(),
                ModoPago = p.ModoPago.ToString(),
                Monto = p.Monto,
                Referencia = p.Referencia,
                EstadoPago = p.EstadoPago.ToString()
            }).ToList()
        };

        return Result<CuentaPorCobrarDetalleDto>.Success(dto);
    }

    public async Task<Result<CuentaPorCobrarResumenDto>> GenerarDesdeFacturaAsync(
        long facturaVentaId,
        CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<CuentaPorCobrarResumenDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var factura = await _facturaRepo.GetByIdAsync(facturaVentaId, ct);
        if (factura is null || !factura.Activo)
            return Result<CuentaPorCobrarResumenDto>.Failure("Factura no encontrada.");

        if (factura.EmpresaId != empresaId)
            return Result<CuentaPorCobrarResumenDto>.Failure("La factura no pertenece a la empresa activa.");

        // Obtener Venta y Cliente
        var venta = await _ventaRepo.GetByIdAsync(factura.VentaId, ct);
        var fechaVencimiento = venta?.FechaVencimientoPago ?? venta?.FechaVenta ?? factura.FechaEmision;

        int? clienteId = venta?.ClienteId;
        string? clienteNombre = null;
        string? clienteNit = null;

        if (clienteId.HasValue)
        {
            var cliente = await _clienteRepo.GetByIdAsync(clienteId.Value, ct);
            if (cliente is not null)
            {
                clienteNombre = cliente.RazonSocial;
                clienteNit = cliente.NIT;
            }
        }

        // Verificar si ya existe una CxC para esta factura
        var existing = (await _cxcRepo.FindAsync(
            c => c.FacturaVentaId == facturaVentaId && c.EmpresaId == empresaId && c.Activo, ct))
            .FirstOrDefault();

        if (existing is not null)
        {
            // Actualizar datos existentes
            existing.TotalFactura = factura.Total;
            existing.FechaEmision = factura.FechaEmision;
            existing.NumeroFactura = factura.NumeroFactura;
            existing.NumeroVenta = venta?.NumeroVenta;
            existing.FechaVencimiento = fechaVencimiento;
            existing.ClienteId = clienteId;
            existing.ClienteNombre = clienteNombre;
            existing.ClienteNit = clienteNit;
            existing.MonedaCodigo = factura.MonedaCodigo;
            existing.TipoCambio = factura.TipoCambio;
            existing.Estado = CalcularEstado(existing.TotalFactura, existing.TotalPagado, existing.FechaVencimiento);

            // Si la factura está revertida/anulada, anular la CxC
            if (factura.Revertido)
                existing.Estado = EstadoCuentaPorCobrar.Anulada;

            await _cxcRepo.UpdateAsync(existing, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<CuentaPorCobrarResumenDto>.Success(MapToResumen(existing, factura.VentaId));
        }

        // Crear nueva CxC
        var cxc = new CuentaPorCobrar
        {
            EmpresaId = empresaId,
            FacturaVentaId = factura.Id,
            NumeroFactura = factura.NumeroFactura,
            NumeroVenta = venta?.NumeroVenta,
            FechaEmision = factura.FechaEmision,
            FechaVencimiento = fechaVencimiento,
            ClienteId = clienteId,
            ClienteNombre = clienteNombre,
            ClienteNit = clienteNit,
            MonedaCodigo = factura.MonedaCodigo,
            TipoCambio = factura.TipoCambio,
            TotalFactura = factura.Total,
            TotalPagado = 0m,
            Estado = factura.Revertido
                ? EstadoCuentaPorCobrar.Anulada
                : CalcularEstado(factura.Total, 0m, fechaVencimiento),
            Activo = true
        };

        await _cxcRepo.AddAsync(cxc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<CuentaPorCobrarResumenDto>.Success(MapToResumen(cxc, factura.VentaId));
    }

    public async Task<Result<CuentaPorCobrarResumenDto>> ActualizarPorPagoAsync(
        long facturaVentaId,
        decimal montoPagado,
        CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<CuentaPorCobrarResumenDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var cxc = (await _cxcRepo.FindAsync(
            c => c.FacturaVentaId == facturaVentaId && c.EmpresaId == empresaId && c.Activo, ct))
            .FirstOrDefault();

        if (cxc is null)
        {
            // Si no existe CxC, intentar generarla automáticamente
            var genResult = await GenerarDesdeFacturaAsync(facturaVentaId, ct);
            if (!genResult.IsSuccess)
                return Result<CuentaPorCobrarResumenDto>.Failure(
                    "No se encontró cuenta por cobrar y no se pudo generar automáticamente.");

            cxc = (await _cxcRepo.FindAsync(
                c => c.FacturaVentaId == facturaVentaId && c.EmpresaId == empresaId && c.Activo, ct))
                .FirstOrDefault()!;
        }

        if (cxc.Estado == EstadoCuentaPorCobrar.Anulada)
            return Result<CuentaPorCobrarResumenDto>.Failure(
                "No se puede actualizar una cuenta por cobrar anulada.");

        // Obtener total pagado real desde VentaPagos
        var factura = cxc.FacturaVentaId.HasValue
            ? await _facturaRepo.GetByIdAsync(cxc.FacturaVentaId.Value, ct)
            : null;
        if (cxc.FacturaVentaId.HasValue && factura is null)
            return Result<CuentaPorCobrarResumenDto>.Failure("Documento origen no encontrado.");

        if (cxc.Estado == EstadoCuentaPorCobrar.Anulada)
            return Result<CuentaPorCobrarResumenDto>.Failure(
                "No se puede actualizar una cuenta por cobrar anulada.");

        // Obtener total pagado real desde VentaPagos
        var ventaId = factura?.VentaId ?? cxc.VentaId;
        if (!ventaId.HasValue)
            return Result<CuentaPorCobrarResumenDto>.Failure("No se pudo determinar la venta asociada.");

        var pagos = await _pagoRepo.FindAsync(
            p => p.VentaId == ventaId.Value
                 && p.EmpresaId == empresaId
                 && p.Activo
                 && !p.Anulado, ct);
        var totalPagado = pagos.Sum(p => p.Monto);

        cxc.TotalPagado = totalPagado;
        cxc.Estado = CalcularEstado(cxc.TotalFactura, totalPagado, cxc.FechaVencimiento);

        await _cxcRepo.UpdateAsync(cxc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<CuentaPorCobrarResumenDto>.Success(MapToResumen(cxc, ventaId.Value));
    }

    public async Task<Result<bool>> AnularAsync(long id, CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<bool>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var cxc = (await _cxcRepo.FindAsync(
            c => c.Id == id && c.EmpresaId == empresaId && c.Activo, ct))
            .FirstOrDefault();

        if (cxc is null)
            return Result<bool>.Failure("Cuenta por cobrar no encontrada.");

        if (cxc.Estado == EstadoCuentaPorCobrar.Anulada)
            return Result<bool>.Failure("La cuenta por cobrar ya está anulada.");

        cxc.Estado = EstadoCuentaPorCobrar.Anulada;
        await _cxcRepo.UpdateAsync(cxc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<decimal>> GetSaldoTotalPendienteAsync(CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<decimal>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var cuentas = await _cxcRepo.FindAsync(
            c => c.EmpresaId == empresaId
                 && c.Activo
                 && c.Estado != EstadoCuentaPorCobrar.Anulada,
            ct);

        var saldoTotal = cuentas.Sum(c => c.SaldoPendiente);
        return Result<decimal>.Success(saldoTotal);
    }

    public async Task<Result<AntiguedadSaldosResumenDto>> GetAntiguedadSaldosAsync(CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<AntiguedadSaldosResumenDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var cuentas = await _cxcRepo.FindAsync(
            c => c.EmpresaId == empresaId && c.Activo, ct);

        var cuentasFiltradas = cuentas
            .Where(c => GetDynamicEstado(c) != EstadoCuentaPorCobrar.Anulada
                     && GetDynamicEstado(c) != EstadoCuentaPorCobrar.Pagada
                     && c.SaldoPendiente > 0)
            .ToList();

        var today = DateTime.Today;

        var clientesAgrupados = cuentasFiltradas
            .GroupBy(c => new { ClienteId = c.ClienteId ?? 0, Nombre = c.ClienteNombre ?? "Cliente sin Nombre", Nit = c.ClienteNit ?? string.Empty })
            .Select(g =>
            {
                var dto = new AntiguedadSaldosClienteDto
                {
                    ClienteId = g.Key.ClienteId,
                    ClienteNombre = g.Key.Nombre,
                    ClienteNit = g.Key.Nit
                };

                foreach (var cxc in g)
                {
                    var saldo = cxc.SaldoPendiente;

                    if (!cxc.FechaVencimiento.HasValue || cxc.FechaVencimiento.Value.Date >= today)
                    {
                        dto.NoVencido += saldo;
                    }
                    else
                    {
                        var diasVencido = (today - cxc.FechaVencimiento.Value.Date).Days;

                        if (diasVencido <= 30)
                            dto.Vencido1A30 += saldo;
                        else if (diasVencido <= 60)
                            dto.Vencido31A60 += saldo;
                        else if (diasVencido <= 90)
                            dto.Vencido61A90 += saldo;
                        else
                            dto.VencidoMas90 += saldo;
                    }
                }

                return dto;
            })
            .OrderBy(c => c.ClienteNombre)
            .ToList();

        var totalPagado = cuentas
            .Where(c => GetDynamicEstado(c) != EstadoCuentaPorCobrar.Anulada)
            .Sum(c => c.TotalPagado);

        var saldoPendiente = cuentas
            .Where(c => GetDynamicEstado(c) != EstadoCuentaPorCobrar.Anulada)
            .Sum(c => c.SaldoPendiente);

        var resumen = new AntiguedadSaldosResumenDto
        {
            Clientes = clientesAgrupados,
            TotalNoVencido = clientesAgrupados.Sum(c => c.NoVencido),
            TotalVencido1A30 = clientesAgrupados.Sum(c => c.Vencido1A30),
            TotalVencido31A60 = clientesAgrupados.Sum(c => c.Vencido31A60),
            TotalVencido61A90 = clientesAgrupados.Sum(c => c.Vencido61A90),
            TotalVencidoMas90 = clientesAgrupados.Sum(c => c.VencidoMas90),
            TotalPagado = totalPagado,
            SaldoPendiente = saldoPendiente
        };

        resumen.TotalGeneral = resumen.TotalNoVencido 
                               + resumen.TotalVencido1A30 
                               + resumen.TotalVencido31A60 
                               + resumen.TotalVencido61A90 
                               + resumen.TotalVencidoMas90;

        return Result<AntiguedadSaldosResumenDto>.Success(resumen);
    }

    public async Task<Result<CuentaPorCobrarResumenDto>> ActualizarPorPagoVentaAsync(
        long ventaId,
        CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<CuentaPorCobrarResumenDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        // Obtener todos los pagos activos de la venta
        var pagos = await _pagoRepo.FindAsync(
            p => p.VentaId == ventaId && p.EmpresaId == empresaId && p.Activo && !p.Anulado, ct);
        var totalPagado = pagos.Sum(p => p.Monto);

        CuentaPorCobrarResumenDto? ultimoResumen = null;

        // 1. Intentar actualizar CxC existente creada desde venta directa (sin factura)
        var cxcVenta = (await _cxcRepo.FindAsync(
            c => c.VentaId == ventaId
              && c.EmpresaId == empresaId
              && c.Activo
              && c.TipoDocumentoOrigen == "Venta", ct))
            .FirstOrDefault();

        if (cxcVenta is not null)
        {
            cxcVenta.TotalPagado = totalPagado;
            cxcVenta.Estado = CalcularEstado(cxcVenta.TotalFactura, totalPagado, cxcVenta.FechaVencimiento);
            await _cxcRepo.UpdateAsync(cxcVenta, ct);
            ultimoResumen = MapToResumen(cxcVenta, ventaId);
        }

        // 2. También actualizar CxC creadas desde factura (si existen)
        var facturas = await _facturaRepo.FindAsync(
            f => f.VentaId == ventaId && f.EmpresaId == empresaId && f.Activo, ct);

        foreach (var factura in facturas)
        {
            var cxcFactura = (await _cxcRepo.FindAsync(
                c => c.FacturaVentaId == factura.Id && c.EmpresaId == empresaId && c.Activo, ct))
                .FirstOrDefault();

            if (cxcFactura is null)
            {
                // Si no existe CxC para esta factura, crearla
                var genRes = await GenerarDesdeFacturaAsync(factura.Id, ct);
                if (genRes.IsSuccess)
                {
                    ultimoResumen = genRes.Value;
                }
                continue;
            }

            // Si la CxC de factura es la misma que la de venta (mismo registro), no duplicar
            if (cxcVenta is not null && cxcFactura.Id == cxcVenta.Id)
                continue;

            cxcFactura.TotalPagado = totalPagado;
            cxcFactura.Estado = CalcularEstado(cxcFactura.TotalFactura, totalPagado, cxcFactura.FechaVencimiento);
            await _cxcRepo.UpdateAsync(cxcFactura, ct);
            ultimoResumen = MapToResumen(cxcFactura, ventaId);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        if (ultimoResumen is null)
            return Result<CuentaPorCobrarResumenDto>.Failure("No se pudo actualizar ninguna cuenta por cobrar.");

        return Result<CuentaPorCobrarResumenDto>.Success(ultimoResumen);
    }

    public async Task<Result<CuentaPorCobrarResumenDto>> GenerarDesdeVentaAsync(
        long ventaId,
        CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<CuentaPorCobrarResumenDto>.Failure("No existe empresa activa en la sesión.");

        var empresaId = _currentUser.EmpresaId.Value;

        // Buscar venta
        var venta = await _ventaRepo.GetByIdAsync(ventaId, ct);
        if (venta is null || !venta.Activo)
            return Result<CuentaPorCobrarResumenDto>.Failure("Venta no encontrada.");

        if (venta.EmpresaId != empresaId)
            return Result<CuentaPorCobrarResumenDto>.Failure("La venta no pertenece a la empresa activa.");

        // Validar estado: solo ventas confirmadas o superiores generan CxC
        if (venta.EstadoVenta < EstadoVenta.Confirmada)
            return Result<CuentaPorCobrarResumenDto>.Failure(
                "La venta debe estar confirmada o en estado superior para generar cuenta por cobrar.");

        // Validar total mayor a cero
        if (venta.Total <= 0)
            return Result<CuentaPorCobrarResumenDto>.Failure(
                "La venta debe tener un total mayor a cero para generar cuenta por cobrar.");

        // Validar cliente
        if (!venta.ClienteId.HasValue)
            return Result<CuentaPorCobrarResumenDto>.Failure(
                "La venta debe tener un cliente asignado para generar cuenta por cobrar.");

        // Verificar si ya existe CxC activa para esta venta
        var existing = (await _cxcRepo.FindAsync(
            c => c.VentaId == ventaId
              && c.EmpresaId == empresaId
              && c.Activo
              && c.Estado != EstadoCuentaPorCobrar.Anulada, ct))
            .FirstOrDefault();

        if (existing is not null)
        {
            // Actualizar datos existentes
            existing.TotalFactura = venta.Total;
            existing.FechaEmision = venta.FechaVenta;
            existing.NumeroVenta = venta.NumeroVenta;
            existing.FechaVencimiento = venta.FechaVencimientoPago;
            existing.ClienteId = venta.ClienteId;
            existing.Estado = CalcularEstado(existing.TotalFactura, existing.TotalPagado, existing.FechaVencimiento);

            // Obtener datos del cliente
            if (venta.ClienteId.HasValue)
            {
                var cliente = await _clienteRepo.GetByIdAsync(venta.ClienteId.Value, ct);
                if (cliente is not null)
                {
                    existing.ClienteNombre = cliente.RazonSocial;
                    existing.ClienteNit = cliente.NIT;
                }
            }

            await _cxcRepo.UpdateAsync(existing, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<CuentaPorCobrarResumenDto>.Success(MapToResumen(existing, venta.Id));
        }

        // Obtener datos del cliente
        string? clienteNombre = null;
        string? clienteNit = null;
        if (venta.ClienteId.HasValue)
        {
            var cliente = await _clienteRepo.GetByIdAsync(venta.ClienteId.Value, ct);
            if (cliente is not null)
            {
                clienteNombre = cliente.RazonSocial;
                clienteNit = cliente.NIT;
            }
        }

        // Crear nueva CxC desde venta (sin factura)
        var cxc = new CuentaPorCobrar
        {
            EmpresaId = empresaId,
            VentaId = venta.Id,
            FacturaVentaId = null,
            TipoDocumentoOrigen = "Venta",
            NumeroFactura = null,
            NumeroVenta = venta.NumeroVenta,
            FechaEmision = venta.FechaVenta,
            FechaVencimiento = venta.FechaVencimientoPago,
            ClienteId = venta.ClienteId,
            ClienteNombre = clienteNombre,
            ClienteNit = clienteNit,
            MonedaCodigo = venta.MonedaCodigo,
            TipoCambio = venta.TipoCambio,
            TotalFactura = venta.Total,
            TotalPagado = 0m,
            Estado = EstadoCuentaPorCobrar.Pendiente,
            Activo = true
        };

        await _cxcRepo.AddAsync(cxc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<CuentaPorCobrarResumenDto>.Success(MapToResumen(cxc, venta.Id));
    }

    // ─── Helpers privados ────────────────────────────────────────────────────

    private static EstadoCuentaPorCobrar CalcularEstado(decimal totalFactura, decimal totalPagado, DateTime? fechaVencimiento)
    {
        if (totalPagado >= totalFactura)
            return EstadoCuentaPorCobrar.Pagada;

        if (fechaVencimiento.HasValue && fechaVencimiento.Value.Date < DateTime.Today)
            return EstadoCuentaPorCobrar.Vencida;

        if (totalPagado <= 0)
            return EstadoCuentaPorCobrar.Pendiente;

        return EstadoCuentaPorCobrar.Parcial;
    }

    private static EstadoCuentaPorCobrar GetDynamicEstado(CuentaPorCobrar cxc)
    {
        if (cxc.Estado == EstadoCuentaPorCobrar.Anulada)
            return EstadoCuentaPorCobrar.Anulada;

        return CalcularEstado(cxc.TotalFactura, cxc.TotalPagado, cxc.FechaVencimiento);
    }

    private static CuentaPorCobrarResumenDto MapToResumen(CuentaPorCobrar cxc, long ventaId = 0)
    {
        return new CuentaPorCobrarResumenDto
        {
            Id = cxc.Id,
            FacturaVentaId = cxc.FacturaVentaId,
            VentaId = cxc.VentaId ?? ventaId,
            TipoDocumentoOrigen = cxc.TipoDocumentoOrigen,
            NumeroFactura = cxc.NumeroFactura,
            NumeroVenta = cxc.NumeroVenta,
            ClienteId = cxc.ClienteId,
            ClienteNombre = cxc.ClienteNombre,
            ClienteNit = cxc.ClienteNit,
            FechaEmision = cxc.FechaEmision,
            FechaVencimiento = cxc.FechaVencimiento,
            MonedaCodigo = cxc.MonedaCodigo,
            TipoCambio = cxc.TipoCambio,
            TotalFactura = cxc.TotalFactura,
            TotalPagado = cxc.TotalPagado,
            SaldoPendiente = cxc.SaldoPendiente,
            Estado = GetDynamicEstado(cxc).ToString()
        };
    }

}
