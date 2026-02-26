namespace AgoraHub360.ERP.Persistence.Configurations.CMP;

using AgoraHub360.ERP.Domain.Entities.CMP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrdenCompraLineaConfiguration : IEntityTypeConfiguration<OrdenCompraLinea>
{
    public void Configure(EntityTypeBuilder<OrdenCompraLinea> builder)
    {
        builder.ToTable("OrdenCompraLineas", "cmp");
        builder.HasKey(l => l.OrdenCompraLineaId);
        builder.Property(l => l.OrdenCompraLineaId).UseIdentityColumn();

        builder.Property(l => l.Descripcion).IsRequired().HasMaxLength(300);
        builder.Property(l => l.UnidadMedida).IsRequired().HasMaxLength(20);
        builder.Property(l => l.CreadoPor).HasMaxLength(100);
        builder.Property(l => l.ModificadoPor).HasMaxLength(100);

        builder.Property(l => l.Cantidad).HasColumnType("decimal(18,4)");
        builder.Property(l => l.PrecioUnitario).HasColumnType("decimal(18,4)");
        builder.Property(l => l.PorcentajeDescuento).HasColumnType("decimal(5,2)");
        builder.Property(l => l.MontoDescuento).HasColumnType("decimal(18,4)");
        builder.Property(l => l.Subtotal).HasColumnType("decimal(18,4)");
        builder.Property(l => l.PorcentajeImpuesto).HasColumnType("decimal(5,2)");
        builder.Property(l => l.MontoImpuesto).HasColumnType("decimal(18,4)");
        builder.Property(l => l.TotalLinea).HasColumnType("decimal(18,4)");
        builder.Property(l => l.CantidadRecepcionada).HasColumnType("decimal(18,4)");

        // Ignore computed properties
        builder.Ignore(l => l.RecepcionCompleta);
        builder.Ignore(l => l.CantidadPendiente);

        // Indexes
        builder.HasIndex(l => new { l.OrdenCompraId, l.NumeroLinea }).IsUnique();

        // Relations
        builder.HasOne(l => l.CompanyProduct)
            .WithMany()
            .HasForeignKey(l => l.CompanyProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
