using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Application.Interfaces.Reports;
using AgoraHub360.ERP.Domain.Entities.CXC;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Infrastructure.Pdf;
using AgoraHub360.ERP.Shared.DTOs.Reportes;

namespace AgoraHub360.ERP.Infrastructure.Services;

public class PdfReporteService : IPdfReporteService
{
    private readonly IRepository<CuentaPorCobrar> _cxcRepo;
    private readonly IRepository<FacturaVenta> _facturaRepo;
    private readonly IRepository<FacturaVentaDetalle> _facturaDetalleRepo;
    private readonly IRepository<Venta> _ventaRepo;
    private readonly IRepository<VentaDetalle> _detalleRepo;
    private readonly IRepository<VentaPago> _pagoRepo;
    private readonly IRepository<Cliente> _clienteRepo;
    private readonly IRepository<Empresa> _empresaRepo;
    private readonly ICurrentUserService _currentUser;

    public PdfReporteService(
        IRepository<CuentaPorCobrar> cxcRepo,
        IRepository<FacturaVenta> facturaRepo,
        IRepository<FacturaVentaDetalle> facturaDetalleRepo,
        IRepository<Venta> ventaRepo,
        IRepository<VentaDetalle> detalleRepo,
        IRepository<VentaPago> pagoRepo,
        IRepository<Cliente> clienteRepo,
        IRepository<Empresa> empresaRepo,
        ICurrentUserService currentUser)
    {
        _cxcRepo = cxcRepo;
        _facturaRepo = facturaRepo;
        _facturaDetalleRepo = facturaDetalleRepo;
        _ventaRepo = ventaRepo;
        _detalleRepo = detalleRepo;
        _pagoRepo = pagoRepo;
        _clienteRepo = clienteRepo;
        _empresaRepo = empresaRepo;
        _currentUser = currentUser;
    }

    public async Task<byte[]> GenerarReciboPagoAsync(long pagoId, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId
            ?? throw new InvalidOperationException("No se pudo determinar la empresa activa.");

        var pago = (await _pagoRepo.FindAsync(p => p.Id == pagoId && p.EmpresaId == empresaId && p.Activo, ct))
            .FirstOrDefault()
            ?? throw new KeyNotFoundException("Pago no encontrado o no pertenece a la empresa activa.");

        var empresa = (await _empresaRepo.FindAsync(e => e.Id == empresaId, ct)).FirstOrDefault();
        var venta = await _ventaRepo.GetByIdAsync(pago.VentaId, ct);
        var facturas = await _facturaRepo.FindAsync(f => f.VentaId == pago.VentaId && f.EmpresaId == empresaId && f.Activo, ct);
        var factura = facturas.FirstOrDefault();
        var cuentas = factura != null
            ? await _cxcRepo.FindAsync(c => c.FacturaVentaId == factura.Id && c.EmpresaId == empresaId && c.Activo, ct)
            : new List<CuentaPorCobrar>();
        var cxc = cuentas.FirstOrDefault();
        var cliente = venta?.ClienteId.HasValue == true ? await _clienteRepo.GetByIdAsync(venta.ClienteId.Value, ct) : null;

        var saldoAnterior = (cxc?.SaldoPendiente ?? 0m) + pago.Monto;
        var saldoPosterior = cxc?.SaldoPendiente ?? 0m;

        var dto = new ReciboPagoDto
        {
            PagoId = pago.Id,
            NumeroRecibo = $"REC-{pago.Id:D6}",
            FechaPago = pago.FechaPago,
            EmpresaNombre = empresa?.Nombre ?? "N/A",
            EmpresaNit = empresa?.NIT,
            EmpresaDireccion = empresa?.Direccion,
            EmpresaTelefono = empresa?.Telefono,
            EmpresaEmail = empresa?.Email,
            ClienteId = cliente?.Id,
            ClienteNombre = cliente?.RazonSocial ?? "N/A",
            ClienteNit = cliente?.NIT ?? "N/A",
            FacturaVentaId = factura?.Id ?? 0,
            NumeroFactura = factura?.NumeroFactura ?? venta?.NumeroVenta ?? "N/A",
            CuentaPorCobrarId = cxc?.Id,
            MontoPagado = pago.Monto,
            MetodoPago = pago.ModoPago.ToString(),
            Referencia = pago.Referencia,
            SaldoAnterior = saldoAnterior,
            SaldoPosterior = saldoPosterior,
            MonedaCodigo = pago.MonedaCodigo ?? "BOB",
            TipoCambio = pago.TipoCambio,
            Observaciones = $"Pago registrado para factura/venta N° {factura?.NumeroFactura ?? venta?.NumeroVenta ?? "N/A"}",
            UsuarioRegistro = pago.CreadoPor ?? _currentUser.UserName
        };

        return PdfGenerator.GenerarReciboPago(dto);
    }

