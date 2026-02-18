namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductStatusConfiguration : IEntityTypeConfiguration<ProductStatus>
{
    public void Configure(EntityTypeBuilder<ProductStatus> builder)
    {
        builder.ToTable("ProductStatuses", "mdm");
        builder.HasKey(s => s.ProductStatusId);
        builder.Property(s => s.ProductStatusId).UseIdentityColumn();

        builder.Property(s => s.Code).IsRequired().HasMaxLength(30);
        builder.Property(s => s.Nombre).IsRequired().HasMaxLength(80);
        builder.Property(s => s.CreadoPor).HasMaxLength(100);
        builder.Property(s => s.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(s => s.Code).IsUnique();
    }
}
