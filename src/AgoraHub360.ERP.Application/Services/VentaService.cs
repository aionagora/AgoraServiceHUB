namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Ventas;

public class VentaService : IVentaService
{
    private readonly IRepository<Venta> _ventaRepo;
    private readonly IRepository<VentaDetalle> _detalleRepo;
    private readonly IRepository<VentaFacturacionDatos> _facturacionRepo;
    private readonly IRepository<VentaPago> _pagoRepo;
    private readonly IRepository<Sucursal> _sucursalRepo;
    private readonly IRepository<Almacen> _almacenRepo;
    private readonly IRepository<Cliente> _clienteRepo;
    private readonly IRepository<ClientePerfilFiscal> _clientePerfilFiscalRepo;
    private readonly IRepository<PedidoVenta> _pedidoVentaRepo;
    private readonly IRepository<PedidoVentaDetalle> _pedidoVentaDetalleRepo;
    private readonly IRepository<CompanyProduct> _companyProductRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public VentaService(
        IRepository<Venta> ventaRepo,
        IRepository<VentaDetalle> detalleRepo,
        IRepository<VentaFacturacionDatos> facturacionRepo,
        IRepository<VentaPago> pagoRepo,
        IRepository<Sucursal> sucursalRepo,
        IRepository<Almacen> almacenRepo,
        IRepository<Cliente> clienteRepo,
        IRepository<ClientePerfilFiscal> clientePerfilFiscalRepo,
        IRepository<PedidoVenta> pedidoVentaRepo,
        IRepository<PedidoVentaDetalle> pedidoVentaDetalleRepo,
        IRepository<CompanyProduct> companyProductRepo,
        IRepository<Product> productRepo,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _ventaRepo = ventaRepo;
        _detalleRepo = detalleRepo;
        _facturacionRepo = facturacionRepo;
        _pagoRepo = pagoRepo;
        _sucursalRepo = sucursalRepo;
        _almacenRepo = almacenRepo;
        _clienteRepo = clienteRepo;
        _clientePerfilFiscalRepo = clientePerfilFiscalRepo;
        _pedidoVentaRepo = pedidoVentaRepo;
        _pedidoVentaDetalleRepo = pedidoVentaDetalleRepo;
        _companyProductRepo = companyProductRepo;
        _productRepo = productRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<VentaResumenDto>>> GetAllAsync(CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<IReadOnlyList<VentaResumenDto>>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var ventas = await _ventaRepo.FindAsync(v => v.EmpresaId == empresaId && v.Activo, ct);
        var clientes = await _clienteRepo.FindAsync(c => c.EmpresaId == empresaId && c.Activo, ct);
        var sucursales = await _sucursalRepo.FindAsync(s => s.EmpresaId == empresaId && s.Activo, ct);

        var clienteMap = clientes.ToDictionary(c => c.Id, c => c.RazonSocial);
        var sucursalMap = sucursales.ToDictionary(s => s.Id, s => s.Nombre);

        var result = ventas
            .OrderByDescending(v => v.FechaVenta)
            .ThenByDescending(v => v.Id)
            .Select(v => new VentaResumenDto
            {
                Id = v.Id,
                NumeroVenta = v.NumeroVenta,
                FechaVenta = v.FechaVenta,
                ClienteId = v.ClienteId,
                ClienteNombre = v.ClienteId.HasValue && clienteMap.TryGetValue(v.ClienteId.Value, out var cliente) ? cliente : null,
                SucursalId = v.SucursalId,
                SucursalNombre = sucursalMap.TryGetValue(v.SucursalId, out var sucursal) ? sucursal : null,
                TipoVenta = v.TipoVenta.ToString(),
                EstadoVenta = v.EstadoVenta.ToString(),
                EstadoPago = v.EstadoPago.ToString(),
                Total = v.Total,
                FacturaGenerada = v.FacturaGenerada,
                PedidoVentaId = v.PedidoVentaId
            })
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<VentaResumenDto>>.Success(result);
    }

    public async Task<Result<VentaDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<VentaDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;
        var venta = await _ventaRepo.GetByIdAsync(id, ct);
        if (venta is null || !venta.Activo)
            return Result<VentaDto>.Failure("Venta no encontrada.");

        if (venta.EmpresaId != empresaId)
            return Result<VentaDto>.Failure("La venta no pertenece a la empresa activa.");

        var detalles = await _detalleRepo.FindAsync(d => d.VentaId == venta.Id && d.EmpresaId == empresaId && d.Activo, ct);
        var fact = (await _facturacionRepo.FindAsync(f => f.VentaId == venta.Id && f.EmpresaId == empresaId && f.Activo, ct)).FirstOrDefault();
        var pagos = await _pagoRepo.FindAsync(p => p.VentaId == venta.Id && p.EmpresaId == empresaId && p.Activo, ct);

        return Result<VentaDto>.Success(MapToVentaDto(venta, detalles, fact, pagos));
    }