    public async Task<byte[]> GenerarEstadoCuentaClienteAsync(
        int clienteId,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        bool soloPendientes = false,
        bool incluirPagos = false,
        CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId
            ?? throw new InvalidOperationException("No se pudo determinar la empresa activa.");

        var empresa = (await _empresaRepo.FindAsync(e => e.Id == empresaId, ct)).FirstOrDefault();
        var cliente = await _clienteRepo.GetByIdAsync(clienteId, ct)
            ?? throw new KeyNotFoundException("Cliente no encontrado.");

        if (cliente.EmpresaId != empresaId)
            throw new UnauthorizedAccessException("El cliente no pertenece a la empresa activa.");

        var cuentas = await _cxcRepo.FindAsync(c =>
            c.EmpresaId == empresaId && c.Activo && c.ClienteId == clienteId, ct);

        var filtered = cuentas.AsEnumerable();

        if (fechaDesde.HasValue)
            filtered = filtered.Where(c => c.FechaEmision >= fechaDesde.Value);
        if (fechaHasta.HasValue)
            filtered = filtered.Where(c => c.FechaEmision <= fechaHasta.Value);
        if (soloPendientes)
            filtered = filtered.Where(c => c.SaldoPendiente > 0 && c.Estado != EstadoCuentaPorCobrar.Anulada);

        var lista = filtered.ToList();
        var facturaIds = lista.Select(c => c.FacturaVentaId).Distinct().ToList();
        var facturas = facturaIds.Any()
            ? await _facturaRepo.FindAsync(f => facturaIds.Contains(f.Id), ct)
            : new List<FacturaVenta>();
        var facturaMap = facturas.ToDictionary(f => f.Id, f => f.VentaId);

        var dto = new EstadoCuentaClienteDto
        {
            EmpresaNombre = empresa?.Nombre ?? "N/A",
            EmpresaNit = empresa?.NIT,
            EmpresaDireccion = empresa?.Direccion,
            EmpresaTelefono = empresa?.Telefono,
            EmpresaEmail = empresa?.Email,
            ClienteId = cliente.Id,
            ClienteNombre = cliente.RazonSocial,
            ClienteNit = cliente.NIT,
            ClienteDireccion = cliente.Direccion,
            ClienteTelefono = cliente.Telefono,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            MonedaCodigo = "BOB",
            Documentos = lista.Select(c => new EstadoCuentaDocumentoDto
            {
                Id = c.Id,
                NumeroFactura = c.NumeroFactura,
                FechaEmision = c.FechaEmision,
                FechaVencimiento = c.FechaVencimiento,
                MontoOriginal = c.TotalFactura,
                MontoPagado = c.TotalPagado,
                Saldo = c.SaldoPendiente,
                Estado = c.Estado.ToString(),
                DiasVencidos = c.FechaVencimiento.HasValue && c.SaldoPendiente > 0
                    ? (int?)Math.Max(0, (DateTime.Today - c.FechaVencimiento.Value).Days)
                    : null
            }).ToList()
        };

        dto.TotalFacturado = lista.Sum(c => c.TotalFactura);
        dto.TotalPagado = lista.Sum(c => c.TotalPagado);
        dto.SaldoPendiente = lista.Sum(c => c.SaldoPendiente);
        dto.SaldoVencido = lista
            .Where(c => c.FechaVencimiento.HasValue && c.FechaVencimiento.Value < DateTime.Today && c.SaldoPendiente > 0)
            .Sum(c => c.SaldoPendiente);

        if (incluirPagos && lista.Count > 0)
        {
            var ventaIds = lista
                .Select(c => c.FacturaVentaId.HasValue ? facturaMap.GetValueOrDefault(c.FacturaVentaId.Value) : 0)
                .Where(id => id != 0)
                .Distinct()
                .ToList();

            if (ventaIds.Any())
            {
                var pagos = await _pagoRepo.FindAsync(p =>
                    ventaIds.Contains(p.VentaId) && p.EmpresaId == empresaId && p.Activo, ct);

                dto.Pagos = pagos.Select(p => new EstadoCuentaPagoDto
                {
                    PagoId = p.Id,
                    FechaPago = p.FechaPago,
                    Monto = p.Monto,
                    MetodoPago = p.ModoPago.ToString(),
                    Referencia = p.Referencia,
                    NumeroFactura = lista
                        .FirstOrDefault(c => c.FacturaVentaId.HasValue && facturaMap.GetValueOrDefault(c.FacturaVentaId.Value) == p.VentaId)
                        ?.NumeroFactura ?? "N/A"
                }).ToList();
            }
        }

        return PdfGenerator.GenerarEstadoCuenta(dto);
    }

