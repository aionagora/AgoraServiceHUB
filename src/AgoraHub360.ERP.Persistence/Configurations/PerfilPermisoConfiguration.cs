namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PerfilPermisoConfiguration : IEntityTypeConfiguration<PerfilPermiso>
{
    public void Configure(EntityTypeBuilder<PerfilPermiso> builder)
    {
        builder.ToTable("PerfilesPermisos", "core");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasOne(x => x.PerfilAcceso)
            .WithMany(p => p.Permisos)
            .HasForeignKey(x => x.PerfilAccesoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FormularioSistema)
            .WithMany()
            .HasForeignKey(x => x.FormularioSistemaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AccionSistema)
            .WithMany()
            .HasForeignKey(x => x.AccionSistemaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EmpresaId, x.PerfilAccesoId, x.FormularioSistemaId, x.AccionSistemaId })
            .IsUnique()
            .HasDatabaseName("IX_PerfilesPermisos_UQ");
    }
}
