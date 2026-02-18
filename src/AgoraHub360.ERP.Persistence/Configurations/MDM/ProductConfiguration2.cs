namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Configuración de la nueva entidad Product (plantilla global).
/// La entidad Producto (legacy) sigue existiendo en ProductoConfiguration.cs.
/// </summary>
public class ProductConfiguration2 : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", "mdm");
        builder.HasKey(p => p.ProductId);
        builder.Property(p => p.ProductId).UseIdentityColumn();

        builder.Property(p => p.ProductKind).IsRequired();
        builder.Property(p => p.NombreGenerico).IsRequired().HasMaxLength(180);
        builder.Property(p => p.NombreComercial).IsRequired().HasMaxLength(180);
        builder.Property(p => p.DescripcionCorta).HasMaxLength(300);
        builder.Property(p => p.CreadoPor).HasMaxLength(100);
        builder.Property(p => p.ModificadoPor).HasMaxLength(100);

        // FK
        builder.HasOne(p => p.Catalog)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CatalogId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Manufacturer)
            .WithMany(m => m.Products)
            .HasForeignKey(p => p.ManufacturerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.DefaultUom)
            .WithMany(u => u.Products)
            .HasForeignKey(p => p.DefaultUomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.LifecycleStatus)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.LifecycleStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(p => new { p.CatalogId, p.NombreComercial });
        builder.HasIndex(p => new { p.CatalogId, p.BrandId });
    }
}
