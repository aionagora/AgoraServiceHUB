namespace AgoraHub360.ERP.Persistence.Configurations.DOC;

using AgoraHub360.ERP.Domain.Entities.DOC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents", "doc");
        builder.HasKey(d => d.DocumentId);
        builder.Property(d => d.DocumentId).UseIdentityColumn();

        builder.Property(d => d.FileName).IsRequired().HasMaxLength(160);
        builder.Property(d => d.MimeType).IsRequired().HasMaxLength(80);
        builder.Property(d => d.Url).HasMaxLength(500);
        builder.Property(d => d.Hash).HasMaxLength(64);
        builder.Property(d => d.CreadoPor).HasMaxLength(100);
        builder.Property(d => d.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(d => d.EmpresaId);
    }
}
