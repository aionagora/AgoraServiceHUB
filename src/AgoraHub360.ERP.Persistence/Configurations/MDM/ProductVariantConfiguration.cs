namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants", "mdm");
        builder.HasKey(v => v.VariantId);
        builder.Property(v => v.VariantId).UseIdentityColumn();

        builder.Property(v => v.Sku).IsRequired().HasMaxLength(60);
        builder.Property(v => v.Barcode).HasMaxLength(120);
        builder.Property(v => v.VariantName).HasMaxLength(180);
        builder.Property(v => v.CreadoPor).HasMaxLength(100);
        builder.Property(v => v.ModificadoPor).HasMaxLength(100);

        builder.HasOne(v => v.ParentProduct)
            .WithMany(p => p.Variants)
            .HasForeignKey(v => v.ParentProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // SKU único por empresa
        builder.HasIndex(v => new { v.EmpresaId, v.Sku }).IsUnique();
        builder.HasIndex(v => v.ParentProductId);
    }
}
