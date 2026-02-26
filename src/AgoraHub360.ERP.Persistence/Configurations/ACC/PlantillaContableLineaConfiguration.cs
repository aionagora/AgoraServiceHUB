namespace AgoraHub360.ERP.Persistence.Configurations.ACC;

using AgoraHub360.ERP.Domain.Entities.ACC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PlantillaContableLineaConfiguration : IEntityTypeConfiguration<PlantillaContableLinea>
{
    public void Configure(EntityTypeBuilder<PlantillaContableLinea> builder)
    {
        builder.ToTable("PlantillaContableLineas", "acc");
        builder.HasKey(l => l.PlantillaContableLineaId);
        builder.Property(l => l.PlantillaContableLineaId).UseIdentityColumn();

        builder.Property(l => l.TipoMovimiento).IsRequired().HasMaxLength(10);
        builder.Property(l => l.CampoMonto).IsRequired().HasMaxLength(50);
        builder.Property(l => l.Factor).HasColumnType("decimal(10,4)");
        builder.Property(l => l.Glosa).HasMaxLength(300);
        builder.Property(l => l.CreadoPor).HasMaxLength(100);
        builder.Property(l => l.ModificadoPor).HasMaxLength(100);

        builder.HasOne(l => l.CuentaContable)
            .WithMany()
            .HasForeignKey(l => l.CuentaContableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
