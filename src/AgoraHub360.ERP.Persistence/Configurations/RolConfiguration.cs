namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roles", "core");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nombre)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Descripcion)
            .HasMaxLength(200);

        builder.Property(r => r.CreadoPor).HasMaxLength(100);
        builder.Property(r => r.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(r => r.Nombre).IsUnique();

        // Seed roles base del sistema
        builder.HasData(
            new Rol { Id = 1, Nombre = "Admin", Descripcion = "Administrador con acceso total" },
            new Rol { Id = 2, Nombre = "Manager", Descripcion = "Gerente con acceso a reportes y aprobaciones" },
            new Rol { Id = 3, Nombre = "User", Descripcion = "Usuario operativo con acceso a modulos asignados" },
            new Rol { Id = 4, Nombre = "Viewer", Descripcion = "Solo lectura" }
        );
    }
}
