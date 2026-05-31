namespace AgoraHub360.ERP.Tests.Application;

using System.Linq.Expressions;
using AgoraHub360.ERP.Application.Services;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Empresa;

public class EmpresaServiceTests
{
    private readonly EmpresaService _sut;
    private readonly FakeRepository _repository;
    private readonly FakeSucursalRepository _sucursalRepository;
    private readonly FakeAlmacenRepository _almacenRepository;
    private readonly FakeUnitOfWork _unitOfWork;

    public EmpresaServiceTests()
    {
        _repository = new FakeRepository();
        _sucursalRepository = new FakeSucursalRepository();
        _almacenRepository = new FakeAlmacenRepository();
        _unitOfWork = new FakeUnitOfWork();
        _sut = new EmpresaService(
            _repository, 
            _sucursalRepository, 
            _almacenRepository, 
            _unitOfWork, 
            new FakeSeedService(),
            new FakeConfiguracionInicialEmpresaService());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEmpresas()
    {
        _repository.Seed(new Empresa { Id = 1, Nombre = "Emp1" });
        _repository.Seed(new Empresa { Id = 2, Nombre = "Emp2" });

        var result = await _sut.GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsEmpresa()
    {
        _repository.Seed(new Empresa { Id = 1, Nombre = "Test" });

        var result = await _sut.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Test", result.Value!.Nombre);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsFailure()
    {
        var result = await _sut.GetByIdAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Contains("no encontrada", result.Error);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsSuccess()
    {
        var dto = new CreateEmpresaDto { Nombre = "Nueva Empresa", NIT = "111" };

        var result = await _sut.CreateAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal("Nueva Empresa", result.Value!.Nombre);
        Assert.True(_unitOfWork.SaveCalled);
    }

    [Fact]
    public async Task CreateAsync_DuplicateNIT_ReturnsFailure()
    {
        _repository.Seed(new Empresa { Id = 1, Nombre = "Existente", NIT = "111" });

        var dto = new CreateEmpresaDto { Nombre = "Otra", NIT = "111" };
        var result = await _sut.CreateAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Contains("Ya existe", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsSuccess()
    {
        _repository.Seed(new Empresa { Id = 1, Nombre = "ToDelete" });

        var result = await _sut.DeleteAsync(1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFailure()
    {
        var result = await _sut.DeleteAsync(999);

        Assert.False(result.IsSuccess);
    }

    // ── Fakes ──

    private class FakeRepository : IRepository<Empresa>
    {
        private readonly List<Empresa> _store = new();

        public void Seed(Empresa e) => _store.Add(e);

        public Task<Empresa?> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_store.FirstOrDefault(e => e.Id == id));

        public Task<Empresa?> GetByIdAsync(long id, CancellationToken ct = default)
            => GetByIdAsync((int)id, ct);

        public Task<IReadOnlyList<Empresa>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Empresa>>(_store.AsReadOnly());

        public Task<IReadOnlyList<Empresa>> FindAsync(Expression<Func<Empresa, bool>> predicate, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Empresa>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<IReadOnlyList<Empresa>> FindIgnoreQueryFiltersAsync(Expression<Func<Empresa, bool>> predicate, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Empresa>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<Empresa?> GetByIdIgnoreQueryFiltersAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_store.FirstOrDefault(e => e.Id == id));

        public Task<Empresa> AddAsync(Empresa entity, CancellationToken ct = default)
        {
            _store.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(Empresa entity, CancellationToken ct = default) => Task.CompletedTask;

        public Task DeleteAsync(Empresa entity, CancellationToken ct = default)
        {
            _store.Remove(entity);
            return Task.CompletedTask;
        }
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public bool SaveCalled { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            SaveCalled = true;
            return Task.FromResult(1);
        }

        public Task BeginTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task ExecuteInTransactionAsync(Func<Task> op, CancellationToken ct = default) => op();
        public Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> op, CancellationToken ct = default) => op();

        public void Dispose() { }
    }

    private class FakeSeedService : AgoraHub360.ERP.Application.Interfaces.IEmpresaSeedService
    {
        public Task<AgoraHub360.ERP.Application.Common.Result<bool>> SeedDefaultDataAsync(int empresaId, CancellationToken ct = default)
            => Task.FromResult(AgoraHub360.ERP.Application.Common.Result<bool>.Success(true));
    }

    private class FakeConfiguracionInicialEmpresaService : AgoraHub360.ERP.Application.Interfaces.IConfiguracionInicialEmpresaService
    {
        public Task<AgoraHub360.ERP.Application.Common.Result<ConfiguracionInicialEmpresaResultadoDto>> GenerarConfiguracionBasicaAsync(long empresaId, CancellationToken ct = default)
            => Task.FromResult(AgoraHub360.ERP.Application.Common.Result<ConfiguracionInicialEmpresaResultadoDto>.Success(new ConfiguracionInicialEmpresaResultadoDto()));
    }

    private class FakeSucursalRepository : IRepository<Sucursal>
    {
        public Task<IReadOnlyList<Sucursal>> FindAsync(Expression<Func<Sucursal, bool>> predicate, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<Sucursal>> FindIgnoreQueryFiltersAsync(Expression<Func<Sucursal, bool>> predicate, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<Sucursal>> GetAllAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Sucursal?> GetByIdAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Sucursal?> GetByIdAsync(long id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Sucursal?> GetByIdIgnoreQueryFiltersAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Sucursal> AddAsync(Sucursal entity, CancellationToken ct = default) => Task.FromResult(entity);
        public Task UpdateAsync(Sucursal entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Sucursal entity, CancellationToken ct = default) => Task.CompletedTask;
    }

    private class FakeAlmacenRepository : IRepository<AgoraHub360.ERP.Domain.Entities.MDM.Almacen>
    {
        public Task<IReadOnlyList<AgoraHub360.ERP.Domain.Entities.MDM.Almacen>> FindAsync(Expression<Func<AgoraHub360.ERP.Domain.Entities.MDM.Almacen, bool>> predicate, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<AgoraHub360.ERP.Domain.Entities.MDM.Almacen>> FindIgnoreQueryFiltersAsync(Expression<Func<AgoraHub360.ERP.Domain.Entities.MDM.Almacen, bool>> predicate, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<AgoraHub360.ERP.Domain.Entities.MDM.Almacen>> GetAllAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<AgoraHub360.ERP.Domain.Entities.MDM.Almacen?> GetByIdAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<AgoraHub360.ERP.Domain.Entities.MDM.Almacen?> GetByIdAsync(long id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<AgoraHub360.ERP.Domain.Entities.MDM.Almacen?> GetByIdIgnoreQueryFiltersAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<AgoraHub360.ERP.Domain.Entities.MDM.Almacen> AddAsync(AgoraHub360.ERP.Domain.Entities.MDM.Almacen entity, CancellationToken ct = default) => Task.FromResult(entity);
        public Task UpdateAsync(AgoraHub360.ERP.Domain.Entities.MDM.Almacen entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(AgoraHub360.ERP.Domain.Entities.MDM.Almacen entity, CancellationToken ct = default) => Task.CompletedTask;
    }
}
