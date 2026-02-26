namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductUomConfiguration : IEntityTypeConfiguration<ProductUom>
{
    public void Configure(EntityTypeBuilder<ProductUom> builder)
    {
        builder.ToTable("ProductUoms", "mdm");
        builder.HasKey(pu => pu.ProductUomId);
        builder.Property(pu => pu.ProductUomId).UseIdentityColumn();

        builder.Property(pu => pu.FactorToBase).HasPrecision(18, 6);
        builder.Property(pu => pu.Barcode).HasMaxLength(120);
        builder.Property(pu => pu.CreadoPor).HasMaxLength(100);
        builder.Property(pu => pu.ModificadoPor).HasMaxLength(100);

        builder.HasOne(pu => pu.Product)
            .WithMany(p => p.ProductUoms)
            .HasForeignKey(pu => pu.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pu => pu.Uom)
            .WithMany(u => u.ProductUoms)
            .HasForeignKey(pu => pu.UomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pu => new { pu.ProductId, pu.UomId }).IsUnique();
    }
}
