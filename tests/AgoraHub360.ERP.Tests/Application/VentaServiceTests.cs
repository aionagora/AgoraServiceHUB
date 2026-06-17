namespace AgoraHub360.ERP.Tests.Application;

using System.Linq.Expressions;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Application.Services;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Entities.CXC;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;
using AgoraHub360.ERP.Shared.DTOs.CxC;
using AgoraHub360.ERP.Shared.DTOs.Numeracion;

public class VentaServiceTests
{
    private readonly VentaService _sut;
    private readonly FakeRepo<Venta> _ventaRepo;
    private readonly FakeRepo<VentaDetalle> _detalleRepo;
    private readonly FakeRepo<VentaFacturacionDatos> _facturacionRepo;
    private readonly FakeRepo<VentaPago> _pagoRepo;
    private readonly FakeRepo<Sucursal> _sucursalRepo;
    private readonly FakeRepo<Almacen> _almacenRepo;
    private readonly FakeRepo<Cliente> _clienteRepo;
    private readonly FakeRepo<ClientePerfilFiscal> _clientePerfilFiscalRepo;
    private readonly FakeRepo<PedidoVenta> _pedidoVentaRepo;
    private readonly FakeRepo<PedidoVentaDetalle> _pedidoVentaDetalleRepo;
    private readonly FakeRepo<CompanyProduct> _companyProductRepo;
    private readonly FakeRepo<Product> _productRepo;
    private readonly FakeRepo<Empresa> _empresaRepo;
    private readonly FakeNumeracionService _numeracionService;
    private readonly FakeCurrentUserService _currentUserService;
    private readonly FakeUow _uow;
    private readonly FakeCxcService _cxcService;
    private readonly FakeRepo<FacturaVenta> _facturaRepo;

    public VentaServiceTests()
    {
        _ventaRepo = new FakeRepo<Venta>();
        _detalleRepo = new FakeRepo<VentaDetalle>();
        _facturacionRepo = new FakeRepo<VentaFacturacionDatos>();
        _pagoRepo = new FakeRepo<VentaPago>();
        _sucursalRepo = new FakeRepo<Sucursal>();
        _almacenRepo = new FakeRepo<Almacen>();
        _clienteRepo = new FakeRepo<Cliente>();
        _clientePerfilFiscalRepo = new FakeRepo<ClientePerfilFiscal>();
        _pedidoVentaRepo = new FakeRepo<PedidoVenta>();
        _pedidoVentaDetalleRepo = new FakeRepo<PedidoVentaDetalle>();
        _companyProductRepo = new FakeRepo<CompanyProduct>();
        _productRepo = new FakeRepo<Product>();
        _empresaRepo = new FakeRepo<Empresa>();
        _numeracionService = new FakeNumeracionService();
        _currentUserService = new FakeCurrentUserService { EmpresaId = 1, UserName = "testuser" };
        _uow = new FakeUow();
        _cxcService = new FakeCxcService();
        _facturaRepo = new FakeRepo<FacturaVenta>();

        _sut = new VentaService(
            _ventaRepo,
            _detalleRepo,
            _facturacionRepo,
            _pagoRepo,
            _sucursalRepo,
            _almacenRepo,
            _clienteRepo,
            _clientePerfilFiscalRepo,
            _pedidoVentaRepo,
            _pedidoVentaDetalleRepo,
            _companyProductRepo,
            _productRepo,
            _empresaRepo,
            _numeracionService,
            _currentUserService,
            _uow,
            _cxcService,
            _facturaRepo);
    }

    [Fact]
    public async Task AnularAsync_SaleWithActivePayment_ReturnsFailure()
    {
        // Arrange
        _ventaRepo.Seed(new Venta
        {
            Id = 10,
            EmpresaId = 1,
            Total = 1000m,
            EstadoVenta = EstadoVenta.Confirmada,
            EstadoPago = EstadoPagoVenta.Parcial,
            Activo = true
        });

        _pagoRepo.Seed(new VentaPago
        {
            Id = 1,
            VentaId = 10,
            EmpresaId = 1,
            Monto = 500m,
            Anulado = false,
            Activo = true
        });

        // Act
        var result = await _sut.AnularAsync(10, new AnularVentaRequestDto { MotivoAnulacion = "Cancelada por cliente" });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("tiene pagos registrados", result.Error);
    }

