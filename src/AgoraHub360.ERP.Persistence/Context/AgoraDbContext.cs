namespace AgoraHub360.ERP.Persistence.Context;

using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// DbContext principal del ERP con soporte multi-tenant y auditoría.
/// </summary>
public class AgoraDbContext : DbContext, IUnitOfWork
{
    public AgoraDbContext(DbContextOptions<AgoraDbContext> options) : base(options)
    {
    }

    // Core
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioEmpresa> UsuarioEmpresas => Set<UsuarioEmpresa>();
    public DbSet<Moneda> Monedas => Set<Moneda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de UsuarioEmpresa (clave compuesta)
        modelBuilder.Entity<UsuarioEmpresa>()
            .HasKey(ue => new { ue.UsuarioId, ue.EmpresaId });

        // Moneda usa Codigo como PK
        modelBuilder.Entity<Moneda>()
            .HasKey(m => m.Codigo);

        // Aplicar configuraciones desde el assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgoraDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
