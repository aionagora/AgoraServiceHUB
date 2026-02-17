namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ParametroSistemaConfiguration : IEntityTypeConfiguration<ParametroSistema>
{
    public void Configure(EntityTypeBuilder<ParametroSistema> builder)
    {
        builder.ToTable("ParametrosSistema", "core");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Clave)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Valor)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Descripcion)
            .HasMaxLength(500);

        builder.Property(p => p.Categoria)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.TipoDato)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.CreadoPor).HasMaxLength(100);
        builder.Property(p => p.ModificadoPor).HasMaxLength(100);

        // Clave única por empresa
        builder.HasIndex(p => new { p.EmpresaId, p.Clave }).IsUnique();
    }
}
