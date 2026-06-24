namespace AgoraHub360.ERP.Tests.Application;

using System.Linq.Expressions;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Application.Services;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Parametro;

public class ParametroSistemaServiceTests
{
    private readonly ParametroSistemaService _sut;
    private readonly FakeParamRepo _repo;
    private readonly FakeUow _uow;
    private readonly FakeCurrentUserService _currentUserService;

    public ParametroSistemaServiceTests()
    {
        _repo = new FakeParamRepo();
        _uow = new FakeUow();
        _currentUserService = new FakeCurrentUserService { EmpresaId = 1 };
        _sut = new ParametroSistemaService(_repo, _uow, _currentUserService);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAll()
    {
        _repo.Seed(new ParametroSistema { Id = 1, Clave = "MonedaBase", Valor = "BOB", EmpresaId = 1 });
        _repo.Seed(new ParametroSistema { Id = 2, Clave = "IVA", Valor = "13", EmpresaId = 1 });

        var result = await _sut.GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task GetByClaveAsync_Found_ReturnsDto()
    {
        _repo.Seed(new ParametroSistema { Id = 1, Clave = "MonedaBase", Valor = "BOB", EmpresaId = 1 });

        var result = await _sut.GetByClaveAsync("MonedaBase");

        Assert.True(result.IsSuccess);
        Assert.Equal("BOB", result.Value!.Valor);
    }

    [Fact]
    public async Task GetByClaveAsync_NotFound_Fails()
    {
        var result = await _sut.GetByClaveAsync("NoExiste");

        Assert.False(result.IsSuccess);
        Assert.Contains("no encontrado", result.Error);
    }

    [Fact]
    public async Task UpsertAsync_NewKey_Creates()
    {
        var dto = new UpsertParametroDto
        {
            Clave = "MonedaBase",
            Valor = "BOB",
            Categoria = "General",
            TipoDato = "Select"
        };

        var result = await _sut.UpsertAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal("BOB", result.Value!.Valor);
        Assert.Equal(1, result.Value!.EmpresaId);
        Assert.True(_uow.SaveCalled);
    }

    [Fact]
    public async Task UpsertAsync_ExistingKey_Updates()
    {
        _repo.Seed(new ParametroSistema { Id = 1, Clave = "MonedaBase", Valor = "BOB", EmpresaId = 1 });

        var dto = new UpsertParametroDto
        {
            Clave = "MonedaBase",
            Valor = "USD",
            Categoria = "General",
            TipoDato = "Select"
        };

        var result = await _sut.UpsertAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal("USD", result.Value!.Valor);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_Fails()
    {
        var result = await _sut.DeleteAsync(999);
        Assert.False(result.IsSuccess);
    }

    // ── Fakes ──

    private class FakeParamRepo : IRepository<ParametroSistema>
    {
        private readonly List<ParametroSistema> _store = new();

        public void Seed(ParametroSistema p) => _store.Add(p);

        public Task<ParametroSistema?> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_store.FirstOrDefault(p => p.Id == id));

        public Task<ParametroSistema?> GetByIdAsync(long id, CancellationToken ct = default)
            => GetByIdAsync((int)id, ct);

        public Task<IReadOnlyList<ParametroSistema>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<ParametroSistema>>(_store.AsReadOnly());

        public Task<IReadOnlyList<ParametroSistema>> FindAsync(Expression<Func<ParametroSistema, bool>> predicate, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<ParametroSistema>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<IReadOnlyList<ParametroSistema>> FindIgnoreQueryFiltersAsync(Expression<Func<ParametroSistema, bool>> predicate, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<ParametroSistema>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<ParametroSistema?> GetByIdIgnoreQueryFiltersAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_store.FirstOrDefault(p => p.Id == id));

        public Task<ParametroSistema> AddAsync(ParametroSistema entity, CancellationToken ct = default)
        {
            _store.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(ParametroSistema entity, CancellationToken ct = default) => Task.CompletedTask;

        public Task DeleteAsync(ParametroSistema entity, CancellationToken ct = default)
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