    [Fact]
    public async Task AnularAsync_SaleWithNoActivePayments_Succeeds()
    {
        // Arrange
        _ventaRepo.Seed(new Venta
        {
            Id = 10,
            EmpresaId = 1,
            Total = 1000m,
            EstadoVenta = EstadoVenta.Confirmada,
            EstadoPago = EstadoPagoVenta.Pendiente,
            Activo = true
        });

        // Act
        var result = await _sut.AnularAsync(10, new AnularVentaRequestDto { MotivoAnulacion = "Cancelada" });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(EstadoVenta.Anulada.ToString(), result.Value!.EstadoVenta);
        Assert.Equal(EstadoPagoVenta.Anulado.ToString(), result.Value.EstadoPago);
    }

    [Fact]
    public async Task AnularPagoAsync_ValidPayment_LogicallyCancelsPaymentAndUpdatesStates()
    {
        // Arrange
        _ventaRepo.Seed(new Venta
        {
            Id = 10,
            EmpresaId = 1,
            Total = 1000m,
            EstadoVenta = EstadoVenta.Pagada,
            EstadoPago = EstadoPagoVenta.Pagado,
            Activo = true
        });

        var pago = new VentaPago
        {
            Id = 100,
            VentaId = 10,
            EmpresaId = 1,
            Monto = 1000m,
            Anulado = false,
            Activo = true
        };
        _pagoRepo.Seed(pago);

        // Act
        var result = await _sut.AnularPagoAsync(100, new AnularPagoVentaRequestDto { Motivo = "Error en cobro" });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(pago.Anulado);
        Assert.Equal("Error en cobro", pago.MotivoAnulacion);
        Assert.Equal(EstadoVenta.Confirmada.ToString(), result.Value!.EstadoVenta);
        Assert.Equal(EstadoPagoVenta.Pendiente.ToString(), result.Value.EstadoPago);
        Assert.True(_cxcService.ActualizarPorPagoVentaCalled);
        Assert.Equal(10, _cxcService.LastVentaId);
    }

    // ── Fakes ──

    private class FakeRepo<T> : IRepository<T> where T : class
    {
        private readonly List<T> _store = new();

        public void Seed(T p) => _store.Add(p);

        public Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var result = _store.FirstOrDefault(p => {
                var prop = p.GetType().GetProperty("Id");
                if (prop == null) return false;
                var val = prop.GetValue(p);
                return val != null && Convert.ToInt32(val) == id;
            });
            return Task.FromResult(result);
        }

        public Task<T?> GetByIdAsync(long id, CancellationToken ct = default)
            => GetByIdAsync((int)id, ct);

        public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<T>>(_store.AsReadOnly());

        public Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<T>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<IReadOnlyList<T>> FindIgnoreQueryFiltersAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<T>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<T?> GetByIdIgnoreQueryFiltersAsync(int id, CancellationToken ct = default)
            => GetByIdAsync(id, ct);

        public Task<T> AddAsync(T entity, CancellationToken ct = default)
        {
            _store.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(T entity, CancellationToken ct = default) => Task.CompletedTask;

        public Task DeleteAsync(T entity, CancellationToken ct = default)
        {
            _store.Remove(entity);
            return Task.CompletedTask;
        }
    }

