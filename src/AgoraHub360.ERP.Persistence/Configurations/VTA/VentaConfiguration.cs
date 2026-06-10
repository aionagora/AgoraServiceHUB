using AgoraHub360.ERP.Domain.Entities.VTA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations.VTA;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("Ventas", "vta");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.NumeroVenta)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FechaVenta)
            .IsRequired();

        builder.Property(x => x.TipoVenta)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(x => x.EstadoVenta)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(x => x.EstadoPago)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(x => x.MonedaId)
            .HasMaxLength(3);

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

        builder.Property(x => x.FechaVencimientoPago)
            .HasColumnType("datetime2");

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasOne(x => x.Sucursal)
            .WithMany()
            .HasForeignKey(x => x.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Almacen)
            .WithMany()
            .HasForeignKey(x => x.AlmacenId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(x => x.Cliente)
            .WithMany()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(x => x.PedidoVenta)
            .WithMany()
            .HasForeignKey(x => x.PedidoVentaId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(x => x.Moneda)
            .WithMany()
            .HasForeignKey(x => x.MonedaId)
            .HasPrincipalKey(x => x.Codigo)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasMany(x => x.Detalles)
            .WithOne(x => x.Venta)
            .HasForeignKey(x => x.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.DatosFacturacion)
            .WithOne(x => x.Venta)
            .HasForeignKey<VentaFacturacionDatos>(x => x.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Pagos)
            .WithOne(x => x.Venta)
            .HasForeignKey(x => x.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.EmpresaId, x.NumeroVenta }).IsUnique();
        builder.HasIndex(x => new { x.EmpresaId, x.FechaVenta });
        builder.HasIndex(x => new { x.EmpresaId, x.ClienteId });
        builder.HasIndex(x => x.PedidoVentaId);
    }
}
