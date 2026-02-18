namespace AgoraHub360.ERP.Persistence.Configurations.DOC;

using AgoraHub360.ERP.Domain.Entities.DOC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductDocumentConfiguration : IEntityTypeConfiguration<ProductDocument>
{
    public void Configure(EntityTypeBuilder<ProductDocument> builder)
    {
        builder.ToTable("ProductDocuments", "doc");
        builder.HasKey(pd => new { pd.ProductId, pd.DocumentId });

        builder.HasOne(pd => pd.Document)
            .WithMany(d => d.ProductDocuments)
            .HasForeignKey(pd => pd.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pd => new { pd.ProductId, pd.DocType });
    }
}