    public async Task<byte[]> GenerarFacturaPdfAsync(long facturaId, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId
            ?? throw new InvalidOperationException("No se pudo determinar la empresa activa.");

        var factura = (await _facturaRepo.FindAsync(f => f.Id == facturaId && f.EmpresaId == empresaId && f.Activo, ct))
            .FirstOrDefault()
            ?? throw new KeyNotFoundException("Factura no encontrada o no pertenece a la empresa activa.");

        var empresa = (await _empresaRepo.FindAsync(e => e.Id == empresaId, ct)).FirstOrDefault();
        var detalles = await _facturaDetalleRepo.FindAsync(d => d.FacturaVentaId == facturaId && d.Activo, ct);
        var venta = await _ventaRepo.GetByIdAsync(factura.VentaId, ct);
        var cliente = venta?.ClienteId.HasValue == true ? await _clienteRepo.GetByIdAsync(venta.ClienteId.Value, ct) : null;

        var dto = new FacturaPdfDto
        {
            EmpresaNombre = empresa?.Nombre ?? "N/A",
            EmpresaNit = empresa?.NIT,
            EmpresaDireccion = empresa?.Direccion,
            EmpresaTelefono = empresa?.Telefono,
            EmpresaEmail = empresa?.Email,
            ClienteNombre = factura.RazonSocialFactura ?? cliente?.RazonSocial ?? "N/A",
            ClienteNit = factura.NitFactura ?? cliente?.NIT ?? "N/A",
            ClienteDireccion = cliente?.Direccion,
            NumeroFactura = factura.NumeroFactura,
            NumeroAutorizacion = factura.NumeroAutorizacion,
            FechaEmision = factura.FechaEmision,
            MonedaCodigo = factura.MonedaCodigo ?? "BOB",
            TipoCambio = factura.TipoCambio,
            Subtotal = factura.Subtotal,
            DescuentoTotal = factura.DescuentoTotal,
            ImpuestoTotal = factura.ImpuestoTotal,
            Total = factura.Total,
            EstadoFactura = factura.EstadoFactura.ToString(),
            Observaciones = factura.Observaciones,
            Cuf = factura.Cuf,
            Cufd = factura.Cufd,
            Leyenda = factura.Leyenda,
            SiatQr = factura.SiatQr,
            EstadoSiat = factura.EstadoSiat.ToString(),
            Detalles = detalles.Select(d => new FacturaPdfDetalleDto
            {
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                UnidadMedida = d.UnidadMedida ?? "",
                PrecioUnitario = d.PrecioUnitario,
                DescuentoMonto = d.DescuentoMonto,
                ImpuestoMonto = d.ImpuestoMonto,
                TotalLinea = d.TotalLinea
            }).ToList()
        };

        return PdfGenerator.GenerarFactura(dto);
    }

