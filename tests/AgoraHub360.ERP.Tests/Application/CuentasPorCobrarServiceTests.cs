namespace AgoraHub360.ERP.Tests.Application;

using System.Linq.Expressions;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Application.Services;
using AgoraHub360.ERP.Domain.Entities.CXC;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Enums;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.CxC;

public class CuentasPorCobrarServiceTests
{
    private readonly CuentasPorCobrarService _sut;
    private readonly FakeRepo<CuentaPorCobrar> _cxcRepo;
    private readonly FakeRepo<FacturaVenta> _facturaRepo;
    private readonly FakeRepo<VentaPago> _pagoRepo;
    private readonly FakeRepo<Venta> _ventaRepo;
    private readonly FakeRepo<Cliente> _clienteRepo;
    private readonly FakeUow _uow;
    private readonly FakeCurrentUserService _currentUserService;

    public CuentasPorCobrarServiceTests()
    {
        _cxcRepo = new FakeRepo<CuentaPorCobrar>();
        _facturaRepo = new FakeRepo<FacturaVenta>();
        _pagoRepo = new FakeRepo<VentaPago>();
        _ventaRepo = new FakeRepo<Venta>();
        _clienteRepo = new FakeRepo<Cliente>();
        _uow = new FakeUow();
        _currentUserService = new FakeCurrentUserService { EmpresaId = 1 };

        _sut = new CuentasPorCobrarService(
            _cxcRepo,
            _facturaRepo,
            _pagoRepo,
            _ventaRepo,
            _clienteRepo,
            _currentUserService,
            _uow);
    }

    [Fact]
    public async Task GetAntiguedadSaldosAsync_NoEmpresa_Fails()
    {
        _currentUserService.EmpresaId = null;

        var result = await _sut.GetAntiguedadSaldosAsync();

        Assert.False(result.IsSuccess);
        Assert.Contains("empresa activa", result.Error);
    }

    [Fact]
    public async Task GetAntiguedadSaldosAsync_GroupsAndBucketsCorrectly()
    {
        var today = DateTime.Today;

        // Cliente 1
        _cxcRepo.Seed(new CuentaPorCobrar
        {
            Id = 1,
            EmpresaId = 1,
            Activo = true,
            ClienteId = 101,
            ClienteNombre = "Cliente A",
            ClienteNit = "111",
            TotalFactura = 1000m,
            TotalPagado = 0m,
            FechaVencimiento = today.AddDays(5), // No Vencido
            Estado = EstadoCuentaPorCobrar.Pendiente
        });

        _cxcRepo.Seed(new CuentaPorCobrar
        {
            Id = 2,
            EmpresaId = 1,
            Activo = true,
            ClienteId = 101,
            ClienteNombre = "Cliente A",
            ClienteNit = "111",
            TotalFactura = 500m,
            TotalPagado = 0m,
            FechaVencimiento = today.AddDays(-15), // 1–30 Días
            Estado = EstadoCuentaPorCobrar.Pendiente
        });

        // Cliente 2
        _cxcRepo.Seed(new CuentaPorCobrar
        {
            Id = 3,
            EmpresaId = 1,
            Activo = true,
            ClienteId = 102,
            ClienteNombre = "Cliente B",
            ClienteNit = "222",
            TotalFactura = 700m,
            TotalPagado = 0m,
            FechaVencimiento = today.AddDays(-45), // 31–60 Días
            Estado = EstadoCuentaPorCobrar.Pendiente
        });

        _cxcRepo.Seed(new CuentaPorCobrar
        {
            Id = 4,
            EmpresaId = 1,
            Activo = true,
            ClienteId = 102,
            ClienteNombre = "Cliente B",
            ClienteNit = "222",
            TotalFactura = 600m,
            TotalPagado = 0m,
            FechaVencimiento = today.AddDays(-75), // 61–90 Días
            Estado = EstadoCuentaPorCobrar.Pendiente
        });

        // Cliente 3
        _cxcRepo.Seed(new CuentaPorCobrar
        {
            Id = 5,
            EmpresaId = 1,
            Activo = true,
            ClienteId = 103,
            ClienteNombre = "Cliente C",
            ClienteNit = "333",
            TotalFactura = 900m,
            TotalPagado = 0m,
            FechaVencimiento = today.AddDays(-100), // +90 Días
            Estado = EstadoCuentaPorCobrar.Pendiente
        });

        // Exclusiones:
        // Pagada
        _cxcRepo.Seed(new CuentaPorCobrar
        {
            Id = 6,
            EmpresaId = 1,
            Activo = true,
            ClienteId = 101,
            ClienteNombre = "Cliente A",
            TotalFactura = 100m,
            TotalPagado = 100m, // Pagada
            FechaVencimiento = today.AddDays(-10),
            Estado = EstadoCuentaPorCobrar.Pagada
        });

        // Anulada
        _cxcRepo.Seed(new CuentaPorCobrar
        {
            Id = 7,
            EmpresaId = 1,
            Activo = true,
            ClienteId = 101,
            ClienteNombre = "Cliente A",
            TotalFactura = 200m,
            TotalPagado = 0m,
            FechaVencimiento = today.AddDays(-20),
            Estado = EstadoCuentaPorCobrar.Anulada
        });

        // Inactiva
        _cxcRepo.Seed(new CuentaPorCobrar
        {
            Id = 8,
            EmpresaId = 1,
            Activo = false, // Inactiva
            ClienteId = 101,
            ClienteNombre = "Cliente A",
            TotalFactura = 300m,
            TotalPagado = 0m,
            FechaVencimiento = today.AddDays(-5),
            Estado = EstadoCuentaPorCobrar.Pendiente
        });

        // Saldo <= 0
        _cxcRepo.Seed(new CuentaPorCobrar
        {
            Id = 9,
            EmpresaId = 1,
            Activo = true,
            ClienteId = 101,
            ClienteNombre = "Cliente A",
            TotalFactura = 0m, // Saldo = 0
            TotalPagado = 0m,
            FechaVencimiento = today.AddDays(-5),
            Estado = EstadoCuentaPorCobrar.Pendiente
        });

        var result = await _sut.GetAntiguedadSaldosAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Clientes.Count);

