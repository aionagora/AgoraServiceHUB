namespace AgoraHub360.ERP.Persistence.Configurations.CMP;

using AgoraHub360.ERP.Domain.Entities.CMP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrdenCompraConfiguration : IEntityTypeConfiguration<OrdenCompra>
{
    public void Configure(EntityTypeBuilder<OrdenCompra> builder)
    {
        builder.ToTable("OrdenesCompra", "cmp");
        builder.HasKey(o => o.OrdenCompraId);
        builder.Property(o => o.OrdenCompraId).UseIdentityColumn();

        builder.Property(o => o.Numero).IsRequired().HasMaxLength(30);
        builder.Property(o => o.FechaEmision).IsRequired();
        builder.Property(o => o.MonedaId).IsRequired().HasMaxLength(3);
        builder.Property(o => o.TasaCambio).HasColumnType("decimal(18,6)");
        builder.Property(o => o.CondicionPago).HasMaxLength(100);
        builder.Property(o => o.Observaciones).HasMaxLength(1000);
        builder.Property(o => o.ReferenciaExterna).HasMaxLength(100);
        builder.Property(o => o.CreadoPor).HasMaxLength(100);
        builder.Property(o => o.ModificadoPor).HasMaxLength(100);

        builder.Property(o => o.Estado)
            .IsRequired()
            .HasConversion<byte>();

        // Totales
        builder.Property(o => o.Subtotal).HasColumnType("decimal(18,4)");
        builder.Property(o => o.Descuento).HasColumnType("decimal(18,4)");
        builder.Property(o => o.Impuesto).HasColumnType("decimal(18,4)");
        builder.Property(o => o.Total).HasColumnType("decimal(18,4)");

        // Indexes
        builder.HasIndex(o => new { o.EmpresaId, o.Numero }).IsUnique();
        builder.HasIndex(o => new { o.EmpresaId, o.ProveedorId });
        builder.HasIndex(o => new { o.EmpresaId, o.Estado });
        builder.HasIndex(o => new { o.EmpresaId, o.FechaEmision });

        // Relations
        builder.HasOne(o => o.Proveedor)
            .WithMany()
            .HasForeignKey(o => o.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.AlmacenDestino)
            .WithMany()
            .HasForeignKey(o => o.AlmacenDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Lineas)
            .WithOne(l => l.OrdenCompra)
            .HasForeignKey(l => l.OrdenCompraId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
