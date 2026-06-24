namespace AgoraHub360.ERP.Tests.Application;

using System.Linq.Expressions;
using System.Security.Claims;
using AgoraHub360.ERP.Api.Authorization.Handlers;
using AgoraHub360.ERP.Api.Authorization.Requirements;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using Microsoft.AspNetCore.Authorization;

public class TenantMembershipHandlerTests
{
    [Fact]
    public async Task DebeDenegar_SiNoExisteUsuarioEmpresa()
    {
        var ueRepo = new FakeUsuarioEmpresaRepo();
        var empresaRepo = new FakeEmpresaRepo();
        empresaRepo.Seed(new Empresa { Id = 1, Nombre = "Empresa A", Activo = true });

        var sut = new TenantMembershipHandler(ueRepo, empresaRepo);
        var user = BuildUser(userId: 10, tenantId: 1);
        var requirement = new TenantMembershipRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, resource: null);

        await sut.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task DebeAprobar_SiExisteUsuarioEmpresaYEmpresaActiva()
    {
        var ueRepo = new FakeUsuarioEmpresaRepo();
        ueRepo.Seed(new UsuarioEmpresa { UsuarioId = 10, EmpresaId = 1, Rol = Roles.Viewer });

        var empresaRepo = new FakeEmpresaRepo();
        empresaRepo.Seed(new Empresa { Id = 1, Nombre = "Empresa A", Activo = true });

        var sut = new TenantMembershipHandler(ueRepo, empresaRepo);
        var user = BuildUser(userId: 10, tenantId: 1);
        var requirement = new TenantMembershipRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, resource: null);

        await sut.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    private static ClaimsPrincipal BuildUser(int userId, int tenantId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypesCustom.TenantId, tenantId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private sealed class FakeUsuarioEmpresaRepo : IRepository<UsuarioEmpresa>
    {
        private readonly List<UsuarioEmpresa> _store = new();

        public void Seed(UsuarioEmpresa ue) => _store.Add(ue);

        public Task<UsuarioEmpresa?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<UsuarioEmpresa?>(null);
        public Task<UsuarioEmpresa?> GetByIdAsync(long id, CancellationToken cancellationToken = default) => Task.FromResult<UsuarioEmpresa?>(null);
        public Task<UsuarioEmpresa?> GetByIdIgnoreQueryFiltersAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<UsuarioEmpresa?>(null);
        public Task<IReadOnlyList<UsuarioEmpresa>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<UsuarioEmpresa>>(_store.AsReadOnly());

        public Task<IReadOnlyList<UsuarioEmpresa>> FindAsync(Expression<Func<UsuarioEmpresa, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UsuarioEmpresa>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<IReadOnlyList<UsuarioEmpresa>> FindIgnoreQueryFiltersAsync(Expression<Func<UsuarioEmpresa, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UsuarioEmpresa>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<UsuarioEmpresa> AddAsync(UsuarioEmpresa entity, CancellationToken cancellationToken = default)
        {
            _store.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(UsuarioEmpresa entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(UsuarioEmpresa entity, CancellationToken cancellationToken = default)
        {
            _store.Remove(entity);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeEmpresaRepo : IRepository<Empresa>
    {
        private readonly List<Empresa> _store = new();

        public void Seed(Empresa empresa) => _store.Add(empresa);

        public Task<Empresa?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(_store.FirstOrDefault(e => e.Id == id));

        public Task<Empresa?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => Task.FromResult(_store.FirstOrDefault(e => e.Id == id));

        public Task<Empresa?> GetByIdIgnoreQueryFiltersAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(_store.FirstOrDefault(e => e.Id == id));

        public Task<IReadOnlyList<Empresa>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Empresa>>(_store.AsReadOnly());

        public Task<IReadOnlyList<Empresa>> FindAsync(Expression<Func<Empresa, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Empresa>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<IReadOnlyList<Empresa>> FindIgnoreQueryFiltersAsync(Expression<Func<Empresa, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Empresa>>(_store.Where(predicate.Compile()).ToList().AsReadOnly());

        public Task<Empresa> AddAsync(Empresa entity, CancellationToken cancellationToken = default)
        {
            _store.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(Empresa entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Empresa entity, CancellationToken cancellationToken = default)
        {
            _store.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
