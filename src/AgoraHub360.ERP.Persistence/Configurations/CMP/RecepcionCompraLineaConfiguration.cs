namespace AgoraHub360.ERP.Persistence.Configurations.CMP;

using AgoraHub360.ERP.Domain.Entities.CMP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RecepcionCompraLineaConfiguration : IEntityTypeConfiguration<RecepcionCompraLinea>
{
    public void Configure(EntityTypeBuilder<RecepcionCompraLinea> builder)
    {
        builder.ToTable("RecepcionCompraLineas", "cmp");
        builder.HasKey(l => l.RecepcionCompraLineaId);
        builder.Property(l => l.RecepcionCompraLineaId).UseIdentityColumn();

        builder.Property(l => l.CantidadRecibida).HasColumnType("decimal(18,4)");
        builder.Property(l => l.CostoUnitario).HasColumnType("decimal(18,4)");
        builder.Property(l => l.Notas).HasMaxLength(500);
        builder.Property(l => l.CreadoPor).HasMaxLength(100);
        builder.Property(l => l.ModificadoPor).HasMaxLength(100);

        // Relations
        builder.HasOne(l => l.OrdenCompraLinea)
            .WithMany()
            .HasForeignKey(l => l.OrdenCompraLineaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
