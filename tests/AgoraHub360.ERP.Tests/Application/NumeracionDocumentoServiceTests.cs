namespace AgoraHub360.ERP.Tests.Application;

using System.Linq.Expressions;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Application.Services;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Numeracion;

public class NumeracionDocumentoServiceTests
{
    private readonly NumeracionDocumentoService _sut;
    private readonly FakeNumRepo _repo;
    private readonly FakeSucursalRepo _sucursalRepo;
    private readonly FakeUow _uow;
    private readonly FakeCurrentUserService _currentUserService;

    public NumeracionDocumentoServiceTests()
    {
        _repo = new FakeNumRepo();
        _sucursalRepo = new FakeSucursalRepo();
        _uow = new FakeUow();
        _currentUserService = new FakeCurrentUserService { EmpresaId = 1 };
        _sut = new NumeracionDocumentoService(_repo, _sucursalRepo, _uow, _currentUserService);
    }

    private class FakeSucursalRepo : IRepository<Sucursal>
    {
        private readonly List<Sucursal> _store = new();

        public Task<Sucursal?> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_store.FirstOrDefault(x => x.Id == id));

        public Task<Sucursal?> GetByIdAsync(long id, CancellationToken ct = default)
            => Task.FromResult(_store.FirstOrDefault(x => x.Id == id));

        public Task<Sucursal?> GetByIdIgnoreQueryFiltersAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_store.FirstOrDefault(x => x.Id == id));

        public Task<IReadOnlyList<Sucursal>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Sucursal>>(_store.AsReadOnly());

        public Task<IReadOnlyList<Sucursal>> FindAsync(Expression<Func<Sucursal, bool>> predicate, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Sucursal>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<IReadOnlyList<Sucursal>> FindIgnoreQueryFiltersAsync(Expression<Func<Sucursal, bool>> predicate, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Sucursal>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<Sucursal> AddAsync(Sucursal entity, CancellationToken ct = default)
        {
            _store.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(Sucursal entity, CancellationToken ct = default) => Task.CompletedTask;

        public Task DeleteAsync(Sucursal entity, CancellationToken ct = default)
        {
            _store.Remove(entity);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAll()
    {
        _repo.Seed(new NumeracionDocumento { Id = 1, TipoDocumento = "OC", Prefijo = "OC-", EmpresaId = 1 });
        _repo.Seed(new NumeracionDocumento { Id = 2, TipoDocumento = "FAC", Prefijo = "FAC-", EmpresaId = 1 });

        var result = await _sut.GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task CreateAsync_Valid_Succeeds()
    {
        var dto = new CrearNumeracionDocumentoRequestDto
        {
            TipoDocumento = "OC",
            Descripcion = "Orden de Compra",
            Prefijo = "OC-",
            SiguienteNumero = 1,
            LongitudNumero = 6
        };

        var result = await _sut.CreateAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal("OC", result.Value!.TipoDocumento);
        Assert.Equal("OC-000001", result.Value!.PreviewSiguiente);
        Assert.Equal(1, result.Value!.EmpresaId);
        Assert.True(_uow.SaveCalled);
    }

    [Fact]
    public async Task CreateAsync_DuplicateTipo_Fails()
    {
        _repo.Seed(new NumeracionDocumento { Id = 1, TipoDocumento = "OC", Prefijo = "OC-", EmpresaId = 1 });

        var dto = new CrearNumeracionDocumentoRequestDto
        {
            TipoDocumento = "OC",
            Descripcion = "Duplicada",
            Prefijo = "OC2-"
        };

        var result = await _sut.CreateAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Contains("Ya existe", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_Fails()
    {
        var result = await _sut.GetByIdAsync(999);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DesactivarAsync_NotFound_Fails()
    {
        var result = await _sut.DesactivarAsync(999);
        Assert.False(result.IsSuccess);
    }

    // ── Fakes ──

    private class FakeNumRepo : IRepository<NumeracionDocumento>
    {
        private readonly List<NumeracionDocumento> _store = new();

        public void Seed(NumeracionDocumento n) => _store.Add(n);

        public Task<NumeracionDocumento?> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_store.FirstOrDefault(n => n.Id == id));

        public Task<NumeracionDocumento?> GetByIdAsync(long id, CancellationToken ct = default)
            => GetByIdAsync((int)id, ct);

        public Task<IReadOnlyList<NumeracionDocumento>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<NumeracionDocumento>>(_store.AsReadOnly());

        public Task<IReadOnlyList<NumeracionDocumento>> FindAsync(Expression<Func<NumeracionDocumento, bool>> predicate, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<NumeracionDocumento>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<IReadOnlyList<NumeracionDocumento>> FindIgnoreQueryFiltersAsync(Expression<Func<NumeracionDocumento, bool>> predicate, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<NumeracionDocumento>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<NumeracionDocumento?> GetByIdIgnoreQueryFiltersAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_store.FirstOrDefault(n => n.Id == id));

        public Task<NumeracionDocumento> AddAsync(NumeracionDocumento entity, CancellationToken ct = default)
        {
            _store.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(NumeracionDocumento entity, CancellationToken ct = default) => Task.CompletedTask;

        public Task DeleteAsync(NumeracionDocumento entity, CancellationToken ct = default)
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
        public bool IsInRole(string role) => false;
    }
}
