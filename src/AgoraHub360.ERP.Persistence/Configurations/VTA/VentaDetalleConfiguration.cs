using AgoraHub360.ERP.Domain.Entities.VTA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations.VTA;

public class VentaDetalleConfiguration : IEntityTypeConfiguration<VentaDetalle>
{
    public void Configure(EntityTypeBuilder<VentaDetalle> builder)
    {
        builder.ToTable("VentaDetalles", "vta");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.TipoItemVenta)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(x => x.Descripcion)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Cantidad).HasPrecision(18, 4);
        builder.Property(x => x.PrecioUnitario).HasPrecision(18, 4);
        builder.Property(x => x.CostoUnitario).HasPrecision(18, 4);
        builder.Property(x => x.DescuentoPorcentaje).HasPrecision(9, 4);
        builder.Property(x => x.DescuentoMonto).HasPrecision(18, 2);
        builder.Property(x => x.ImpuestoMonto).HasPrecision(18, 2);
        builder.Property(x => x.TotalLinea).HasPrecision(18, 2);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasOne(x => x.Venta)
            .WithMany(x => x.Detalles)
            .HasForeignKey(x => x.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CompanyProduct)
            .WithMany()
            .HasForeignKey(x => x.CompanyProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(x => x.Almacen)
            .WithMany()
            .HasForeignKey(x => x.AlmacenId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(x => x.UnidadMedida)
            .WithMany()
            .HasForeignKey(x => x.UnidadMedidaId)
            .HasPrincipalKey(x => x.UomId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(x => x.VentaId);
    }
}
