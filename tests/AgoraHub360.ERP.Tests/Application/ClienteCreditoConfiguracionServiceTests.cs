namespace AgoraHub360.ERP.Tests.Application;

using System.Linq.Expressions;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Application.Services;
using AgoraHub360.ERP.Domain.Entities.CXC;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.CxC;

public class ClienteCreditoConfiguracionServiceTests
{
    private readonly ClienteCreditoConfiguracionService _sut;
    private readonly FakeRepo<ClienteCreditoConfiguracion> _configRepo;
    private readonly FakeRepo<Cliente> _clienteRepo;
    private readonly FakeUow _uow;
    private readonly FakeCurrentUserService _currentUserService;

    public ClienteCreditoConfiguracionServiceTests()
    {
        _configRepo = new FakeRepo<ClienteCreditoConfiguracion>();
        _clienteRepo = new FakeRepo<Cliente>();
        _uow = new FakeUow();
        _currentUserService = new FakeCurrentUserService { EmpresaId = 1 };
        _sut = new ClienteCreditoConfiguracionService(_configRepo, _clienteRepo, _currentUserService, _uow);
    }

    [Fact]
    public async Task GetByClienteAsync_ClienteNoExiste_Fails()
    {
        var result = await _sut.GetByClienteAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Contains("no existe", result.Error);
    }

    [Fact]
    public async Task GetByClienteAsync_ClienteInactivo_Fails()
    {
        _clienteRepo.Seed(new Cliente { Id = 10, Activo = false, EmpresaId = 1 });

        var result = await _sut.GetByClienteAsync(10);

        Assert.False(result.IsSuccess);
        Assert.Contains("no existe o no pertenece", result.Error);
    }

    [Fact]
    public async Task GetByClienteAsync_ClientePerteneceAOtraEmpresa_Fails()
    {
        _clienteRepo.Seed(new Cliente { Id = 10, Activo = true, EmpresaId = 2 }); // Otra empresa

        var result = await _sut.GetByClienteAsync(10);

        Assert.False(result.IsSuccess);
        Assert.Contains("no pertenece a la empresa activa", result.Error);
    }

    [Fact]
    public async Task GetByClienteAsync_NoConfigExists_ReturnsDefaultValues()
    {
        _clienteRepo.Seed(new Cliente { Id = 10, Activo = true, EmpresaId = 1 });

        var result = await _sut.GetByClienteAsync(10);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.CreditoHabilitado);
        Assert.Equal(0, result.Value.DiasCredito);
        Assert.Equal(0, result.Value.LimiteCredito);
        Assert.Empty(result.Value.Observaciones);
    }

    [Fact]
    public async Task GetByClienteAsync_ConfigExists_ReturnsConfigValues()
    {
        _clienteRepo.Seed(new Cliente { Id = 10, Activo = true, EmpresaId = 1 });
        _configRepo.Seed(new ClienteCreditoConfiguracion
        {
            Id = 1,
            ClienteId = 10,
            EmpresaId = 1,
            CreditoHabilitado = true,
            DiasCredito = 15,
            LimiteCredito = 5000,
            Observaciones = "Test Obs",
            Activo = true
        });

        var result = await _sut.GetByClienteAsync(10);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.CreditoHabilitado);
        Assert.Equal(15, result.Value.DiasCredito);
        Assert.Equal(5000, result.Value.LimiteCredito);
        Assert.Equal("Test Obs", result.Value.Observaciones);
    }

    [Fact]
    public async Task GuardarAsync_DiasCreditoNegativo_Fails()
    {
        _clienteRepo.Seed(new Cliente { Id = 10, Activo = true, EmpresaId = 1 });
        var req = new GuardarClienteCreditoConfiguracionRequestDto
        {
            CreditoHabilitado = true,
            DiasCredito = -5,
            LimiteCredito = 1000
        };

        var result = await _sut.GuardarAsync(10, req);

        Assert.False(result.IsSuccess);
        Assert.Contains("negativos", result.Error);
    }

    [Fact]
    public async Task GuardarAsync_LimiteCreditoNegativo_Fails()
    {
        _clienteRepo.Seed(new Cliente { Id = 10, Activo = true, EmpresaId = 1 });
        var req = new GuardarClienteCreditoConfiguracionRequestDto
        {
            CreditoHabilitado = true,
            DiasCredito = 10,
            LimiteCredito = -100
        };

        var result = await _sut.GuardarAsync(10, req);

        Assert.False(result.IsSuccess);
        Assert.Contains("negativo", result.Error);
    }

    [Fact]
    public async Task GuardarAsync_NewConfig_CreatesConfig()
    {
        _clienteRepo.Seed(new Cliente { Id = 10, Activo = true, EmpresaId = 1 });
        var req = new GuardarClienteCreditoConfiguracionRequestDto
        {
            CreditoHabilitado = true,
            DiasCredito = 30,
            LimiteCredito = 10000,
            Observaciones = "Nuevas observaciones"
        };

        var result = await _sut.GuardarAsync(10, req);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.CreditoHabilitado);
        Assert.Equal(30, result.Value.DiasCredito);
        Assert.Equal(10000, result.Value.LimiteCredito);
        Assert.Equal("Nuevas observaciones", result.Value.Observaciones);
        Assert.True(_uow.SaveCalled);
    }

    [Fact]
    public async Task GuardarAsync_ExistingConfig_UpdatesConfig()
    {
        _clienteRepo.Seed(new Cliente { Id = 10, Activo = true, EmpresaId = 1 });
        var existing = new ClienteCreditoConfiguracion
        {
            Id = 1,
            ClienteId = 10,
            EmpresaId = 1,
            CreditoHabilitado = false,
            DiasCredito = 0,
            LimiteCredito = 0,
            Observaciones = "Viejas",
            Activo = true
        };
        _configRepo.Seed(existing);

        var req = new GuardarClienteCreditoConfiguracionRequestDto
        {
            CreditoHabilitado = true,
            DiasCredito = 15,
            LimiteCredito = 500,
            Observaciones = "Nuevas"
        };

        var result = await _sut.GuardarAsync(10, req);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.CreditoHabilitado);
        Assert.Equal(15, result.Value.DiasCredito);
        Assert.Equal(500, result.Value.LimiteCredito);
        Assert.Equal("Nuevas", result.Value.Observaciones);
        Assert.True(_uow.SaveCalled);
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
