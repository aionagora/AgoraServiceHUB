namespace AgoraHub360.ERP.Tests.Application;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Application.Services;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Entities.CXC;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;
using AgoraHub360.ERP.Shared.DTOs.CxC;
using Xunit;

public class FacturaVentaServiceTests
{
    private readonly FacturaVentaService _sut;
    private readonly FakeRepo<FacturaVenta> _facturaRepo;
    private readonly FakeRepo<FacturaVentaDetalle> _facturaDetalleRepo;
    private readonly FakeRepo<Venta> _ventaRepo;
    private readonly FakeRepo<VentaDetalle> _ventaDetalleRepo;
    private readonly FakeRepo<VentaFacturacionDatos> _ventaFacturacionRepo;
    private readonly FakeRepo<VentaPago> _ventaPagoRepo;
    private readonly FakeRepo<SiatMetodoPago> _siatMetodoPagoRepo;
    private readonly FakeRepo<Cliente> _clienteRepo;
    private readonly FakeRepo<ClientePerfilFiscal> _clientePerfilFiscalRepo;
    private readonly FakeRepo<CompanyProduct> _companyProductRepo;
    private readonly FakeRepo<Uom> _uomRepo;
    private readonly FakeCurrentUserService _currentUserService;
    private readonly FakeUow _uow;
    private readonly FakeCxcService _cxcService;
    private readonly FakeRepo<CuentaPorCobrar> _cxcRepo;

    public FacturaVentaServiceTests()
    {
        _facturaRepo = new FakeRepo<FacturaVenta>();
        _facturaDetalleRepo = new FakeRepo<FacturaVentaDetalle>();
        _ventaRepo = new FakeRepo<Venta>();
        _ventaDetalleRepo = new FakeRepo<VentaDetalle>();
        _ventaFacturacionRepo = new FakeRepo<VentaFacturacionDatos>();
        _ventaPagoRepo = new FakeRepo<VentaPago>();
        _siatMetodoPagoRepo = new FakeRepo<SiatMetodoPago>();
        _clienteRepo = new FakeRepo<Cliente>();
        _clientePerfilFiscalRepo = new FakeRepo<ClientePerfilFiscal>();
        _companyProductRepo = new FakeRepo<CompanyProduct>();
        _uomRepo = new FakeRepo<Uom>();
        _currentUserService = new FakeCurrentUserService { EmpresaId = 1, UserName = "testuser" };
        _uow = new FakeUow();
        _cxcService = new FakeCxcService();
        _cxcRepo = new FakeRepo<CuentaPorCobrar>();

        _sut = new FacturaVentaService(
            _facturaRepo,
            _facturaDetalleRepo,
            _ventaRepo,
            _ventaDetalleRepo,
            _ventaFacturacionRepo,
            _ventaPagoRepo,
            _siatMetodoPagoRepo,
            _clienteRepo,
            _clientePerfilFiscalRepo,
            _companyProductRepo,
            _uomRepo,
            _currentUserService,
            _uow,
            _cxcService,
            _cxcRepo
        );
    }

    [Fact]
    public async Task GetAllAsync_PopulatesCollectionFields_WhenAssociatedCxcExists()
    {
        // Arrange
        var today = DateTime.Today;
        _facturaRepo.Seed(new FacturaVenta
        {
            Id = 100,
            EmpresaId = 1,
            NumeroFactura = "FAC-001",
            FechaEmision = today,
            Total = 1000m,
            EstadoFactura = EstadoFacturaVentaComercial.Generada,
            Activo = true
        });

        _cxcRepo.Seed(new CuentaPorCobrar
        {
            Id = 1,
            EmpresaId = 1,
            FacturaVentaId = 100,
            TotalFactura = 1000m,
            TotalPagado = 300m,
            FechaVencimiento = today.AddDays(-10), // Overdue
            Estado = EstadoCuentaPorCobrar.Parcial,
            Activo = true
        });

        // Act
        var result = await _sut.GetAllAsync(new FacturaVentaFilterDto { Top = 10 });

        // Assert
        Assert.True(result.IsSuccess);
        var items = result.Value.Items;
        Assert.Single(items);
        var item = items.First();
        Assert.Equal("Parcial", item.EstadoCobro);
        Assert.Equal(300m, item.TotalPagado);
        Assert.Equal(700m, item.SaldoPendiente);
        Assert.Equal(today.AddDays(-10), item.FechaVencimiento);
        Assert.Equal(10, item.DiasVencidos);
    }

    [Fact]
    public async Task GetByIdAsync_PopulatesCollectionFields_WhenCxcDoesNotExist()
    {
        // Arrange
        var today = DateTime.Today;
        _facturaRepo.Seed(new FacturaVenta
        {
            Id = 200,
            EmpresaId = 1,
            NumeroFactura = "FAC-002",
            FechaEmision = today,
            Total = 1500m,
            EstadoFactura = EstadoFacturaVentaComercial.Generada,
            Activo = true
        });

        // Act
        var result = await _sut.GetByIdAsync(200);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.Value;
        Assert.Equal("Pendiente", dto.EstadoCobro);
        Assert.Equal(0m, dto.TotalPagado);
        Assert.Equal(1500m, dto.SaldoPendiente);
        Assert.Null(dto.FechaVencimiento);
        Assert.Equal(0, dto.DiasVencidos);
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

    private class FakeCxcService : ICuentasPorCobrarService
    {
        public Task<Result<CuentaPorCobrarResumenDto>> ActualizarPorPagoVentaAsync(long ventaId, CancellationToken ct = default)
            => Task.FromResult(Result<CuentaPorCobrarResumenDto>.Success(new CuentaPorCobrarResumenDto()));

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
