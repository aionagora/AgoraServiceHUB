using AgoraHub360.ERP.Domain.Entities.VTA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations.VTA;

public class PedidoVentaDetalleConfiguration : IEntityTypeConfiguration<PedidoVentaDetalle>
{
    public void Configure(EntityTypeBuilder<PedidoVentaDetalle> builder)
    {
        builder.ToTable("PedidoVentaDetalles", "vta");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CantidadSolicitada).HasPrecision(18, 4);
        builder.Property(x => x.CantidadConfirmada).HasPrecision(18, 4);
        builder.Property(x => x.CantidadDespachada).HasPrecision(18, 4);
        builder.Property(x => x.PrecioUnitario).HasPrecision(18, 4);
        builder.Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Property(x => x.Impuestos).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.Property(x => x.Observaciones).HasMaxLength(500);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        // Relacion con la cabecera (OnDelete Cascade sí se permite aquí para que si borran la OP borren los detalles, o Restrict si se prefiere no borrar cabeceras nunca, pero Cascade es habitual en Header-Lines en EF Core cuando la linea no puede existir sin cabecera).
        builder.HasOne(x => x.PedidoVenta)
            .WithMany(p => p.Detalles)
            .HasForeignKey(x => x.PedidoVentaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación Producto
        builder.HasOne(x => x.CompanyProduct)
            .WithMany()
            .HasForeignKey(x => x.CompanyProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
