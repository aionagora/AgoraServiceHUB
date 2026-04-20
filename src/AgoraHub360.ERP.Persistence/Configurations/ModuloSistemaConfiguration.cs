namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ModuloSistemaConfiguration : IEntityTypeConfiguration<ModuloSistema>
{
    public void Configure(EntityTypeBuilder<ModuloSistema> builder)
    {
        builder.ToTable("ModulosSistema", "core");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.Descripcion)
            .HasMaxLength(300);

        builder.Property(x => x.Icono)
            .HasMaxLength(100);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}
