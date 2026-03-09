namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands", "mdm");
        builder.HasKey(b => b.BrandId);
        builder.Property(b => b.BrandId).UseIdentityColumn();

        builder.Property(b => b.Name).IsRequired().HasMaxLength(120);
        builder.Property(b => b.LogoUrl).HasMaxLength(500);
        builder.Property(b => b.CreadoPor).HasMaxLength(100);
        builder.Property(b => b.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(b => new { b.EmpresaId, b.Name }).IsUnique();
    }
}
