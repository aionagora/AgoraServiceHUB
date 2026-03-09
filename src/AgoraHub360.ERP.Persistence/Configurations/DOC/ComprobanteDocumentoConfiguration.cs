namespace AgoraHub360.ERP.Persistence.Configurations.DOC;

using AgoraHub360.ERP.Domain.Entities.DOC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ComprobanteDocumentoConfiguration : IEntityTypeConfiguration<ComprobanteDocumento>
{
    public void Configure(EntityTypeBuilder<ComprobanteDocumento> builder)
    {
        builder.ToTable("ComprobanteDocumentos", "doc");
        builder.HasKey(cd => cd.Id);
        builder.Property(cd => cd.Id).UseIdentityColumn();

        builder.Property(cd => cd.Descripcion).HasMaxLength(200);
        builder.Property(cd => cd.CreadoPor).HasMaxLength(100);
        builder.Property(cd => cd.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(cd => cd.EmpresaId);
        builder.HasIndex(cd => cd.ComprobanteId);

        builder.HasOne(cd => cd.Comprobante)
            .WithMany(a => a.Documentos)
            .HasForeignKey(cd => cd.ComprobanteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cd => cd.Document)
            .WithMany()
            .HasForeignKey(cd => cd.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
