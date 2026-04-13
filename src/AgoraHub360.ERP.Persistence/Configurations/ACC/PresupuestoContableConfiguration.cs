namespace AgoraHub360.ERP.Persistence.Configurations.ACC;

using AgoraHub360.ERP.Domain.Entities.ACC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PresupuestoContableConfiguration : IEntityTypeConfiguration<PresupuestoContable>
{
    public void Configure(EntityTypeBuilder<PresupuestoContable> builder)
    {
        builder.ToTable("PresupuestosContables", "acc");

        builder.HasKey(p => p.PresupuestoContableId);

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Estado).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Observaciones).HasMaxLength(500);
        
        builder.Property(p => p.CreadoPor).HasMaxLength(100);
        builder.Property(p => p.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(p => new { p.EmpresaId, p.Gestion, p.Estado });
        builder.HasIndex(p => p.Activo);

        builder.HasMany(p => p.Lineas)
            .WithOne(l => l.PresupuestoContable)
            .HasForeignKey(l => l.PresupuestoContableId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