    private class FakeUow : IUnitOfWork
    {
        public bool SaveCalled { get; private set; }
        public Task<int> SaveChangesAsync(CancellationToken ct = default) { SaveCalled = true; return Task.FromResult(1); }
        public Task BeginTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task ExecuteInTransactionAsync(Func<Task> op, CancellationToken ct = default) => op();
        public Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> op, CancellationToken ct = default) => op();
        public void Dispose() { }
    }

    private class FakeCurrentUserService : ICurrentUserService
    {
        public string? UserId { get; set; }
        public int? UserIdInt { get; set; }
        public string? UserName { get; set; }
        public int? EmpresaId { get; set; }
        public int? TenantId { get; set; }
        public string PlatformRole { get; set; } = string.Empty;
        public string TenantRole { get; set; } = string.Empty;
        public string TenantStatus { get; set; } = string.Empty;
        public bool IsInRole(string role) => false;
    }

    private class FakeNumeracionService : INumeracionDocumentoService
    {
        public Task<Result<IReadOnlyList<NumeracionDocumentoDto>>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Result<IReadOnlyList<NumeracionDocumentoDto>>.Success(new List<NumeracionDocumentoDto>().AsReadOnly()));

        public Task<Result<NumeracionDocumentoDto>> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(Result<NumeracionDocumentoDto>.Success(new NumeracionDocumentoDto()));

        public Task<Result<NumeracionDocumentoDto>> CreateAsync(CrearNumeracionDocumentoRequestDto dto, CancellationToken ct = default)
            => Task.FromResult(Result<NumeracionDocumentoDto>.Success(new NumeracionDocumentoDto()));

        public Task<Result<NumeracionDocumentoDto>> UpdateAsync(int id, ActualizarNumeracionDocumentoRequestDto dto, CancellationToken ct = default)
            => Task.FromResult(Result<NumeracionDocumentoDto>.Success(new NumeracionDocumentoDto()));

        public Task<Result<bool>> ActivarAsync(int id, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Success(true));

        public Task<Result<bool>> DesactivarAsync(int id, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Success(true));

        public Task<Result<string>> GenerarSiguienteNumeroAsync(string tipoDocumento, long sucursalId, CancellationToken ct = default)
            => Task.FromResult(Result<string>.Success("VTA-001"));
    }

    private class FakeCxcService : ICuentasPorCobrarService
    {
        public bool ActualizarPorPagoVentaCalled { get; private set; }
        public long LastVentaId { get; private set; }

        public Task<Result<CuentaPorCobrarResumenDto>> ActualizarPorPagoVentaAsync(long ventaId, CancellationToken ct = default)
        {
            ActualizarPorPagoVentaCalled = true;
            LastVentaId = ventaId;
            return Task.FromResult(Result<CuentaPorCobrarResumenDto>.Success(new CuentaPorCobrarResumenDto()));
        }

        public Task<Result<CuentaPorCobrarResumenDto>> ActualizarPorPagoAsync(long facturaVentaId, decimal montoPagado, CancellationToken ct = default)
            => Task.FromResult(Result<CuentaPorCobrarResumenDto>.Success(new CuentaPorCobrarResumenDto()));

        public Task<Result<CuentaPorCobrarResumenDto>> GenerarDesdeFacturaAsync(long facturaVentaId, CancellationToken ct = default)
            => Task.FromResult(Result<CuentaPorCobrarResumenDto>.Success(new CuentaPorCobrarResumenDto()));

        public Task<Result<CuentaPorCobrarResumenDto>> GenerarDesdeVentaAsync(long ventaId, CancellationToken ct = default)
            => Task.FromResult(Result<CuentaPorCobrarResumenDto>.Success(new CuentaPorCobrarResumenDto()));

        public Task<Result<PaginatedResultDto<CuentaPorCobrarResumenDto>>> GetAllAsync(CuentaPorCobrarFilterDto? filter = null, CancellationToken ct = default)
            => Task.FromResult(Result<PaginatedResultDto<CuentaPorCobrarResumenDto>>.Success(new PaginatedResultDto<CuentaPorCobrarResumenDto>()));

        public Task<Result<CuentaPorCobrarDetalleDto>> GetByIdAsync(long id, CancellationToken ct = default)
            => Task.FromResult(Result<CuentaPorCobrarDetalleDto>.Success(new CuentaPorCobrarDetalleDto()));

        public Task<Result<decimal>> GetSaldoTotalPendienteAsync(CancellationToken ct = default)
            => Task.FromResult(Result<decimal>.Success(0m));

        public Task<Result<AntiguedadSaldosResumenDto>> GetAntiguedadSaldosAsync(CancellationToken ct = default)
            => Task.FromResult(Result<AntiguedadSaldosResumenDto>.Success(new AntiguedadSaldosResumenDto()));

        public Task<Result<bool>> AnularAsync(long id, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Success(true));
    }
}
