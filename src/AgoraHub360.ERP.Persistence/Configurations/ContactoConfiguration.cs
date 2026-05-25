namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ContactoConfiguration : IEntityTypeConfiguration<Contacto>
{
    public void Configure(EntityTypeBuilder<Contacto> builder)
    {
        builder.ToTable("Contactos", "mdm");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombres)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Apellidos)
            .HasMaxLength(150);

        builder.Property(x => x.Cargo)
            .HasMaxLength(120);

        builder.Property(x => x.Telefono)
            .HasMaxLength(50);

        builder.Property(x => x.Celular)
            .HasMaxLength(50);

        builder.Property(x => x.WhatsApp)
            .HasMaxLength(50);

        builder.Property(x => x.Email)
            .HasMaxLength(200);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasOne(x => x.Cliente)
            .WithMany(c => c.Contactos)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ClienteSucursal)
            .WithMany(s => s.Contactos)
            .HasForeignKey(x => x.ClienteSucursalId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.EmpresaId, x.ClienteId, x.Email })
            .HasDatabaseName("IX_Contacto_Empresa_Cliente_Email");

        builder.HasIndex(x => new { x.EmpresaId, x.ClienteId, x.EsPrincipal })
            .IsUnique()
            .HasDatabaseName("IX_Contacto_Empresa_Cliente_Principal")
            .HasFilter("[EsPrincipal] = 1 AND [Activo] = 1");
    }
}
