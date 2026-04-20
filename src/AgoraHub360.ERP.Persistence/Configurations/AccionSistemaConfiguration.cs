namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AccionSistemaConfiguration : IEntityTypeConfiguration<AccionSistema>
{
    public void Configure(EntityTypeBuilder<AccionSistema> builder)
    {
        builder.ToTable("AccionesSistema", "core");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Codigo)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.Descripcion)
            .HasMaxLength(300);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasOne(x => x.FormularioSistema)
            .WithMany(f => f.Acciones)
            .HasForeignKey(x => x.FormularioSistemaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.FormularioSistemaId, x.Codigo }).IsUnique();
        builder.HasIndex(x => new { x.FormularioSistemaId, x.Orden });
    }
}
