namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductClassificationLinkConfiguration : IEntityTypeConfiguration<ProductClassificationLink>
{
    public void Configure(EntityTypeBuilder<ProductClassificationLink> builder)
    {
        builder.ToTable("ProductClassificationLinks", "mdm");
        builder.HasKey(l => new { l.ProductId, l.ClassificationId });

        builder.HasOne(l => l.Product)
            .WithMany(p => p.ClassificationLinks)
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Classification)
            .WithMany(c => c.Links)
            .HasForeignKey(l => l.ClassificationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