        // Totales generales
        Assert.Equal(1000m, result.Value.TotalNoVencido);
        Assert.Equal(500m, result.Value.TotalVencido1A30);
        Assert.Equal(700m, result.Value.TotalVencido31A60);
        Assert.Equal(600m, result.Value.TotalVencido61A90);
        Assert.Equal(900m, result.Value.TotalVencidoMas90);
        Assert.Equal(3700m, result.Value.TotalGeneral);

        // Cliente A
        var clientA = result.Value.Clientes.First(c => c.ClienteId == 101);
        Assert.Equal(1000m, clientA.NoVencido);
        Assert.Equal(500m, clientA.Vencido1A30);
        Assert.Equal(0m, clientA.Vencido31A60);
        Assert.Equal(0m, clientA.Vencido61A90);
        Assert.Equal(0m, clientA.VencidoMas90);
        Assert.Equal(1500m, clientA.Total);

        // Cliente B
        var clientB = result.Value.Clientes.First(c => c.ClienteId == 102);
        Assert.Equal(0m, clientB.NoVencido);
        Assert.Equal(0m, clientB.Vencido1A30);
        Assert.Equal(700m, clientB.Vencido31A60);
        Assert.Equal(600m, clientB.Vencido61A90);
        Assert.Equal(0m, clientB.VencidoMas90);
        Assert.Equal(1300m, clientB.Total);

        // Cliente C
        var clientC = result.Value.Clientes.First(c => c.ClienteId == 103);
        Assert.Equal(0m, clientC.NoVencido);
        Assert.Equal(0m, clientC.Vencido1A30);
        Assert.Equal(0m, clientC.Vencido31A60);
        Assert.Equal(0m, clientC.Vencido61A90);
        Assert.Equal(900m, clientC.VencidoMas90);
        Assert.Equal(900m, clientC.Total);
    }

    [Fact]
    public async Task ActualizarPorPagoVentaAsync_RecalculatesAndUpdatesCxC()
    {
        var today = DateTime.Today;

        // Seed Venta
        _ventaRepo.Seed(new Venta
        {
            Id = 50,
            EmpresaId = 1,
            Total = 1000m,
            Activo = true
        });

        // Seed FacturaVenta
        _facturaRepo.Seed(new FacturaVenta
        {
            Id = 60,
            VentaId = 50,
            EmpresaId = 1,
            Total = 1000m,
            Activo = true
        });

        // Seed CuentaPorCobrar
        var cxc = new CuentaPorCobrar
        {
            Id = 70,
            FacturaVentaId = 60,
            EmpresaId = 1,
            TotalFactura = 1000m,
            TotalPagado = 0m,
            FechaVencimiento = today.AddDays(10),
            Estado = EstadoCuentaPorCobrar.Pendiente,
            Activo = true
        };
        _cxcRepo.Seed(cxc);

        // Seed VentaPago
        _pagoRepo.Seed(new VentaPago
        {
            Id = 1,
            VentaId = 50,
            Monto = 400m,
            EmpresaId = 1,
            Activo = true
        });

        _pagoRepo.Seed(new VentaPago
        {
            Id = 2,
            VentaId = 50,
            Monto = 600m,
            EmpresaId = 1,
            Activo = true
        });

        var result = await _sut.ActualizarPorPagoVentaAsync(50);

        Assert.True(result.IsSuccess);
        Assert.Equal(1000m, cxc.TotalPagado);
        Assert.Equal(EstadoCuentaPorCobrar.Pagada, cxc.Estado);
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
}
