namespace AgoraHub360.ERP.Tests.Application;

using System.Linq.Expressions;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Application.Services;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs.Empresa;
using AgoraHub360.ERP.Shared.DTOs.Numeracion;
using AgoraHub360.ERP.Shared.DTOs.Parametro;

public class EmpresaDemoServiceTests
{
    [Fact]
    public async Task CrearCompletaAsync_DEMO01_CreaEmpresaYDatosBase()
    {
        var ctx = new TestContext(Roles.SuperAdmin);

        var result = await ctx.Service.CrearCompletaAsync(new CrearEmpresaDemoCompletaRequestDto
        {
            CodigoDemo = "DEMO01"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var response = Assert.IsType<CrearEmpresaDemoCompletaResponseDto>(result.Value);
        Assert.Equal("DEMO01", response.CodigoDemo);
        Assert.True(response.EmpresaCreada);
        Assert.Equal("990001001", response.Nit);

        Assert.Single(ctx.EmpresaRepo.Items);
        Assert.Equal(3, ctx.UsuarioRepo.Items.Count);
        Assert.Equal(3, ctx.UsuarioEmpresaRepo.Items.Count);
        Assert.Equal(2, ctx.SucursalRepo.Items.Count);
        Assert.Equal(2, ctx.AlmacenRepo.Items.Count);
        Assert.Equal(3, ctx.ClienteRepo.Items.Count);
        Assert.Equal(2, ctx.ProveedorRepo.Items.Count);
        Assert.Equal(3, ctx.ProductRepo.Items.Count);
        Assert.Equal(3, ctx.CompanyProductRepo.Items.Count);
    }

    [Fact]
    public async Task CrearCompletaAsync_DEMO01yDEMO02_MantieneAislamiento()
    {
        var ctx = new TestContext(Roles.SuperAdmin);

        var r1 = await ctx.Service.CrearCompletaAsync(new CrearEmpresaDemoCompletaRequestDto { CodigoDemo = "DEMO01" }, CancellationToken.None);
        var r2 = await ctx.Service.CrearCompletaAsync(new CrearEmpresaDemoCompletaRequestDto { CodigoDemo = "DEMO02" }, CancellationToken.None);

        Assert.True(r1.IsSuccess);
        Assert.True(r2.IsSuccess);

        Assert.Equal(2, ctx.EmpresaRepo.Items.Count);
        Assert.Contains(ctx.EmpresaRepo.Items, e => e.NIT == "990001001");
        Assert.Contains(ctx.EmpresaRepo.Items, e => e.NIT == "990001002");

        Assert.Contains(ctx.UsuarioRepo.Items, u => u.NombreUsuario == "admindemo01");
        Assert.Contains(ctx.UsuarioRepo.Items, u => u.NombreUsuario == "admindemo02");

        var demo01Empresa = ctx.EmpresaRepo.Items.Single(e => e.NIT == "990001001").Id;
        var demo02Empresa = ctx.EmpresaRepo.Items.Single(e => e.NIT == "990001002").Id;

        Assert.Contains(ctx.SucursalRepo.Items, s => s.EmpresaId == demo01Empresa && (s.Codigo ?? string.Empty).Contains("DEMO01", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(ctx.SucursalRepo.Items, s => s.EmpresaId == demo02Empresa && (s.Codigo ?? string.Empty).Contains("DEMO02", StringComparison.OrdinalIgnoreCase));

        Assert.Contains(ctx.ClienteRepo.Items, c => c.EmpresaId == demo01Empresa && c.Codigo.Contains("DEMO01", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(ctx.ClienteRepo.Items, c => c.EmpresaId == demo02Empresa && c.Codigo.Contains("DEMO02", StringComparison.OrdinalIgnoreCase));

        Assert.Contains(ctx.NumeracionService.Keys, k => k.Contains("DEMO01-", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(ctx.NumeracionService.Keys, k => k.Contains("DEMO02-", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CrearCompletaAsync_ReejecutarDEMO01_EsIdempotente()
    {
        var ctx = new TestContext(Roles.SuperAdmin);

        var first = await ctx.Service.CrearCompletaAsync(new CrearEmpresaDemoCompletaRequestDto { CodigoDemo = "DEMO01" }, CancellationToken.None);
        Assert.True(first.IsSuccess);

        var empresasAntes = ctx.EmpresaRepo.Items.Count;
        var usuariosAntes = ctx.UsuarioRepo.Items.Count;
        var sucursalesAntes = ctx.SucursalRepo.Items.Count;
        var almacenesAntes = ctx.AlmacenRepo.Items.Count;
        var clientesAntes = ctx.ClienteRepo.Items.Count;
        var proveedoresAntes = ctx.ProveedorRepo.Items.Count;
        var productosAntes = ctx.ProductRepo.Items.Count;

        var second = await ctx.Service.CrearCompletaAsync(new CrearEmpresaDemoCompletaRequestDto { CodigoDemo = "DEMO01" }, CancellationToken.None);

        Assert.True(second.IsSuccess);
        Assert.True(second.Value!.EmpresaExistente);

        Assert.Equal(empresasAntes, ctx.EmpresaRepo.Items.Count);
        Assert.Equal(usuariosAntes, ctx.UsuarioRepo.Items.Count);
        Assert.Equal(sucursalesAntes, ctx.SucursalRepo.Items.Count);
        Assert.Equal(almacenesAntes, ctx.AlmacenRepo.Items.Count);
        Assert.Equal(clientesAntes, ctx.ClienteRepo.Items.Count);
        Assert.Equal(proveedoresAntes, ctx.ProveedorRepo.Items.Count);
        Assert.Equal(productosAntes, ctx.ProductRepo.Items.Count);
    }

    [Fact]
    public async Task CrearCompletaAsync_ResetSiExiste_DevuelveAdvertencia()
    {
        var ctx = new TestContext(Roles.SuperAdmin);

        var first = await ctx.Service.CrearCompletaAsync(new CrearEmpresaDemoCompletaRequestDto { CodigoDemo = "DEMO01" }, CancellationToken.None);
        Assert.True(first.IsSuccess);

        var second = await ctx.Service.CrearCompletaAsync(new CrearEmpresaDemoCompletaRequestDto
        {
            CodigoDemo = "DEMO01",
            ResetSiExiste = true
        }, CancellationToken.None);

        Assert.True(second.IsSuccess);
        Assert.True(second.Value!.ResetSolicitado);
        Assert.Contains("Reset no implementado", second.Value.Advertencia ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CrearCompletaAsync_UsuarioSinPermiso_Falla()
    {
        var ctx = new TestContext(Roles.None);

        var result = await ctx.Service.CrearCompletaAsync(new CrearEmpresaDemoCompletaRequestDto
        {
            CodigoDemo = "DEMO01"
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Solo PlatformAdmin/SuperAdmin", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CrearCompletaAsync_SystemAdmin_PuedeCrearDemo()
    {
        var ctx = new TestContext(Roles.SystemAdmin);

        var result = await ctx.Service.CrearCompletaAsync(new CrearEmpresaDemoCompletaRequestDto
        {
            CodigoDemo = "DEMO03"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("DEMO03", result.Value!.CodigoDemo);
    }

    [Fact]
    public async Task CrearCompletaAsync_CreaAccesosSucursalSegunRol()
    {
        var ctx = new TestContext(Roles.SuperAdmin);

        var result = await ctx.Service.CrearCompletaAsync(new CrearEmpresaDemoCompletaRequestDto
        {
            CodigoDemo = "DEMO01"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var empresaId = ctx.EmpresaRepo.Items.Single().Id;
        var admin = ctx.UsuarioRepo.Items.Single(u => u.NombreUsuario == "admindemo01");
        var operador = ctx.UsuarioRepo.Items.Single(u => u.NombreUsuario == "operadordemo01");
        var viewer = ctx.UsuarioRepo.Items.Single(u => u.NombreUsuario == "viewerdemo01");

        var adminAccesos = ctx.UsuarioSucursalAccesoRepo.Items.Where(a => a.EmpresaId == empresaId && a.UsuarioId == admin.Id).ToList();
        var operadorAccesos = ctx.UsuarioSucursalAccesoRepo.Items.Where(a => a.EmpresaId == empresaId && a.UsuarioId == operador.Id).ToList();
        var viewerAccesos = ctx.UsuarioSucursalAccesoRepo.Items.Where(a => a.EmpresaId == empresaId && a.UsuarioId == viewer.Id).ToList();

        Assert.Equal(2, adminAccesos.Count);
        Assert.All(adminAccesos, a =>
        {
            Assert.True(a.PuedeConsultar);
            Assert.True(a.PuedeOperar);
        });

        Assert.Equal(2, operadorAccesos.Count);
        Assert.All(operadorAccesos, a =>
        {
            Assert.True(a.PuedeConsultar);
            Assert.True(a.PuedeOperar);
        });

        Assert.Equal(2, viewerAccesos.Count);
        Assert.All(viewerAccesos, a =>
        {
            Assert.True(a.PuedeConsultar);
            Assert.False(a.PuedeOperar);
        });

        Assert.Single(adminAccesos.Where(a => a.EsPredeterminada));
        Assert.Single(operadorAccesos.Where(a => a.EsPredeterminada));
        Assert.Single(viewerAccesos.Where(a => a.EsPredeterminada));
    }

    [Fact]
    public async Task CrearCompletaAsync_FalloProductos_NoBloqueaModoMinimoYDevuelveAdvertencia()
    {
        var ctx = new TestContext(Roles.SuperAdmin, failProducts: true);

        var result = await ctx.Service.CrearCompletaAsync(new CrearEmpresaDemoCompletaRequestDto
        {
            CodigoDemo = "DEMO01"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var response = Assert.IsType<CrearEmpresaDemoCompletaResponseDto>(result.Value);

        Assert.Single(ctx.EmpresaRepo.Items);
        Assert.Equal(3, ctx.UsuarioRepo.Items.Count);
        Assert.Equal(2, ctx.SucursalRepo.Items.Count);
        Assert.Equal(2, ctx.AlmacenRepo.Items.Count);
        Assert.Equal(0, ctx.ProductRepo.Items.Count);

        Assert.Contains(response.Advertencias, a => a.Contains("Productos no creados", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("Productos no creados", response.Advertencia ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        Assert.NotEmpty(response.Usuarios);
        Assert.NotEmpty(response.Sucursales);
        Assert.NotEmpty(response.Almacenes);
        Assert.Equal(response.NombreEmpresa, response.Empresa);
    }

    private sealed class TestContext
    {
        public InMemoryRepository<Empresa> EmpresaRepo { get; } = new();
        public InMemoryRepository<Usuario> UsuarioRepo { get; } = new();
        public InMemoryRepository<UsuarioEmpresa> UsuarioEmpresaRepo { get; } = new();
        public InMemoryRepository<Sucursal> SucursalRepo { get; } = new();
        public InMemoryRepository<Almacen> AlmacenRepo { get; } = new();
        public InMemoryRepository<Cliente> ClienteRepo { get; } = new();
        public InMemoryRepository<Proveedor> ProveedorRepo { get; } = new();
        public InMemoryRepository<Catalog> CatalogRepo { get; } = new();
        public InMemoryRepository<Uom> UomRepo { get; } = new();
        public InMemoryRepository<ProductStatus> ProductStatusRepo { get; } = new();
        public InMemoryRepository<Product> ProductRepo { get; } = new();
        public InMemoryRepository<CompanyProduct> CompanyProductRepo { get; } = new();
        public InMemoryRepository<UsuarioSucursalAcceso> UsuarioSucursalAccesoRepo { get; } = new();

        public FakeNumeracionDocumentoService NumeracionService { get; } = new();
        public EmpresaDemoService Service { get; }

        public TestContext(string platformRole, bool failProducts = false)
        {
            ProductRepo.FailOnAdd = failProducts;

            Service = new EmpresaDemoService(
                EmpresaRepo,
                UsuarioRepo,
                UsuarioEmpresaRepo,
                SucursalRepo,
                AlmacenRepo,
                ClienteRepo,
                ProveedorRepo,
                CatalogRepo,
                UomRepo,
                ProductStatusRepo,
                ProductRepo,
                CompanyProductRepo,
                UsuarioSucursalAccesoRepo,
                new FakeParametroSistemaService(),
                NumeracionService,
                new FakeConfiguracionInicialEmpresaService(),
                new FakeCurrentUserService(platformRole),
                new FakeUnitOfWork());
        }
    }

    private sealed class InMemoryRepository<T> : IRepository<T> where T : class
    {
        private static readonly string[] IdProps = ["Id", "ProductId", "CatalogId", "UomId", "ProductStatusId", "CompanyProductId"];
        private readonly List<T> _items = [];
        private long _nextId = 1;

        public IReadOnlyList<T> Items => _items;
        public bool FailOnAdd { get; set; }

        public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => GetEntityId(x) == id));

        public Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => GetEntityId(x) == id));

        public Task<T?> GetByIdIgnoreQueryFiltersAsync(int id, CancellationToken cancellationToken = default)
            => GetByIdAsync(id, cancellationToken);

        public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<T>>(_items.AsReadOnly());

        public Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<T>>(_items.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<IReadOnlyList<T>> FindIgnoreQueryFiltersAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<T>>(_items.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (FailOnAdd)
                throw new InvalidOperationException($"Error simulado de repositorio para {typeof(T).Name}.");

            TryAssignId(entity);
            _items.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(T entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _items.Remove(entity);
            return Task.CompletedTask;
        }

        private long? GetEntityId(T entity)
        {
            foreach (var propName in IdProps)
            {
                var prop = typeof(T).GetProperty(propName);
                if (prop is null)
                    continue;

                var value = prop.GetValue(entity);
                if (value is int intId)
                    return intId;
                if (value is long longId)
                    return longId;
            }

            return null;
        }

        private void TryAssignId(T entity)
        {
            foreach (var propName in IdProps)
            {
                var prop = typeof(T).GetProperty(propName);
                if (prop is null || !prop.CanWrite)
                    continue;

                if (prop.PropertyType == typeof(int))
                {
                    var current = (int)(prop.GetValue(entity) ?? 0);
                    if (current == 0)
                    {
                        prop.SetValue(entity, (int)_nextId);
                        _nextId++;
                    }
                    return;
                }

                if (prop.PropertyType == typeof(long))
                {
                    var current = (long)(prop.GetValue(entity) ?? 0L);
                    if (current == 0L)
                    {
                        prop.SetValue(entity, _nextId);
                        _nextId++;
                    }
                    return;
                }
            }
        }
    }

    private sealed class FakeCurrentUserService(string platformRole) : ICurrentUserService
    {
        public string? UserId => "1";
        public int? UserIdInt => 1;
        public string? UserName => "tester";
        public int? EmpresaId => null;
        public int? TenantId => null;
        public string PlatformRole { get; } = platformRole;
        public string TenantRole => Roles.NoAccess;
        public string TenantStatus => AgoraHub360.ERP.Shared.Constants.TenantStatus.NotSelected;
        public bool IsInRole(string role) => string.Equals(role, PlatformRole, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default) => operation();
        public Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default) => operation();
        public void Dispose() { }
    }

    private sealed class FakeConfiguracionInicialEmpresaService : IConfiguracionInicialEmpresaService
    {
        public Task<Result<ConfiguracionInicialEmpresaResultadoDto>> GenerarConfiguracionBasicaAsync(long empresaId, CancellationToken ct = default)
            => Task.FromResult(Result<ConfiguracionInicialEmpresaResultadoDto>.Success(new ConfiguracionInicialEmpresaResultadoDto()));
    }

    private sealed class FakeParametroSistemaService : IParametroSistemaService
    {
        public Task<Result<IReadOnlyList<ParametroSistemaDto>>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Result<IReadOnlyList<ParametroSistemaDto>>.Success(Array.Empty<ParametroSistemaDto>()));

        public Task<Result<IReadOnlyList<ParametroSistemaDto>>> GetByCategoriaAsync(string categoria, CancellationToken ct = default)
            => Task.FromResult(Result<IReadOnlyList<ParametroSistemaDto>>.Success(Array.Empty<ParametroSistemaDto>()));

        public Task<Result<ParametroSistemaDto>> GetByClaveAsync(string clave, CancellationToken ct = default)
            => Task.FromResult(Result<ParametroSistemaDto>.Failure("No implementado en test."));

        public Task<Result<ParametroSistemaDto>> UpsertAsync(UpsertParametroDto dto, CancellationToken ct = default)
            => Task.FromResult(Result<ParametroSistemaDto>.Failure("No implementado en test."));

        public Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Success(true));
    }

    private sealed class FakeNumeracionDocumentoService : INumeracionDocumentoService
    {
        private int _nextId = 1;
        public HashSet<string> Keys { get; } = [];

        public Task<Result<IReadOnlyList<NumeracionDocumentoDto>>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Result<IReadOnlyList<NumeracionDocumentoDto>>.Success(Array.Empty<NumeracionDocumentoDto>()));

        public Task<Result<NumeracionDocumentoDto>> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(Result<NumeracionDocumentoDto>.Failure("No implementado en test."));

        public Task<Result<NumeracionDocumentoDto>> CreateAsync(CrearNumeracionDocumentoRequestDto dto, CancellationToken ct = default)
        {
            var key = $"{dto.SucursalId}|{dto.TipoDocumento}|{dto.Prefijo}";
            if (!Keys.Add(key))
            {
                return Task.FromResult(Result<NumeracionDocumentoDto>.Failure("Ya existe una numeración para este tipo de documento y sucursal."));
            }

            var created = new NumeracionDocumentoDto
            {
                Id = _nextId++,
                SucursalId = dto.SucursalId,
                TipoDocumento = dto.TipoDocumento,
                Descripcion = dto.Descripcion,
                Prefijo = dto.Prefijo,
                SiguienteNumero = dto.SiguienteNumero,
                LongitudNumero = dto.LongitudNumero,
                Activo = dto.Activo
            };

            return Task.FromResult(Result<NumeracionDocumentoDto>.Success(created));
        }

        public Task<Result<NumeracionDocumentoDto>> UpdateAsync(int id, ActualizarNumeracionDocumentoRequestDto dto, CancellationToken ct = default)
            => Task.FromResult(Result<NumeracionDocumentoDto>.Failure("No implementado en test."));

        public Task<Result<bool>> ActivarAsync(int id, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Success(true));

        public Task<Result<bool>> DesactivarAsync(int id, CancellationToken ct = default)
            => Task.FromResult(Result<bool>.Success(true));

        public Task<Result<string>> GenerarSiguienteNumeroAsync(string tipoDocumento, long sucursalId, CancellationToken ct = default)
            => Task.FromResult(Result<string>.Failure("No implementado en test."));
    }
}
