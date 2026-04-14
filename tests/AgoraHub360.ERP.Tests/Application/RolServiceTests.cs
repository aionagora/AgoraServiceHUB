namespace AgoraHub360.ERP.Tests.Application;

using System.Linq.Expressions;
using AgoraHub360.ERP.Application.Services;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Rol;

public class RolServiceTests
{
    private readonly RolService _sut;
    private readonly FakeRepo _repo;
    private readonly FakeUow _uow;

    public RolServiceTests()
    {
        _repo = new FakeRepo();
        _uow = new FakeUow();
        _sut = new RolService(_repo, _uow);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAll()
    {
        _repo.Seed(new Rol { Id = 1, Nombre = "Admin" });
        _repo.Seed(new Rol { Id = 2, Nombre = "User" });

        var result = await _sut.GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_Fails()
    {
        _repo.Seed(new Rol { Id = 1, Nombre = "Admin" });

        var result = await _sut.CreateAsync(new CreateRolDto { Nombre = "Admin" });

        Assert.False(result.IsSuccess);
        Assert.Contains("Ya existe", result.Error);
    }

    [Fact]
    public async Task CreateAsync_Valid_Succeeds()
    {
        var result = await _sut.CreateAsync(new CreateRolDto { Nombre = "Custom", Descripcion = "Rol custom" });

        Assert.True(result.IsSuccess);
        Assert.Equal("Custom", result.Value!.Nombre);
        Assert.True(_uow.SaveCalled);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_Fails()
    {
        var result = await _sut.DeleteAsync(999);
        Assert.False(result.IsSuccess);
    }

    // ── Fakes ──
    private class FakeRepo : IRepository<Rol>
    {
        private readonly List<Rol> _store = new();
        public void Seed(Rol r) => _store.Add(r);
        public Task<Rol?> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_store.FirstOrDefault(r => r.Id == id));
        public Task<Rol?> GetByIdAsync(long id, CancellationToken ct = default)
            => GetByIdAsync((int)id, ct);
        public Task<IReadOnlyList<Rol>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Rol>>(_store.AsReadOnly());
        public Task<IReadOnlyList<Rol>> FindAsync(Expression<Func<Rol, bool>> p, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Rol>>(_store.Where(p.Compile()).ToList().AsReadOnly());
        public Task<Rol> AddAsync(Rol e, CancellationToken ct = default) { _store.Add(e); return Task.FromResult(e); }
        public Task UpdateAsync(Rol e, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Rol e, CancellationToken ct = default) { _store.Remove(e); return Task.CompletedTask; }
    }

    private class FakeUow : IUnitOfWork
    {
        public bool SaveCalled { get; private set; }
        public Task<int> SaveChangesAsync(CancellationToken ct = default) { SaveCalled = true; return Task.FromResult(1); }
        public Task BeginTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public void Dispose() { }
    }
}
