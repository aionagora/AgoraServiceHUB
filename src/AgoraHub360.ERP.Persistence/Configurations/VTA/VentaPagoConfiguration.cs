using AgoraHub360.ERP.Domain.Entities.VTA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations.VTA;

public class VentaPagoConfiguration : IEntityTypeConfiguration<VentaPago>
{
    public void Configure(EntityTypeBuilder<VentaPago> builder)
    {
        builder.ToTable("VentaPagos", "vta");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.FechaPago).IsRequired();

        builder.Property(x => x.TipoPago)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(x => x.ModoPago)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(x => x.Monto).HasPrecision(18, 2);
        builder.Property(x => x.TipoCambio).HasPrecision(18, 6);

        builder.Property(x => x.MonedaId).HasMaxLength(3);
        builder.Property(x => x.MonedaCodigo).HasMaxLength(10);

        builder.Property(x => x.Referencia).HasMaxLength(150);

        builder.Property(x => x.EstadoPago)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.Property(x => x.Anulado).HasDefaultValue(false);
        builder.Property(x => x.MotivoAnulacion).HasMaxLength(500);
        builder.Property(x => x.FechaAnulacion);
        builder.Property(x => x.UsuarioAnulacionId).HasMaxLength(100);

        builder.HasOne(x => x.Venta)
            .WithMany(x => x.Pagos)
            .HasForeignKey(x => x.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FacturaVenta)
            .WithMany()
            .HasForeignKey(x => x.FacturaVentaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.VentaId);
        builder.HasIndex(x => x.FacturaVentaId);
    }
}

