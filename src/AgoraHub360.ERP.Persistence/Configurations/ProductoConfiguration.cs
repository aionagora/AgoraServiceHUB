namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos", "mdm");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Descripcion)
            .HasMaxLength(500);

        builder.Property(p => p.PrecioCompra)
            .HasPrecision(18, 4);

        builder.Property(p => p.PrecioVenta)
            .HasPrecision(18, 4);

        builder.Property(p => p.StockMinimo)
            .HasPrecision(18, 4);

        builder.Property(p => p.Sku)
            .HasMaxLength(50);

        builder.Property(p => p.TipoProducto)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.CostoBase)
            .HasPrecision(18, 4);

        builder.Property(p => p.CreadoPor).HasMaxLength(100);
        builder.Property(p => p.ModificadoPor).HasMaxLength(100);

        // Código único por empresa
        builder.HasIndex(p => new { p.EmpresaId, p.Codigo }).IsUnique();

        // FK a catálogos (sin navegación; se resuelve por diccionario en servicio)
        builder.HasIndex(p => p.CategoriaProductoId);
        builder.HasIndex(p => p.UnidadMedidaId);
    }
}
