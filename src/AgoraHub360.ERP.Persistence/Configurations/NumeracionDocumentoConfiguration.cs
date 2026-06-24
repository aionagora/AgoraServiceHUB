namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class NumeracionDocumentoConfiguration : IEntityTypeConfiguration<NumeracionDocumento>
{
    public void Configure(EntityTypeBuilder<NumeracionDocumento> builder)
    {
        builder.ToTable("NumeracionesDocumento", "core");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.TipoDocumento)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(n => n.Descripcion)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(n => n.Prefijo)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(n => n.SiguienteNumero)
            .IsRequired();

        builder.Property(n => n.Digitos)
            .IsRequired();

        builder.HasOne(n => n.Sucursal)
            .WithMany()
            .HasForeignKey(n => n.SucursalId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.Property(n => n.CreadoPor).HasMaxLength(100);
        builder.Property(n => n.ModificadoPor).HasMaxLength(100);

        // Tipo de documento único por empresa y sucursal (nullable para compatibilidad histórica)
        builder.HasIndex(n => new { n.EmpresaId, n.TipoDocumento, n.SucursalId }).IsUnique();
    }
}
