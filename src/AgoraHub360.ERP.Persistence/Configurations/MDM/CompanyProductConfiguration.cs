namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CompanyProductConfiguration : IEntityTypeConfiguration<CompanyProduct>
{
    public void Configure(EntityTypeBuilder<CompanyProduct> builder)
    {
        builder.ToTable("CompanyProducts", "mdm");
        builder.HasKey(cp => cp.CompanyProductId);
        builder.Property(cp => cp.CompanyProductId).UseIdentityColumn();

        builder.Property(cp => cp.Sku).IsRequired().HasMaxLength(60);
        builder.Property(cp => cp.CodigoInterno).HasMaxLength(60);
        builder.Property(cp => cp.MonedaBaseId).HasMaxLength(3);
        builder.Property(cp => cp.MinStock).HasPrecision(18, 4);
        builder.Property(cp => cp.MaxStock).HasPrecision(18, 4);
        builder.Property(cp => cp.ReorderPoint).HasPrecision(18, 4);
        builder.Property(cp => cp.CreadoPor).HasMaxLength(100);
        builder.Property(cp => cp.ModificadoPor).HasMaxLength(100);

        builder.HasOne(cp => cp.Product)
            .WithMany(p => p.CompanyProducts)
            .HasForeignKey(cp => cp.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique: un SKU único por empresa
        builder.HasIndex(cp => new { cp.EmpresaId, cp.Sku }).IsUnique();
        // Unique: un producto puede activarse solo una vez por empresa
        builder.HasIndex(cp => new { cp.EmpresaId, cp.ProductId }).IsUnique();
        // Filtros de visibilidad frecuentes
        builder.HasIndex(cp => new { cp.EmpresaId, cp.IsVisiblePOS, cp.IsVisibleEcommerce });
    }
}
