using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.CXC;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.CxC;

namespace AgoraHub360.ERP.Application.Services;

public class CuentasPorCobrarService : ICuentasPorCobrarService
{
    private readonly IRepository<CuentaPorCobrar> _cxcRepo;
    private readonly IRepository<FacturaVenta> _facturaRepo;
    private readonly IRepository<VentaPago> _pagoRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CuentasPorCobrarService(
        IRepository<CuentaPorCobrar> cxcRepo,
        IRepository<FacturaVenta> facturaRepo,
        IRepository<VentaPago> pagoRepo,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _cxcRepo = cxcRepo;
        _facturaRepo = facturaRepo;
        _pagoRepo = pagoRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<CuentaPorCobrarResumenDto>>> GetAllAsync(
        CuentaPorCobrarFilterDto? filter = null,
        CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<List<CuentaPorCobrarResumenDto>>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var query = (await _cxcRepo.FindAsync(
            c => c.EmpresaId == empresaId && c.Activo, ct))
            .AsQueryable();

        // Aplicar filtros
        if (filter is not null)
        {
            if (filter.ClienteId.HasValue)
                query = query.Where(c => c.ClienteId == filter.ClienteId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Estado)
                && Enum.TryParse<EstadoCuentaPorCobrar>(filter.Estado, true, out var estado))
                query = query.Where(c => c.Estado == estado);

            if (filter.FechaDesde.HasValue)
                query = query.Where(c => c.FechaEmision >= filter.FechaDesde.Value);

            if (filter.FechaHasta.HasValue)
                query = query.Where(c => c.FechaEmision <= filter.FechaHasta.Value);

            if (filter.FechaVencimientoDesde.HasValue)
                query = query.Where(c => c.FechaVencimiento >= filter.FechaVencimientoDesde.Value);

            if (filter.FechaVencimientoHasta.HasValue)
                query = query.Where(c => c.FechaVencimiento <= filter.FechaVencimientoHasta.Value);
        }

        var result = query
            .OrderByDescending(c => c.FechaEmision)
            .ThenByDescending(c => c.Id)
            .Select(c => MapToResumen(c))
            .ToList();

        return Result<List<CuentaPorCobrarResumenDto>>.Success(result);
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

        // Obtener pagos de la venta asociada a la factura
        var factura = await _facturaRepo.GetByIdAsync(cxc.FacturaVentaId, ct);
        if (factura is null)
            return Result<CuentaPorCobrarDetalleDto>.Failure("Factura asociada no encontrada.");

        var pagos = await _pagoRepo.FindAsync(
            p => p.VentaId == factura.VentaId
                 && p.EmpresaId == empresaId
                 && p.Activo, ct);

        var dto = new CuentaPorCobrarDetalleDto
        {
            Id = cxc.Id,
            FacturaVentaId = cxc.FacturaVentaId,
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
            Estado = cxc.Estado.ToString(),
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
            existing.ClienteId = null; // No tenemos VentaId sin Include
            existing.MonedaCodigo = factura.MonedaCodigo;
            existing.TipoCambio = factura.TipoCambio;
            existing.Estado = CalcularEstado(existing.TotalFactura, existing.TotalPagado);

            // Si la factura está revertida/anulada, anular la CxC
            if (factura.Revertido)
                existing.Estado = EstadoCuentaPorCobrar.Anulada;

            await _cxcRepo.UpdateAsync(existing, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<CuentaPorCobrarResumenDto>.Success(MapToResumen(existing));
        }

        // Crear nueva CxC
        var cxc = new CuentaPorCobrar
        {
            EmpresaId = empresaId,
            FacturaVentaId = factura.Id,
            NumeroFactura = factura.NumeroFactura,
            FechaEmision = factura.FechaEmision,
            MonedaCodigo = factura.MonedaCodigo,
            TipoCambio = factura.TipoCambio,
            TotalFactura = factura.Total,
            TotalPagado = 0m,
            Estado = factura.Revertido
                ? EstadoCuentaPorCobrar.Anulada
                : EstadoCuentaPorCobrar.Pendiente,
            Activo = true
        };

        await _cxcRepo.AddAsync(cxc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<CuentaPorCobrarResumenDto>.Success(MapToResumen(cxc));
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
        var facturaPago = await _facturaRepo.GetByIdAsync(cxc.FacturaVentaId, ct);
        if (facturaPago is null)
            return Result<CuentaPorCobrarResumenDto>.Failure("Factura asociada no encontrada.");

        var pagos = await _pagoRepo.FindAsync(
            p => p.VentaId == facturaPago.VentaId
                 && p.EmpresaId == empresaId
                 && p.Activo, ct);
        var totalPagado = pagos.Sum(p => p.Monto);

        cxc.TotalPagado = totalPagado;
        cxc.Estado = CalcularEstado(cxc.TotalFactura, totalPagado);

        await _cxcRepo.UpdateAsync(cxc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<CuentaPorCobrarResumenDto>.Success(MapToResumen(cxc));
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
                 && (c.Estado == EstadoCuentaPorCobrar.Pendiente
                     || c.Estado == EstadoCuentaPorCobrar.Parcial
                     || c.Estado == EstadoCuentaPorCobrar.Vencida),
            ct);

        var saldoTotal = cuentas.Sum(c => c.SaldoPendiente);
        return Result<decimal>.Success(saldoTotal);
    }

    // ─── Helpers privados ────────────────────────────────────────────────────

    private static EstadoCuentaPorCobrar CalcularEstado(decimal totalFactura, decimal totalPagado)
    {
        if (totalPagado <= 0)
            return EstadoCuentaPorCobrar.Pendiente;

        if (totalPagado < totalFactura)
            return EstadoCuentaPorCobrar.Parcial;

        return EstadoCuentaPorCobrar.Pagada;
    }

    private static CuentaPorCobrarResumenDto MapToResumen(CuentaPorCobrar cxc)
    {
        return new CuentaPorCobrarResumenDto
        {
            Id = cxc.Id,
            FacturaVentaId = cxc.FacturaVentaId,
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
            Estado = cxc.Estado.ToString()
        };
    }
}
