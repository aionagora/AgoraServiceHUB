namespace AgoraHub360.ERP.Persistence.Context;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// DbContext principal del ERP con soporte multi-tenant y auditoría automática.
/// Los filtros globales de tenant garantizan aislamiento de datos por empresa.
/// </summary>
public class AgoraDbContext : DbContext, IUnitOfWork
{
    private readonly int? _empresaId;

    public AgoraDbContext(
        DbContextOptions<AgoraDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _empresaId = currentUserService.EmpresaId;
    }

    // Constructor para migraciones y design-time (sin tenant)
    public AgoraDbContext(DbContextOptions<AgoraDbContext> options)
        : base(options)
    {
    }

    // Core
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioEmpresa> UsuarioEmpresas => Set<UsuarioEmpresa>();
    public DbSet<Moneda> Monedas => Set<Moneda>();
    public DbSet<Rol> Roles => Set<Rol>();

    // Auditoría
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones Fluent API desde el assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgoraDbContext).Assembly);

        // Filtro global multi-tenant: todas las entidades que heredan de TenantEntity
        // se filtran automáticamente por EmpresaId del usuario actual
        ApplyTenantQueryFilters(modelBuilder);
    }

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var method = typeof(AgoraDbContext)
                .GetMethod(nameof(ApplyTenantFilter),
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(this, new object[] { modelBuilder });
        }
    }

    private void ApplyTenantFilter<T>(ModelBuilder modelBuilder) where T : TenantEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => _empresaId == null || e.EmpresaId == _empresaId);
        modelBuilder.Entity<T>().HasIndex(e => e.EmpresaId);
    }
}
