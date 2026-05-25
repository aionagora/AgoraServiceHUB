namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ContactoUsuarioAccesoConfiguration : IEntityTypeConfiguration<ContactoUsuarioAcceso>
{
    public void Configure(EntityTypeBuilder<ContactoUsuarioAcceso> builder)
    {
        builder.ToTable("ContactosUsuariosAccesos", "mdm");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccesoWeb)
            .HasDefaultValue(false);

        builder.Property(x => x.AccesoMovil)
            .HasDefaultValue(false);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasOne(x => x.Contacto)
            .WithMany(c => c.AccesosUsuario)
            .HasForeignKey(x => x.ContactoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Usuario)
            .WithMany(u => u.ContactosAccesos)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PerfilAcceso)
            .WithMany(p => p.ContactosExternos)
            .HasForeignKey(x => x.PerfilAccesoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.EmpresaId, x.ContactoId })
            .IsUnique()
            .HasDatabaseName("IX_ContactoUsuarioAcceso_Empresa_Contacto");

        builder.HasIndex(x => new { x.EmpresaId, x.UsuarioId })
            .IsUnique()
            .HasDatabaseName("IX_ContactoUsuarioAcceso_Empresa_Usuario");
    }
}
