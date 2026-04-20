namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PerfilAccesoConfiguration : IEntityTypeConfiguration<PerfilAcceso>
{
    public void Configure(EntityTypeBuilder<PerfilAcceso> builder)
    {
        builder.ToTable("PerfilesAcceso", "core");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Descripcion)
            .HasMaxLength(400);

        builder.Property(x => x.TipoUsuario)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Ambos");

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique();
    }
}
