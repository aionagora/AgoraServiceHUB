using AgoraHub360.ERP.Domain.Entities.VTA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations.VTA;

public class FacturaVentaConfiguration : IEntityTypeConfiguration<FacturaVenta>
{
    public void Configure(EntityTypeBuilder<FacturaVenta> builder)
    {
        builder.ToTable("FacturasVenta", "vta");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.NumeroFactura)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.NumeroAutorizacion)
            .HasMaxLength(100);

        builder.Property(x => x.FechaEmision)
            .IsRequired();

        builder.Property(x => x.TipoDocumentoFactura)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(x => x.EstadoFactura)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(x => x.EstadoSiat)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(x => x.NitFactura)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Complemento)
            .HasMaxLength(20);

        builder.Property(x => x.RazonSocialFactura)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.EmailFactura)
            .HasMaxLength(150);

        builder.Property(x => x.TelefonoFactura)
            .HasMaxLength(50);

        builder.Property(x => x.MonedaCodigo)
            .HasMaxLength(10);

        builder.Property(x => x.TipoCambio)
            .HasColumnType("decimal(18,6)");

        builder.Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Property(x => x.DescuentoTotal).HasPrecision(18, 2);
        builder.Property(x => x.ImpuestoTotal).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.Property(x => x.Observaciones)
            .HasMaxLength(1000);

        builder.Property(x => x.MotivoAnulacion)
            .HasMaxLength(500);

        builder.Property(x => x.Cuf)
            .HasMaxLength(150);

        builder.Property(x => x.Cufd)
            .HasMaxLength(150);

        builder.Property(x => x.Cuis)
            .HasMaxLength(150);

        builder.Property(x => x.CodigoControl)
            .HasMaxLength(150);

        builder.Property(x => x.CodigoRecepcion)
            .HasMaxLength(150);

        builder.Property(x => x.CodigoExcepcion)
            .HasMaxLength(100);

        builder.Property(x => x.Leyenda)
            .HasMaxLength(500);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasOne(x => x.Venta)
            .WithMany()
            .HasForeignKey(x => x.VentaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Detalles)
            .WithOne(x => x.FacturaVenta)
            .HasForeignKey(x => x.FacturaVentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.EmpresaId, x.NumeroFactura }).IsUnique();
        builder.HasIndex(x => new { x.EmpresaId, x.VentaId }).IsUnique();
        builder.HasIndex(x => new { x.EmpresaId, x.FechaEmision });
        builder.HasIndex(x => x.NitFactura);
        builder.HasIndex(x => x.EstadoFactura);
    }
}
