namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios", "core");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.NombreUsuario)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.NombreCompleto)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(u => u.CreadoPor).HasMaxLength(100);
        builder.Property(u => u.ModificadoPor).HasMaxLength(100);

        builder.HasOne(u => u.EmpresaActiva)
            .WithMany()
            .HasForeignKey(u => u.EmpresaActivaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(u => u.NombreUsuario).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