    public async Task<byte[]> GenerarReporteCxcGeneralAsync(
        int? clienteId = null,
        string? estado = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        bool? vencidas = null,
        bool? conSaldo = null,
        CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId
            ?? throw new InvalidOperationException("No se pudo determinar la empresa activa.");

        var empresa = (await _empresaRepo.FindAsync(e => e.Id == empresaId, ct)).FirstOrDefault();

        var query = await _cxcRepo.FindAsync(c => c.EmpresaId == empresaId && c.Activo, ct);
        var cuentas = query.AsEnumerable();

        if (clienteId.HasValue)
            cuentas = cuentas.Where(c => c.ClienteId == clienteId);
        if (!string.IsNullOrWhiteSpace(estado) && !estado.Equals("Todos", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<EstadoCuentaPorCobrar>(estado, true, out var estadoFilter))
                cuentas = cuentas.Where(c => c.Estado == estadoFilter);
        }
        if (fechaDesde.HasValue)
            cuentas = cuentas.Where(c => c.FechaEmision >= fechaDesde.Value);
        if (fechaHasta.HasValue)
            cuentas = cuentas.Where(c => c.FechaEmision <= fechaHasta.Value);
        if (vencidas == true)
            cuentas = cuentas.Where(c => c.FechaVencimiento.HasValue && c.FechaVencimiento.Value < DateTime.Today && c.SaldoPendiente > 0);
        if (conSaldo == true)
            cuentas = cuentas.Where(c => c.SaldoPendiente > 0);

        var lista = cuentas.OrderBy(c => c.ClienteNombre).ThenBy(c => c.FechaEmision).ToList();

        var dto = new ReporteCxcGeneralDto
        {
            EmpresaNombre = empresa?.Nombre ?? "N/A",
            EmpresaNit = empresa?.NIT,
            ClienteId = clienteId,
            Estado = estado,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            MonedaCodigo = "BOB",
            TotalCuentas = lista.Count,
            CuentasPendientes = lista.Count(c => c.Estado == EstadoCuentaPorCobrar.Pendiente || c.Estado == EstadoCuentaPorCobrar.Parcial),
            CuentasVencidas = lista.Count(c => c.FechaVencimiento.HasValue && c.FechaVencimiento.Value < DateTime.Today && c.SaldoPendiente > 0),
            CuentasPagadas = lista.Count(c => c.Estado == EstadoCuentaPorCobrar.Pagada),
            TotalSaldoPendiente = lista.Where(c => c.Estado != EstadoCuentaPorCobrar.Anulada).Sum(c => c.SaldoPendiente),
            Items = lista.Select(c => new ReporteCxcItemDto
            {
                Id = c.Id,
                ClienteNombre = c.ClienteNombre,
                ClienteNit = c.ClienteNit,
                NumeroFactura = c.NumeroFactura,
                FechaEmision = c.FechaEmision,
                FechaVencimiento = c.FechaVencimiento,
                MontoOriginal = c.TotalFactura,
                MontoPagado = c.TotalPagado,
                SaldoPendiente = c.SaldoPendiente,
                Estado = c.Estado.ToString()
            }).ToList(),
            TotalesPorEstado = lista
                .GroupBy(c => c.Estado)
                .Select(g => new ReporteCxcTotalPorEstadoDto
                {
                    Estado = g.Key.ToString(),
                    Cantidad = g.Count(),
                    TotalSaldo = g.Sum(c => c.SaldoPendiente)
                }).ToList()
        };

        return PdfGenerator.GenerarReporteCxcGeneral(dto);
    }