    public async Task<Result<VentaDto>> CreateAsync(CrearVentaRequestDto dto, CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<VentaDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;

        var sucursalValidation = await ValidateSucursalAsync(dto.SucursalId, empresaId, ct);
        if (sucursalValidation is not null)
            return Result<VentaDto>.Failure(sucursalValidation);

        if (dto.AlmacenId.HasValue)
        {
            var almacenValidation = await ValidateAlmacenAsync(dto.AlmacenId.Value, empresaId, ct);
            if (almacenValidation is not null)
                return Result<VentaDto>.Failure(almacenValidation);
        }

        if (dto.ClienteId.HasValue)
        {
            var clienteValidation = await ValidateClienteAsync(dto.ClienteId.Value, empresaId, ct);
            if (clienteValidation is not null)
                return Result<VentaDto>.Failure(clienteValidation);
        }

        if (dto.PedidoVentaId.HasValue)
        {
            var pedidoValidation = await ValidatePedidoVentaAsync(dto.PedidoVentaId.Value, empresaId, ct);
            if (pedidoValidation is not null)
                return Result<VentaDto>.Failure(pedidoValidation);
        }

        if (dto.Detalles is null || dto.Detalles.Count == 0)
            return Result<VentaDto>.Failure("La venta debe contener al menos un detalle.");

        if (!TryParseEnum(dto.TipoVenta, out TipoVenta tipoVenta))
            return Result<VentaDto>.Failure($"TipoVenta inválido: '{dto.TipoVenta}'.");

        var venta = new Venta
        {
            EmpresaId = empresaId,
            SucursalId = dto.SucursalId,
            AlmacenId = dto.AlmacenId,
            ClienteId = dto.ClienteId,
            PedidoVentaId = dto.PedidoVentaId,
            NumeroVenta = await GenerateNumeroVentaAsync(empresaId, ct),
            FechaVenta = dto.FechaVenta,
            TipoVenta = tipoVenta,
            EstadoVenta = EstadoVenta.Borrador,
            EstadoPago = EstadoPagoVenta.Pendiente,
            MonedaId = dto.MonedaId,
            MonedaCodigo = dto.MonedaCodigo,
            TipoCambio = dto.TipoCambio <= 0 ? 1m : dto.TipoCambio,
            Observaciones = dto.Observaciones,
            FacturaGenerada = false,
            InventarioDescontado = false,
            Activo = true
        };

        var detallesResult = await BuildDetallesAsync(dto.Detalles, empresaId, dto.AlmacenId, ct);
        if (!detallesResult.IsSuccess)
            return Result<VentaDto>.Failure(detallesResult.Error!);

        var detalles = detallesResult.Value!;

        var factResult = await BuildFacturacionAsync(dto.FacturacionDatos, empresaId, dto.ClienteId, ct);
        if (!factResult.IsSuccess)
            return Result<VentaDto>.Failure(factResult.Error!);

        var pagosResult = BuildPagos(dto.Pagos, empresaId);
        if (!pagosResult.IsSuccess)
            return Result<VentaDto>.Failure(pagosResult.Error!);

        var pagos = pagosResult.Value!;

        AssignTotales(venta, detalles);
        venta.EstadoPago = CalculateEstadoPago(venta.Total, pagos.Sum(p => p.Monto));

        if (factResult.Value is not null)
        {
            factResult.Value.EmpresaId = empresaId;
            factResult.Value.Activo = true;
            venta.DatosFacturacion = factResult.Value;
            if (factResult.Value.EstadoFactura == EstadoFacturaVenta.Generada)
                venta.FacturaGenerada = true;
        }

        foreach (var d in detalles)
        {
            d.EmpresaId = empresaId;
            d.Activo = true;
            venta.Detalles.Add(d);
        }

        foreach (var p in pagos)
        {
            p.EmpresaId = empresaId;
            p.Activo = true;
            venta.Pagos.Add(p);
        }

        await _ventaRepo.AddAsync(venta, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(venta.Id, ct);
    }

    public async Task<Result<VentaDto>> UpdateAsync(long id, ActualizarVentaRequestDto dto, CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<VentaDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;
        var venta = await _ventaRepo.GetByIdAsync(id, ct);
        if (venta is null || !venta.Activo)
            return Result<VentaDto>.Failure("Venta no encontrada.");

        if (venta.EmpresaId != empresaId)
            return Result<VentaDto>.Failure("La venta no pertenece a la empresa activa.");

        if (venta.EstadoVenta == EstadoVenta.Anulada)
            return Result<VentaDto>.Failure("No se puede modificar una venta anulada.");

        var sucursalValidation = await ValidateSucursalAsync(dto.SucursalId, empresaId, ct);
        if (sucursalValidation is not null)
            return Result<VentaDto>.Failure(sucursalValidation);

        if (dto.AlmacenId.HasValue)
        {
            var almacenValidation = await ValidateAlmacenAsync(dto.AlmacenId.Value, empresaId, ct);
            if (almacenValidation is not null)
                return Result<VentaDto>.Failure(almacenValidation);
        }

        if (dto.ClienteId.HasValue)
        {
            var clienteValidation = await ValidateClienteAsync(dto.ClienteId.Value, empresaId, ct);
            if (clienteValidation is not null)
                return Result<VentaDto>.Failure(clienteValidation);
        }

        if (dto.Detalles is null || dto.Detalles.Count == 0)
            return Result<VentaDto>.Failure("La venta debe contener al menos un detalle.");

        var detallesResult = await BuildDetallesAsync(dto.Detalles, empresaId, dto.AlmacenId, ct);
        if (!detallesResult.IsSuccess)
            return Result<VentaDto>.Failure(detallesResult.Error!);

        var detalles = detallesResult.Value!;
        var factResult = await BuildFacturacionAsync(dto.FacturacionDatos, empresaId, dto.ClienteId, ct);
        if (!factResult.IsSuccess)
            return Result<VentaDto>.Failure(factResult.Error!);

        venta.SucursalId = dto.SucursalId;
        venta.AlmacenId = dto.AlmacenId;
        venta.ClienteId = dto.ClienteId;
        venta.FechaVenta = dto.FechaVenta;
        venta.MonedaId = dto.MonedaId;
        venta.MonedaCodigo = dto.MonedaCodigo;
        venta.TipoCambio = dto.TipoCambio <= 0 ? 1m : dto.TipoCambio;
        venta.Observaciones = dto.Observaciones;

        var oldDetalles = await _detalleRepo.FindAsync(d => d.VentaId == venta.Id && d.EmpresaId == empresaId && d.Activo, ct);
        foreach (var old in oldDetalles)
            await _detalleRepo.DeleteAsync(old, ct);

        foreach (var d in detalles)
        {
            d.EmpresaId = empresaId;
            d.VentaId = venta.Id;
            d.Activo = true;
            await _detalleRepo.AddAsync(d, ct);
        }

        var existingFact = (await _facturacionRepo.FindAsync(f => f.VentaId == venta.Id && f.EmpresaId == empresaId && f.Activo, ct)).FirstOrDefault();
        if (factResult.Value is null)
        {
            if (existingFact is not null)
                await _facturacionRepo.DeleteAsync(existingFact, ct);
            venta.FacturaGenerada = false;
        }
        else
        {
            var fact = factResult.Value;
            if (existingFact is null)
            {
                fact.EmpresaId = empresaId;
                fact.VentaId = venta.Id;
                fact.Activo = true;
                await _facturacionRepo.AddAsync(fact, ct);
            }
            else
            {
                existingFact.Facturar = fact.Facturar;
                existingFact.FacturarAlMismoCliente = fact.FacturarAlMismoCliente;
                existingFact.TipoDocumentoIdentidad = fact.TipoDocumentoIdentidad;
                existingFact.NitFactura = fact.NitFactura;
                existingFact.Complemento = fact.Complemento;
                existingFact.RazonSocialFactura = fact.RazonSocialFactura;
                existingFact.EmailFactura = fact.EmailFactura;
                existingFact.TelefonoFactura = fact.TelefonoFactura;
                existingFact.ClientePerfilFiscalId = fact.ClientePerfilFiscalId;
                existingFact.EstadoFactura = fact.EstadoFactura;
                existingFact.FacturaId = fact.FacturaId;
                await _facturacionRepo.UpdateAsync(existingFact, ct);
            }

            venta.FacturaGenerada = fact.EstadoFactura == EstadoFacturaVenta.Generada;
        }

        AssignTotales(venta, detalles);
        var pagos = await _pagoRepo.FindAsync(p => p.VentaId == venta.Id && p.EmpresaId == empresaId && p.Activo, ct);
        venta.EstadoPago = CalculateEstadoPago(venta.Total, pagos.Sum(p => p.Monto));

        await _ventaRepo.UpdateAsync(venta, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(venta.Id, ct);
    }

    public async Task<Result<VentaDto>> ConfirmarAsync(long id, ConfirmarVentaRequestDto? dto = null, CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<VentaDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;
        var venta = await _ventaRepo.GetByIdAsync(id, ct);
        if (venta is null || !venta.Activo)
            return Result<VentaDto>.Failure("Venta no encontrada.");

        if (venta.EmpresaId != empresaId)
            return Result<VentaDto>.Failure("La venta no pertenece a la empresa activa.");

        if (venta.EstadoVenta != EstadoVenta.Borrador)
            return Result<VentaDto>.Failure("Solo se puede confirmar una venta en estado Borrador.");

        venta.EstadoVenta = EstadoVenta.Confirmada;
        if (!string.IsNullOrWhiteSpace(dto?.Observaciones))
            venta.Observaciones = dto!.Observaciones;

        await _ventaRepo.UpdateAsync(venta, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(venta.Id, ct);
    }

    public async Task<Result<VentaDto>> CrearDesdePedidoAsync(GenerarVentaDesdePedidoRequestDto dto, CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<VentaDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;
        if (dto.PedidoVentaId <= 0)
            return Result<VentaDto>.Failure("PedidoVentaId inválido.");

        var pedido = await _pedidoVentaRepo.GetByIdAsync(dto.PedidoVentaId, ct);
        if (pedido is null)
            return Result<VentaDto>.Failure("Pedido de venta no encontrado.");

        if (pedido.EmpresaId != empresaId)
            return Result<VentaDto>.Failure("El pedido no pertenece a la empresa activa.");

        var existingVenta = (await _ventaRepo.FindAsync(v => v.EmpresaId == empresaId && v.PedidoVentaId == dto.PedidoVentaId && v.Activo && v.EstadoVenta != EstadoVenta.Anulada, ct)).FirstOrDefault();
        if (existingVenta is not null)
            return Result<VentaDto>.Failure("El pedido ya tiene una venta activa asociada.");

        var pedidoDetalles = await _pedidoVentaDetalleRepo.FindAsync(d => d.EmpresaId == empresaId && d.PedidoVentaId == dto.PedidoVentaId && d.Activo, ct);
        if (pedidoDetalles.Count == 0)
            return Result<VentaDto>.Failure("El pedido no tiene detalles para generar la venta.");

        var createDto = new CrearVentaRequestDto
        {
            SucursalId = pedido.SucursalId,
            AlmacenId = pedido.AlmacenId,
            ClienteId = pedido.ClienteId,
            PedidoVentaId = pedido.Id,
            TipoVenta = TipoVenta.DesdePedido.ToString(),
            FechaVenta = DateTime.Now,
            Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones)
                ? $"Generada desde pedido {pedido.Numero}"
                : dto.Observaciones,
            FacturacionDatos = dto.FacturacionDatos,
            Pagos = dto.Pagos,
            Detalles = pedidoDetalles.Select(d => new VentaDetalleDto
            {
                TipoItemVenta = TipoItemVenta.Producto.ToString(),
                CompanyProductId = d.CompanyProductId,
                AlmacenId = pedido.AlmacenId,
                Descripcion = $"Producto {d.CompanyProductId}",
                Cantidad = d.CantidadConfirmada > 0 ? d.CantidadConfirmada : d.CantidadSolicitada,
                PrecioUnitario = d.CantidadSolicitada > 0 ? (d.Subtotal / d.CantidadSolicitada) : 0m,
                DescuentoPorcentaje = 0m,
                DescuentoMonto = 0m,
                ImpuestoMonto = d.Impuestos,
                TotalLinea = d.Total,
                CostoUnitario = null,
                DescuentaInventario = true
            }).ToList()
        };

        return await CreateAsync(createDto, ct);
    }

    public async Task<Result<VentaDto>> RegistrarPagoAsync(long id, RegistrarPagoVentaRequestDto dto, CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<VentaDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;
        var venta = await _ventaRepo.GetByIdAsync(id, ct);
        if (venta is null || !venta.Activo)
            return Result<VentaDto>.Failure("Venta no encontrada.");

        if (venta.EmpresaId != empresaId)
            return Result<VentaDto>.Failure("La venta no pertenece a la empresa activa.");

        if (venta.EstadoVenta == EstadoVenta.Anulada)
            return Result<VentaDto>.Failure("No se pueden registrar pagos en una venta anulada.");

        if (dto.Monto <= 0)
            return Result<VentaDto>.Failure("El monto del pago debe ser mayor a cero.");

        if (!TryParseEnum(dto.TipoPago, out TipoPago tipoPago))
            return Result<VentaDto>.Failure($"TipoPago inválido: '{dto.TipoPago}'.");

        if (!TryParseEnum(dto.ModoPago, out ModoPago modoPago))
            return Result<VentaDto>.Failure($"ModoPago inválido: '{dto.ModoPago}'.");

        if (!TryParseEnum(dto.EstadoPago, out EstadoPagoVenta estadoPago))
            estadoPago = EstadoPagoVenta.Pendiente;

        var pago = new VentaPago
        {
            EmpresaId = empresaId,
            VentaId = venta.Id,
            FechaPago = dto.FechaPago,
            TipoPago = tipoPago,
            ModoPago = modoPago,
            CuentaCajaBancoId = dto.CuentaCajaBancoId,
            Monto = dto.Monto,
            MonedaId = dto.MonedaId,
            MonedaCodigo = dto.MonedaCodigo,
            TipoCambio = dto.TipoCambio <= 0 ? 1m : dto.TipoCambio,
            Referencia = dto.Referencia,
            EstadoPago = estadoPago,
            Activo = true
        };

        await _pagoRepo.AddAsync(pago, ct);

        var pagos = await _pagoRepo.FindAsync(p => p.EmpresaId == empresaId && p.VentaId == venta.Id && p.Activo, ct);
        var totalPagado = pagos.Sum(p => p.Monto) + pago.Monto;
        venta.EstadoPago = CalculateEstadoPago(venta.Total, totalPagado);

        if (venta.EstadoPago == EstadoPagoVenta.Pagado && venta.EstadoVenta == EstadoVenta.Confirmada)
            venta.EstadoVenta = EstadoVenta.Pagada;

        await _ventaRepo.UpdateAsync(venta, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(venta.Id, ct);
    }

    public async Task<Result<VentaDto>> AnularAsync(long id, AnularVentaRequestDto dto, CancellationToken ct = default)
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<VentaDto>.Failure("No se pudo determinar la empresa activa.");

        var empresaId = _currentUser.EmpresaId.Value;
        var venta = await _ventaRepo.GetByIdAsync(id, ct);
        if (venta is null || !venta.Activo)
            return Result<VentaDto>.Failure("Venta no encontrada.");

        if (venta.EmpresaId != empresaId)
            return Result<VentaDto>.Failure("La venta no pertenece a la empresa activa.");

        if (venta.EstadoVenta == EstadoVenta.Anulada)
            return Result<VentaDto>.Failure("La venta ya está anulada.");

        venta.EstadoVenta = EstadoVenta.Anulada;
        venta.EstadoPago = EstadoPagoVenta.Anulado;
        var motivo = dto.MotivoAnulacion?.Trim();
        if (!string.IsNullOrWhiteSpace(motivo))
        {
            venta.Observaciones = string.IsNullOrWhiteSpace(venta.Observaciones)
                ? $"Anulada: {motivo}"
                : $"{venta.Observaciones}\nAnulada: {motivo}";
        }

        await _ventaRepo.UpdateAsync(venta, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(venta.Id, ct);
    }

    private async Task<Result<List<VentaDetalle>>> BuildDetallesAsync(
        IEnumerable<VentaDetalleDto> detalleDtos,
        int empresaId,
        int? defaultAlmacenId,
        CancellationToken ct)
    {
        var detalles = new List<VentaDetalle>();

        foreach (var dto in detalleDtos)
        {
            if (!TryParseEnum(dto.TipoItemVenta, out TipoItemVenta tipoItemVenta))
                return Result<List<VentaDetalle>>.Failure($"TipoItemVenta inválido: '{dto.TipoItemVenta}'.");

            if (dto.Cantidad <= 0)
                return Result<List<VentaDetalle>>.Failure("La cantidad del detalle debe ser mayor a cero.");

            if (dto.PrecioUnitario < 0)
                return Result<List<VentaDetalle>>.Failure("El precio unitario no puede ser negativo.");

            var detalle = new VentaDetalle
            {
                TipoItemVenta = tipoItemVenta,
                Cantidad = dto.Cantidad,
                UnidadMedidaId = dto.UnidadMedidaId,
                PrecioUnitario = dto.PrecioUnitario,
                DescuentoPorcentaje = dto.DescuentoPorcentaje,
                DescuentoMonto = dto.DescuentoMonto,
                ImpuestoMonto = dto.ImpuestoMonto,
                CostoUnitario = dto.CostoUnitario,
                DescuentaInventario = dto.DescuentaInventario,
                Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
                DetalleAdicional = string.IsNullOrWhiteSpace(dto.DetalleAdicional) ? null : dto.DetalleAdicional.Trim()
            };

            if (tipoItemVenta == TipoItemVenta.Producto)
            {
                if (!dto.CompanyProductId.HasValue || dto.CompanyProductId.Value <= 0)
                    return Result<List<VentaDetalle>>.Failure("CompanyProductId es obligatorio para detalles de producto.");

                var companyProduct = await _companyProductRepo.GetByIdAsync(dto.CompanyProductId.Value, ct);
                if (companyProduct is null || companyProduct.EmpresaId != empresaId)
                    return Result<List<VentaDetalle>>.Failure($"El CompanyProduct {dto.CompanyProductId} no existe o no pertenece a la empresa activa.");

                var product = await _productRepo.GetByIdAsync(companyProduct.ProductId, ct);
                if (product is null || product.EmpresaId != empresaId)
                    return Result<List<VentaDetalle>>.Failure($"El Product asociado al CompanyProduct {dto.CompanyProductId} no existe o no pertenece a la empresa activa.");

                if (product.ProductKind == 2)
                    return Result<List<VentaDetalle>>.Failure("No se permiten servicios catalogados en líneas de tipo Producto.");

                if (dto.AlmacenId.HasValue)
                {
                    var almacenValidation = await ValidateAlmacenAsync(dto.AlmacenId.Value, empresaId, ct);
                    if (almacenValidation is not null)
                        return Result<List<VentaDetalle>>.Failure(almacenValidation);
                }

                detalle.CompanyProductId = dto.CompanyProductId.Value;
                detalle.AlmacenId = dto.AlmacenId ?? defaultAlmacenId;
                detalle.DescuentaInventario = dto.DescuentaInventario;
                if (string.IsNullOrWhiteSpace(detalle.Descripcion))
                    detalle.Descripcion = companyProduct.Sku;
            }
            else
            {
                if (dto.CompanyProductId.HasValue && dto.CompanyProductId.Value > 0)
                {
                    var companyProduct = await _companyProductRepo.GetByIdAsync(dto.CompanyProductId.Value, ct);
                    if (companyProduct is null || companyProduct.EmpresaId != empresaId)
                        return Result<List<VentaDetalle>>.Failure($"El CompanyProduct {dto.CompanyProductId} no existe o no pertenece a la empresa activa.");

                    var product = await _productRepo.GetByIdAsync(companyProduct.ProductId, ct);
                    if (product is null || product.EmpresaId != empresaId)
                        return Result<List<VentaDetalle>>.Failure($"El Product asociado al CompanyProduct {dto.CompanyProductId} no existe o no pertenece a la empresa activa.");

                    var esServicioCatalogado = product.ProductKind == 2 || !product.IsStockable;
                    if (!esServicioCatalogado)
                        return Result<List<VentaDetalle>>.Failure("Para TipoItemVenta=Servicio, el CompanyProduct debe ser de tipo Servicio o no stockeable.");

                    if (!product.IsSellable)
                        return Result<List<VentaDetalle>>.Failure("Para TipoItemVenta=Servicio, el CompanyProduct debe estar marcado como vendible.");

                    detalle.CompanyProductId = dto.CompanyProductId.Value;
                    if (string.IsNullOrWhiteSpace(detalle.Descripcion))
                        detalle.Descripcion = product.CommercialName;
                }
                else
                {
                    detalle.CompanyProductId = null;
                }

                detalle.AlmacenId = null;
                detalle.DescuentaInventario = false;
                if (string.IsNullOrWhiteSpace(detalle.Descripcion))
                    return Result<List<VentaDetalle>>.Failure("La descripción es obligatoria para detalles de servicio.");
            }

            var bruto = Round2(detalle.Cantidad * detalle.PrecioUnitario);
            var descuento = detalle.DescuentoMonto;
            if (descuento < 0)
                return Result<List<VentaDetalle>>.Failure("El descuento no puede ser negativo.");

            if (descuento == 0 && detalle.DescuentoPorcentaje > 0)
                descuento = Round2(bruto * (detalle.DescuentoPorcentaje / 100m));

            if (descuento > bruto)
                return Result<List<VentaDetalle>>.Failure("El descuento no puede ser mayor al subtotal bruto de la línea.");

            if (detalle.ImpuestoMonto < 0)
                return Result<List<VentaDetalle>>.Failure("El impuesto no puede ser negativo.");

            detalle.DescuentoMonto = Round2(descuento);
            detalle.ImpuestoMonto = Round2(detalle.ImpuestoMonto);
            detalle.TotalLinea = Round2(bruto - detalle.DescuentoMonto + detalle.ImpuestoMonto);

            detalles.Add(detalle);
        }

        return Result<List<VentaDetalle>>.Success(detalles);
    }

    private async Task<Result<VentaFacturacionDatos?>> BuildFacturacionAsync(
        VentaFacturacionDatosDto? dto,
        int empresaId,
        int? clienteId,
        CancellationToken ct)
    {
        if (dto is null)
            return Result<VentaFacturacionDatos?>.Success(null);

        if (!TryParseEnum(dto.EstadoFactura, out EstadoFacturaVenta estadoFactura))
            estadoFactura = EstadoFacturaVenta.NoGenerada;

        if (dto.Facturar)
        {
            if (string.IsNullOrWhiteSpace(dto.NitFactura))
                return Result<VentaFacturacionDatos?>.Failure("NitFactura es obligatorio cuando Facturar = true.");

            if (string.IsNullOrWhiteSpace(dto.RazonSocialFactura))
                return Result<VentaFacturacionDatos?>.Failure("RazonSocialFactura es obligatorio cuando Facturar = true.");
        }

        if (dto.ClientePerfilFiscalId.HasValue)
        {
            if (!clienteId.HasValue)
                return Result<VentaFacturacionDatos?>.Failure("No se puede usar ClientePerfilFiscalId sin un cliente asociado a la venta.");

            var perfilFiscal = await _clientePerfilFiscalRepo.GetByIdAsync(dto.ClientePerfilFiscalId.Value, ct);
            if (perfilFiscal is null || perfilFiscal.EmpresaId != empresaId || !perfilFiscal.Activo)
                return Result<VentaFacturacionDatos?>.Failure("El perfil fiscal no existe, está inactivo o no pertenece a la empresa activa.");

            if (perfilFiscal.ClienteId != clienteId.Value)
                return Result<VentaFacturacionDatos?>.Failure("El perfil fiscal no pertenece al cliente seleccionado.");

            var factConSnapshotPerfil = new VentaFacturacionDatos
            {
                Facturar = dto.Facturar,
                FacturarAlMismoCliente = dto.FacturarAlMismoCliente,
                ClientePerfilFiscalId = dto.ClientePerfilFiscalId,
                TipoDocumentoIdentidad = perfilFiscal.TipoDocumentoIdentidad,
                NitFactura = perfilFiscal.NumeroDocumento,
                Complemento = perfilFiscal.Complemento,
                RazonSocialFactura = perfilFiscal.RazonSocial,
                EmailFactura = perfilFiscal.EmailFactura,
                TelefonoFactura = perfilFiscal.TelefonoFactura,
                EstadoFactura = estadoFactura,
                FacturaId = dto.FacturaId,
                Activo = true
            };

            return Result<VentaFacturacionDatos?>.Success(factConSnapshotPerfil);
        }

        var fact = new VentaFacturacionDatos
        {
            Facturar = dto.Facturar,
            FacturarAlMismoCliente = dto.FacturarAlMismoCliente,
            ClientePerfilFiscalId = dto.ClientePerfilFiscalId,
            TipoDocumentoIdentidad = dto.TipoDocumentoIdentidad,
            NitFactura = dto.NitFactura,
            Complemento = dto.Complemento,
            RazonSocialFactura = dto.RazonSocialFactura,
            EmailFactura = dto.EmailFactura,
            TelefonoFactura = dto.TelefonoFactura,
            EstadoFactura = estadoFactura,
            FacturaId = dto.FacturaId,
            Activo = true
        };

        return Result<VentaFacturacionDatos?>.Success(fact);
    }

    private Result<List<VentaPago>> BuildPagos(IEnumerable<VentaPagoDto>? pagosDto, int empresaId)
    {
        var pagos = new List<VentaPago>();
        if (pagosDto is null)
            return Result<List<VentaPago>>.Success(pagos);

        foreach (var dto in pagosDto)
        {
            if (dto.Monto <= 0)
                return Result<List<VentaPago>>.Failure("El monto de cada pago inicial debe ser mayor a cero.");

            if (!TryParseEnum(dto.TipoPago, out TipoPago tipoPago))
                return Result<List<VentaPago>>.Failure($"TipoPago inválido: '{dto.TipoPago}'.");

            if (!TryParseEnum(dto.ModoPago, out ModoPago modoPago))
                return Result<List<VentaPago>>.Failure($"ModoPago inválido: '{dto.ModoPago}'.");

            if (!TryParseEnum(dto.EstadoPago, out EstadoPagoVenta estadoPago))
                estadoPago = EstadoPagoVenta.Pendiente;

            pagos.Add(new VentaPago
            {
                EmpresaId = empresaId,
                FechaPago = dto.FechaPago,
                TipoPago = tipoPago,
                ModoPago = modoPago,
                CuentaCajaBancoId = dto.CuentaCajaBancoId,
                Monto = dto.Monto,
                MonedaId = dto.MonedaId,
                MonedaCodigo = dto.MonedaCodigo,
                TipoCambio = dto.TipoCambio <= 0 ? 1m : dto.TipoCambio,
                Referencia = dto.Referencia,
                EstadoPago = estadoPago,
                Activo = true
            });
        }

        return Result<List<VentaPago>>.Success(pagos);
    }

    private void AssignTotales(Venta venta, List<VentaDetalle> detalles)
    {
        var subtotal = detalles.Sum(d => Round2(d.Cantidad * d.PrecioUnitario));
        var descuento = detalles.Sum(d => d.DescuentoMonto);
        var impuesto = detalles.Sum(d => d.ImpuestoMonto);

        venta.Subtotal = Round2(subtotal);
        venta.DescuentoTotal = Round2(descuento);
        venta.ImpuestoTotal = Round2(impuesto);
        venta.Total = Round2(venta.Subtotal - venta.DescuentoTotal + venta.ImpuestoTotal);
    }

    private static EstadoPagoVenta CalculateEstadoPago(decimal totalVenta, decimal totalPagado)
    {
        if (totalPagado <= 0)
            return EstadoPagoVenta.Pendiente;

        if (totalPagado < totalVenta)
            return EstadoPagoVenta.Parcial;

        return EstadoPagoVenta.Pagado;
    }

    private async Task<string?> ValidateSucursalAsync(int sucursalId, int empresaId, CancellationToken ct)
    {
        var sucursal = await _sucursalRepo.GetByIdAsync(sucursalId, ct);
        if (sucursal is null || sucursal.EmpresaId != empresaId)
            return "La sucursal no existe o no pertenece a la empresa activa.";

        return null;
    }

    private async Task<string?> ValidateAlmacenAsync(int almacenId, int empresaId, CancellationToken ct)
    {
        var almacen = await _almacenRepo.GetByIdAsync(almacenId, ct);
        if (almacen is null || almacen.EmpresaId != empresaId)
            return "El almacén no existe o no pertenece a la empresa activa.";

        return null;
    }

    private async Task<string?> ValidateClienteAsync(int clienteId, int empresaId, CancellationToken ct)
    {
        var cliente = await _clienteRepo.GetByIdAsync(clienteId, ct);
        if (cliente is null || cliente.EmpresaId != empresaId)
            return "El cliente no existe o no pertenece a la empresa activa.";

        return null;
    }

    private async Task<string?> ValidatePedidoVentaAsync(long pedidoVentaId, int empresaId, CancellationToken ct)
    {
        var pedido = await _pedidoVentaRepo.GetByIdAsync(pedidoVentaId, ct);
        if (pedido is null || pedido.EmpresaId != empresaId)
            return "El pedido de venta no existe o no pertenece a la empresa activa.";

        return null;
    }

    private async Task<string> GenerateNumeroVentaAsync(int empresaId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var prefix = $"VTA-{now:yyyyMMdd}";
        var existing = await _ventaRepo.FindAsync(v => v.EmpresaId == empresaId && v.NumeroVenta.StartsWith(prefix), ct);
        return $"{prefix}-{(existing.Count + 1):D4}";
    }

    private static bool TryParseEnum<TEnum>(string? value, out TEnum parsed)
        where TEnum : struct, Enum
    {
        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse<TEnum>(value, true, out parsed))
            return true;

        parsed = default;
        return false;
    }

    private static decimal Round2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static VentaDto MapToVentaDto(
        Venta venta,
        IReadOnlyList<VentaDetalle> detalles,
        VentaFacturacionDatos? fact,
        IReadOnlyList<VentaPago> pagos)
    {
        return new VentaDto
        {
            Id = venta.Id,
            SucursalId = venta.SucursalId,
            AlmacenId = venta.AlmacenId,
            ClienteId = venta.ClienteId,
            PedidoVentaId = venta.PedidoVentaId,
            NumeroVenta = venta.NumeroVenta,
            FechaVenta = venta.FechaVenta,
            TipoVenta = venta.TipoVenta.ToString(),
            EstadoVenta = venta.EstadoVenta.ToString(),
            EstadoPago = venta.EstadoPago.ToString(),
            MonedaId = venta.MonedaId,
            MonedaCodigo = venta.MonedaCodigo,
            TipoCambio = venta.TipoCambio,
            Subtotal = venta.Subtotal,
            DescuentoTotal = venta.DescuentoTotal,
            ImpuestoTotal = venta.ImpuestoTotal,
            Total = venta.Total,
            Observaciones = venta.Observaciones,
            FacturaGenerada = venta.FacturaGenerada,
            InventarioDescontado = venta.InventarioDescontado,
            FacturacionDatos = fact is null
                ? null
                : new VentaFacturacionDatosDto
                {
                    Facturar = fact.Facturar,
                    FacturarAlMismoCliente = fact.FacturarAlMismoCliente,
                    ClientePerfilFiscalId = fact.ClientePerfilFiscalId,
                    TipoDocumentoIdentidad = fact.TipoDocumentoIdentidad,
                    NitFactura = fact.NitFactura,
                    Complemento = fact.Complemento,
                    RazonSocialFactura = fact.RazonSocialFactura,
                    EmailFactura = fact.EmailFactura,
                    TelefonoFactura = fact.TelefonoFactura,
                    EstadoFactura = fact.EstadoFactura.ToString(),
                    FacturaId = fact.FacturaId
                },
            Detalles = detalles.Select(d => new VentaDetalleDto
            {
                Id = d.Id,
                TipoItemVenta = d.TipoItemVenta.ToString(),
                CompanyProductId = d.CompanyProductId,
                AlmacenId = d.AlmacenId,
                Descripcion = d.Descripcion,
                DetalleAdicional = d.DetalleAdicional,
                Cantidad = d.Cantidad,
                UnidadMedidaId = d.UnidadMedidaId,
                PrecioUnitario = d.PrecioUnitario,
                DescuentoPorcentaje = d.DescuentoPorcentaje,
                DescuentoMonto = d.DescuentoMonto,
                ImpuestoMonto = d.ImpuestoMonto,
                TotalLinea = d.TotalLinea,
                CostoUnitario = d.CostoUnitario,
                DescuentaInventario = d.DescuentaInventario
            }).ToList(),
            Pagos = pagos.Select(p => new VentaPagoDto
            {
                FechaPago = p.FechaPago,
                TipoPago = p.TipoPago.ToString(),
                ModoPago = p.ModoPago.ToString(),
                CuentaCajaBancoId = p.CuentaCajaBancoId,
                Monto = p.Monto,
                MonedaId = p.MonedaId,
                MonedaCodigo = p.MonedaCodigo,
                TipoCambio = p.TipoCambio,
                Referencia = p.Referencia,
                EstadoPago = p.EstadoPago.ToString()
            }).ToList()
        };
    }
}
