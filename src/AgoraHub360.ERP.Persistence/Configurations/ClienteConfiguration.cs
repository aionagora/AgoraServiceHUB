namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes", "mdm");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.RazonSocial)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.NIT)
            .HasMaxLength(50);

        builder.Property(c => c.Direccion)
            .HasMaxLength(300);

        builder.Property(c => c.Telefono)
            .HasMaxLength(50);

        builder.Property(c => c.Email)
            .HasMaxLength(200);

        builder.Property(c => c.NombreContacto)
            .HasMaxLength(200);

        builder.Property(c => c.TipoCliente)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.CreadoPor).HasMaxLength(100);
        builder.Property(c => c.ModificadoPor).HasMaxLength(100);

        // Código único por empresa
        builder.HasIndex(c => new { c.EmpresaId, c.Codigo }).IsUnique();
    }
}
