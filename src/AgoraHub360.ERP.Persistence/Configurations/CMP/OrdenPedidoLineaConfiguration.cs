namespace AgoraHub360.ERP.Persistence.Configurations.CMP;

using AgoraHub360.ERP.Domain.Entities.CMP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrdenPedidoLineaConfiguration : IEntityTypeConfiguration<OrdenPedidoLinea>
{
    public void Configure(EntityTypeBuilder<OrdenPedidoLinea> builder)
    {
        builder.ToTable("OrdenPedidoLineas", "cmp");
        builder.HasKey(l => l.OrdenPedidoLineaId);
        builder.Property(l => l.OrdenPedidoLineaId).UseIdentityColumn();

        builder.Property(l => l.Descripcion).IsRequired().HasMaxLength(500);
        builder.Property(l => l.UnidadMedida).IsRequired().HasMaxLength(20);
        builder.Property(l => l.CantidadSolicitada).HasColumnType("decimal(18,4)");
        builder.Property(l => l.CantidadStockDisponible).HasColumnType("decimal(18,4)");
        builder.Property(l => l.CantidadEnTransito).HasColumnType("decimal(18,4)");
        builder.Property(l => l.CantidadAComprar).HasColumnType("decimal(18,4)");
        builder.Property(l => l.Notas).HasMaxLength(500);
        builder.Property(l => l.CreadoPor).HasMaxLength(100);
        builder.Property(l => l.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(l => new { l.OrdenPedidoId, l.NumeroLinea }).IsUnique();

        builder.HasOne(l => l.CompanyProduct)
            .WithMany()
            .HasForeignKey(l => l.CompanyProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