    public async Task<byte[]> GenerarReporteCxcVencidasAsync(CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId
            ?? throw new InvalidOperationException("No se pudo determinar la empresa activa.");

        var empresa = (await _empresaRepo.FindAsync(e => e.Id == empresaId, ct)).FirstOrDefault();
        var hoy = DateTime.Today;

        var cuentas = await _cxcRepo.FindAsync(c =>
            c.EmpresaId == empresaId && c.Activo
            && c.FechaVencimiento.HasValue && c.FechaVencimiento.Value < hoy
            && c.SaldoPendiente > 0, ct);

        var lista = cuentas.OrderBy(c => c.ClienteNombre).ThenBy(c => c.FechaVencimiento).ToList();

        var dto = new ReporteCxcVencidasDto
        {
            EmpresaNombre = empresa?.Nombre ?? "N/A",
            EmpresaNit = empresa?.NIT,
            FechaCorte = hoy,
            MonedaCodigo = "BOB",
            TotalVencidas = lista.Count,
            TotalSaldoVencido = lista.Sum(c => c.SaldoPendiente),
            Items = lista.Select(c => new ReporteCxcVencidaItemDto
            {
                Id = c.Id,
                ClienteNombre = c.ClienteNombre,
                ClienteNit = c.ClienteNit,
                NumeroFactura = c.NumeroFactura,
                FechaEmision = c.FechaEmision,
                FechaVencimiento = c.FechaVencimiento,
                DiasVencidos = (hoy - c.FechaVencimiento!.Value).Days,
                MontoOriginal = c.TotalFactura,
                MontoPagado = c.TotalPagado,
                SaldoPendiente = c.SaldoPendiente,
                Estado = c.Estado.ToString()
            }).ToList()
        };

        return PdfGenerator.GenerarReporteCxcVencidas(dto);
    }

    public async Task<byte[]> GenerarVentaPdfAsync(long ventaId, CancellationToken ct = default)
    {
        var empresaId = _currentUser.EmpresaId
            ?? throw new InvalidOperationException("No se pudo determinar la empresa activa.");

        var venta = await _ventaRepo.GetByIdAsync(ventaId, ct);
        if (venta is null || venta.EmpresaId != empresaId)
            throw new InvalidOperationException($"Venta {ventaId} no encontrada.");

        var empresa = (await _empresaRepo.FindAsync(e => e.Id == empresaId, ct)).FirstOrDefault();
        var cliente = venta.ClienteId.HasValue && venta.ClienteId.Value > 0
            ? await _clienteRepo.GetByIdAsync(venta.ClienteId.Value, ct)
            : null;

        // Cargar detalles explícitamente — Repository.GetByIdAsync no hace Include
        var detalles = (await _detalleRepo.FindAsync(d => d.VentaId == ventaId, ct))
            .OrderBy(d => d.Id)
            .ToList();

        var dto = new ReporteVentaDto
        {
            EmpresaNombre = empresa?.Nombre ?? "AgoraHUB360 ERP",
            EmpresaNit = empresa?.NIT,
            EmpresaDireccion = empresa?.Direccion,
            EmpresaTelefono = empresa?.Telefono,
            EmpresaEmail = empresa?.Email,
            NumeroVenta = venta.NumeroVenta,
            FechaVenta = venta.FechaVenta,
            FechaVencimientoPago = venta.FechaVencimientoPago,
            MonedaCodigo = venta.MonedaCodigo ?? "BOB",
            TipoCambio = venta.TipoCambio,
            Origen = venta.TipoVenta.ToString(),
            PedidoVentaId = venta.PedidoVentaId,
            ClienteNombre = cliente?.RazonSocial ?? "-",
            ClienteNit = cliente?.NIT ?? "-",
            ClienteDireccion = cliente?.Direccion,
            ClienteTelefono = cliente?.Telefono,
            Observaciones = venta.Observaciones,
            Subtotal = venta.Subtotal,
            DescuentoTotal = venta.DescuentoTotal,
            Total = venta.Total,
            Items = detalles.Select(d => new ReporteVentaItemDto
            {
                Sku = d.CompanyProduct?.Sku ?? "N/D",
                Descripcion = d.Descripcion,
                DetalleAdicional = d.DetalleAdicional,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                DescuentoPorcentaje = d.DescuentoPorcentaje,
                DescuentoMonto = d.DescuentoMonto,
                TotalLinea = d.TotalLinea
            }).ToList()
        };

        return Pdf.PdfGenerator.GenerarVentaPdf(dto);
    }
}
