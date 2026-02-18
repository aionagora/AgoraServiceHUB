namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories", "mdm");
        builder.HasKey(c => c.CategoryId);
        builder.Property(c => c.CategoryId).UseIdentityColumn();

        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(120);
        builder.Property(c => c.Path).HasMaxLength(600);
        builder.Property(c => c.CreadoPor).HasMaxLength(100);
        builder.Property(c => c.ModificadoPor).HasMaxLength(100);

        builder.HasOne(c => c.Catalog)
            .WithMany(cat => cat.Categories)
            .HasForeignKey(c => c.CatalogId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.CatalogId, c.ParentCategoryId, c.Nombre }).IsUnique();
    }
}
