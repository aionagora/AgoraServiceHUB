namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("ProductAttributes", "mdm");
        builder.HasKey(pa => pa.ProductAttributeId);
        builder.Property(pa => pa.ProductAttributeId).UseIdentityColumn();

        builder.Property(pa => pa.ValueDecimal).HasPrecision(18, 4);
        builder.Property(pa => pa.CreadoPor).HasMaxLength(100);
        builder.Property(pa => pa.ModificadoPor).HasMaxLength(100);

        builder.HasOne(pa => pa.Product)
            .WithMany(p => p.ProductAttributes)
            .HasForeignKey(pa => pa.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pa => pa.AttributeDefinition)
            .WithMany(a => a.ProductAttributes)
            .HasForeignKey(pa => pa.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pa => pa.Option)
            .WithMany()
            .HasForeignKey(pa => pa.OptionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Único por producto+atributo+vigencia
        builder.HasIndex(pa => new { pa.ProductId, pa.AttributeId, pa.ValidFrom }).IsUnique();
        builder.HasIndex(pa => pa.AttributeId);
    }
}
