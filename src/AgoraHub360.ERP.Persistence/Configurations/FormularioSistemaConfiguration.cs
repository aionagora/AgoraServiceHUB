namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FormularioSistemaConfiguration : IEntityTypeConfiguration<FormularioSistema>
{
    public void Configure(EntityTypeBuilder<FormularioSistema> builder)
    {
        builder.ToTable("FormulariosSistema", "core");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Codigo)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Ruta)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Icono)
            .HasMaxLength(100);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasOne(x => x.ModuloSistema)
            .WithMany(m => m.Formularios)
            .HasForeignKey(x => x.ModuloSistemaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Codigo).IsUnique();
        builder.HasIndex(x => new { x.ModuloSistemaId, x.Orden });
    }
}
