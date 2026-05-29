using AgoraHub360.ERP.Domain.Entities.VTA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations.VTA;

public class FacturaVentaDetalleConfiguration : IEntityTypeConfiguration<FacturaVentaDetalle>
{
    public void Configure(EntityTypeBuilder<FacturaVentaDetalle> builder)
    {
        builder.ToTable("FacturaVentaDetalles", "vta");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.TipoItemVenta)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(x => x.CodigoProducto)
            .HasMaxLength(100);

        builder.Property(x => x.Descripcion)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.DetalleAdicional)
            .HasMaxLength(1000);

        builder.Property(x => x.Cantidad)
            .HasPrecision(18, 4);

        builder.Property(x => x.UnidadMedida)
            .HasMaxLength(50);

        builder.Property(x => x.PrecioUnitario)
            .HasPrecision(18, 4);

        builder.Property(x => x.DescuentoMonto)
            .HasPrecision(18, 2);

        builder.Property(x => x.ImpuestoMonto)
            .HasPrecision(18, 2);

        builder.Property(x => x.TotalLinea)
            .HasPrecision(18, 2);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasOne(x => x.FacturaVenta)
            .WithMany(x => x.Detalles)
            .HasForeignKey(x => x.FacturaVentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.VentaDetalle)
            .WithMany()
            .HasForeignKey(x => x.VentaDetalleId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
