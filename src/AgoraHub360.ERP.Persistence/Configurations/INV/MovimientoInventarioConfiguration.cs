namespace AgoraHub360.ERP.Persistence.Configurations.INV;

using AgoraHub360.ERP.Domain.Entities.INV;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> builder)
    {
        builder.ToTable("MovimientosInventario", "inv");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).UseIdentityColumn();

        builder.Property(m => m.Numero).IsRequired().HasMaxLength(30);
        builder.Property(m => m.TipoMovimiento).IsRequired().HasMaxLength(20);
        builder.Property(m => m.FechaMovimiento).IsRequired();

        builder.Property(m => m.Cantidad).HasColumnType("decimal(18,4)");
        builder.Property(m => m.CostoUnitario).HasColumnType("decimal(18,4)");
        builder.Property(m => m.CostoTotal).HasColumnType("decimal(18,4)");

        builder.Property(m => m.Referencia).HasMaxLength(50);
        builder.Property(m => m.Observaciones).HasMaxLength(500);
        builder.Property(m => m.CreadoPor).HasMaxLength(100);
        builder.Property(m => m.ModificadoPor).HasMaxLength(100);

        // Índices
        builder.HasIndex(m => new { m.EmpresaId, m.Numero }).IsUnique();
        builder.HasIndex(m => new { m.EmpresaId, m.ProductoId, m.FechaMovimiento });
        builder.HasIndex(m => new { m.EmpresaId, m.AlmacenId });

        // Relaciones (sin cascade delete para preservar historial)
        builder.HasOne(m => m.Producto)
            .WithMany()
            .HasForeignKey(m => m.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Almacen)
            .WithMany()
            .HasForeignKey(m => m.AlmacenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.AlmacenDestino)
            .WithMany()
            .HasForeignKey(m => m.AlmacenDestinoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
