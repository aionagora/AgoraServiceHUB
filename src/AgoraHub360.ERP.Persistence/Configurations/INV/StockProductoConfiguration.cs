namespace AgoraHub360.ERP.Persistence.Configurations.INV;

using AgoraHub360.ERP.Domain.Entities.INV;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StockProductoConfiguration : IEntityTypeConfiguration<StockProducto>
{
    public void Configure(EntityTypeBuilder<StockProducto> builder)
    {
        builder.ToTable("StockProductos", "inv");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).UseIdentityColumn();

        builder.Property(s => s.StockActual).HasColumnType("decimal(18,4)");
        builder.Property(s => s.CostoPromedio).HasColumnType("decimal(18,4)");
        builder.Property(s => s.UltimaActualizacion).IsRequired();
        builder.Property(s => s.CreadoPor).HasMaxLength(100);
        builder.Property(s => s.ModificadoPor).HasMaxLength(100);

        // Índice único: un registro de stock por producto+almacén+empresa
        builder.HasIndex(s => new { s.EmpresaId, s.ProductoId, s.AlmacenId }).IsUnique();

        builder.HasOne(s => s.Producto)
            .WithMany()
            .HasForeignKey(s => s.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Almacen)
            .WithMany()
            .HasForeignKey(s => s.AlmacenId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
