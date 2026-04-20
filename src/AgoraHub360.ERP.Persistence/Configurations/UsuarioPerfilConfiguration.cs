namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UsuarioPerfilConfiguration : IEntityTypeConfiguration<UsuarioPerfil>
{
    public void Configure(EntityTypeBuilder<UsuarioPerfil> builder)
    {
        builder.ToTable("UsuariosPerfiles", "core");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasOne(x => x.Usuario)
            .WithMany(u => u.Perfiles)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PerfilAcceso)
            .WithMany(p => p.Usuarios)
            .HasForeignKey(x => x.PerfilAccesoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UsuarioId, x.EmpresaId, x.PerfilAccesoId })
            .IsUnique()
            .HasDatabaseName("IX_UsuariosPerfiles_UQ");
    }
}
