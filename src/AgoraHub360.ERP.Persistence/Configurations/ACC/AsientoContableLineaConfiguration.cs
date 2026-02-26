namespace AgoraHub360.ERP.Persistence.Configurations.ACC;

using AgoraHub360.ERP.Domain.Entities.ACC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AsientoContableLineaConfiguration : IEntityTypeConfiguration<AsientoContableLinea>
{
    public void Configure(EntityTypeBuilder<AsientoContableLinea> builder)
    {
        builder.ToTable("AsientoContableLineas", "acc");
        builder.HasKey(l => l.AsientoContableLineaId);
        builder.Property(l => l.AsientoContableLineaId).UseIdentityColumn();

        builder.Property(l => l.Debe).HasColumnType("decimal(18,4)");
        builder.Property(l => l.Haber).HasColumnType("decimal(18,4)");
        builder.Property(l => l.Glosa).HasMaxLength(300);
        builder.Property(l => l.Referencia).HasMaxLength(100);
        builder.Property(l => l.CreadoPor).HasMaxLength(100);
        builder.Property(l => l.ModificadoPor).HasMaxLength(100);

        builder.HasOne(l => l.CuentaContable)
            .WithMany()
            .HasForeignKey(l => l.CuentaContableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
