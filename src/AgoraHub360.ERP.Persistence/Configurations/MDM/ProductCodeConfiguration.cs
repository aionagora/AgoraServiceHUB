namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductCodeConfiguration : IEntityTypeConfiguration<ProductCode>
{
    public void Configure(EntityTypeBuilder<ProductCode> builder)
    {
        builder.ToTable("ProductCodes", "mdm");
        builder.HasKey(c => c.ProductCodeId);
        builder.Property(c => c.ProductCodeId).UseIdentityColumn();

        builder.Property(c => c.Valor).IsRequired().HasMaxLength(120);
        builder.Property(c => c.CreadoPor).HasMaxLength(100);
        builder.Property(c => c.ModificadoPor).HasMaxLength(100);

        builder.HasOne(c => c.Product)
            .WithMany(p => p.ProductCodes)
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice para búsqueda por código con tipo
        builder.HasIndex(c => new { c.ProductId, c.CodeType, c.IsPrimary });
        // Único por empresa+tipo+valor (códigos por empresa)
        builder.HasIndex(c => new { c.EmpresaId, c.CodeType, c.Valor });
    }
}
