namespace AgoraHub360.ERP.Persistence.Configurations.CMP;

using AgoraHub360.ERP.Domain.Entities.CMP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ImportacionLineaConfiguration : IEntityTypeConfiguration<ImportacionLinea>
{
    public void Configure(EntityTypeBuilder<ImportacionLinea> builder)
    {
        builder.ToTable("ImportacionLineas", "cmp");
        builder.HasKey(l => l.ImportacionLineaId);
        builder.Property(l => l.ImportacionLineaId).UseIdentityColumn();

        builder.Property(l => l.CostoFobUnitario).HasColumnType("decimal(18,4)");
        builder.Property(l => l.CostoFobTotal).HasColumnType("decimal(18,4)");
        builder.Property(l => l.FactorDistribucion).HasColumnType("decimal(18,8)");
        builder.Property(l => l.GastoAsignado).HasColumnType("decimal(18,4)");
        builder.Property(l => l.CostoLandedUnitario).HasColumnType("decimal(18,4)");
        builder.Property(l => l.CostoLandedTotal).HasColumnType("decimal(18,4)");
        builder.Property(l => l.CreadoPor).HasMaxLength(100);
        builder.Property(l => l.ModificadoPor).HasMaxLength(100);

        // Relations
        builder.HasOne(l => l.OrdenCompraLinea)
            .WithMany()
            .HasForeignKey(l => l.OrdenCompraLineaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
