namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductClassificationConfiguration : IEntityTypeConfiguration<ProductClassification>
{
    public void Configure(EntityTypeBuilder<ProductClassification> builder)
    {
        builder.ToTable("ProductClassifications", "mdm");
        builder.HasKey(c => c.ClassificationId);
        builder.Property(c => c.ClassificationId).UseIdentityColumn();

        builder.Property(c => c.Name).IsRequired().HasMaxLength(120);
        builder.Property(c => c.CreadoPor).HasMaxLength(100);
        builder.Property(c => c.ModificadoPor).HasMaxLength(100);

        builder.HasOne(c => c.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.CatalogId, c.Type, c.Name });
    }
}
