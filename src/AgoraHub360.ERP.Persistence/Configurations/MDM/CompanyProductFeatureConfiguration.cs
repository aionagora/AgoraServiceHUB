namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CompanyProductFeatureConfiguration : IEntityTypeConfiguration<CompanyProductFeature>
{
    public void Configure(EntityTypeBuilder<CompanyProductFeature> builder)
    {
        builder.ToTable("CompanyProductFeatures", "mdm");
        builder.HasKey(f => new { f.EmpresaId, f.CompanyProductId, f.FeatureCode });

        builder.Property(f => f.FeatureCode).IsRequired().HasMaxLength(40);

        builder.HasOne(f => f.CompanyProduct)
            .WithMany(cp => cp.Features)
            .HasForeignKey(f => f.CompanyProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
