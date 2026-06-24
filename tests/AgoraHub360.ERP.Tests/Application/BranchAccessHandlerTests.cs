namespace AgoraHub360.ERP.Tests.Application;

using System.Linq.Expressions;
using System.Security.Claims;
using AgoraHub360.ERP.Api.Authorization.Handlers;
using AgoraHub360.ERP.Api.Authorization.Requirements;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

public class BranchAccessHandlerTests
{
    [Fact]
    public async Task Read_DebeAprobar_CuandoPuedeConsultar()
    {
        var sut = BuildHandler(
            userId: 10,
            tenantId: 1,
            sucursalId: 100,
            puedeConsultar: true,
            puedeOperar: false,
            routeKey: "sucursalId");

        var context = BuildContext(userId: 10, tenantId: 1, canOperate: false);

        await sut.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task Read_DebeAprobar_CuandoPuedeOperar()
    {
        var sut = BuildHandler(
            userId: 10,
            tenantId: 1,
            sucursalId: 100,
            puedeConsultar: false,
            puedeOperar: true,
            routeKey: "sucursalId");

        var context = BuildContext(userId: 10, tenantId: 1, canOperate: false);

        await sut.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task Operate_DebeAprobar_SoloSiPuedeOperar()
    {
        var sut = BuildHandler(
            userId: 10,
            tenantId: 1,
            sucursalId: 100,
            puedeConsultar: true,
            puedeOperar: false,
            routeKey: "sucursalId");

        var context = BuildContext(userId: 10, tenantId: 1, canOperate: true);

        await sut.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task DebeDenegar_SucursalAjenaATenant()
    {
        var sut = BuildHandler(
            userId: 10,
            tenantId: 1,
            sucursalId: 100,
            puedeConsultar: true,
            puedeOperar: true,
            routeKey: "sucursalId",
            sucursalEmpresaId: 2);

        var context = BuildContext(userId: 10, tenantId: 1, canOperate: false);

        await sut.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private static BranchAccessHandler BuildHandler(
        int userId,
        int tenantId,
        int sucursalId,
        bool puedeConsultar,
        bool puedeOperar,
        string routeKey,
        int? sucursalEmpresaId = null)
    {
        var usuarioEmpresaRepo = new FakeRepo<UsuarioEmpresa>();
        usuarioEmpresaRepo.Seed(new UsuarioEmpresa { UsuarioId = userId, EmpresaId = tenantId, Rol = Roles.AdminEmpresa });

        var empresaRepo = new FakeRepo<Empresa>();
        empresaRepo.Seed(new Empresa { Id = tenantId, Nombre = "Empresa", Activo = true });

        var sucursalRepo = new FakeRepo<Sucursal>();
        sucursalRepo.Seed(new Sucursal
        {
            Id = sucursalId,
            EmpresaId = sucursalEmpresaId ?? tenantId,
            Nombre = "Sucursal",
            Activo = true
        });

        var usuarioSucursalRepo = new FakeRepo<UsuarioSucursalAcceso>();
        usuarioSucursalRepo.Seed(new UsuarioSucursalAcceso
        {
            UsuarioId = userId,
            EmpresaId = tenantId,
            SucursalId = sucursalId,
            PuedeConsultar = puedeConsultar,
            PuedeOperar = puedeOperar
        });

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/v1/sucursales/" + sucursalId;
        httpContext.Request.RouteValues[routeKey] = sucursalId.ToString();

        var accessor = new HttpContextAccessor { HttpContext = httpContext };

        return new BranchAccessHandler(
            usuarioEmpresaRepo,
            empresaRepo,
            usuarioSucursalRepo,
            sucursalRepo,
            accessor);
    }

    private static AuthorizationHandlerContext BuildContext(int userId, int tenantId, bool canOperate)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypesCustom.TenantId, tenantId.ToString()),
            new Claim(ClaimTypesCustom.EmpresaId, tenantId.ToString())
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var requirement = new BranchAccessRequirement(canOperate);
        return new AuthorizationHandlerContext(new[] { requirement }, principal, resource: null);
    }

    private sealed class FakeRepo<T> : IRepository<T> where T : class
    {
        private readonly List<T> _store = new();

        public void Seed(T entity) => _store.Add(entity);

        public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var prop = typeof(T).GetProperty("Id");
            var result = _store.FirstOrDefault(x => (int?)prop?.GetValue(x) == id);
            return Task.FromResult(result);
        }

        public Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => GetByIdAsync((int)id, cancellationToken);

        public Task<T?> GetByIdIgnoreQueryFiltersAsync(int id, CancellationToken cancellationToken = default)
            => GetByIdAsync(id, cancellationToken);

        public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<T>>(_store.AsReadOnly());

        public Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<T>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<IReadOnlyList<T>> FindIgnoreQueryFiltersAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<T>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            _store.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(T entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _store.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
