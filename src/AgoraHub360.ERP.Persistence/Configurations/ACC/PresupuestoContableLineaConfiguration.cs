namespace AgoraHub360.ERP.Persistence.Configurations.ACC;

using AgoraHub360.ERP.Domain.Entities.ACC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PresupuestoContableLineaConfiguration : IEntityTypeConfiguration<PresupuestoContableLinea>
{
    public void Configure(EntityTypeBuilder<PresupuestoContableLinea> builder)
    {
        builder.ToTable("PresupuestosContablesLineas", "acc");

        builder.HasKey(p => p.PresupuestoContableLineaId);

        builder.Property(p => p.MontoPresupuestado).HasPrecision(18, 2);

        builder.Property(p => p.CreadoPor).HasMaxLength(100);
        builder.Property(p => p.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(p => new { p.PresupuestoContableId, p.CuentaContableId, p.Mes, p.CentroCostoId });
        builder.HasIndex(p => p.Activo);

        builder.HasOne(p => p.CuentaContable)
            .WithMany()
            .HasForeignKey(p => p.CuentaContableId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CentroCosto)
            .WithMany()
            .HasForeignKey(p => p.CentroCostoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
