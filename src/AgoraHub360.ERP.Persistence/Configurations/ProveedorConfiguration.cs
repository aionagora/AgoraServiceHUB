namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("Proveedores", "mdm");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.RazonSocial)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.NIT)
            .HasMaxLength(50);

        builder.Property(p => p.Direccion)
            .HasMaxLength(300);

        builder.Property(p => p.Telefono)
            .HasMaxLength(50);

        builder.Property(p => p.Email)
            .HasMaxLength(200);

        builder.Property(p => p.NombreContacto)
            .HasMaxLength(200);

        builder.Property(p => p.TipoProveedor)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Pais)
            .HasMaxLength(100);

        builder.Property(p => p.CondicionPago)
            .HasMaxLength(100);

        builder.Property(p => p.CreadoPor).HasMaxLength(100);
        builder.Property(p => p.ModificadoPor).HasMaxLength(100);

        // Código único por empresa
        builder.HasIndex(p => new { p.EmpresaId, p.Codigo }).IsUnique();
    }
}
