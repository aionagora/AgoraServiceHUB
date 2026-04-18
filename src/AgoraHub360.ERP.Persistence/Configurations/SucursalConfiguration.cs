namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SucursalConfiguration : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> builder)
    {
        builder.ToTable("Sucursales", "core");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Codigo)
            .HasMaxLength(50);

        builder.Property(s => s.Direccion)
            .HasMaxLength(500);

        builder.Property(s => s.Ciudad)
            .HasMaxLength(100);

        builder.Property(s => s.Telefono)
            .HasMaxLength(50);

        builder.Property(s => s.Email)
            .HasMaxLength(200);

        // Índices de clave única
        builder.HasIndex(s => new { s.EmpresaId, s.Nombre })
            .IsUnique()
            .HasDatabaseName("IX_Sucursal_Empresa_Nombre");

        // Índice único para Código por empresa (ignora nulos para compatibilidad en algunos RDBMS o garantiza único valor no nulo)
        builder.HasIndex(s => new { s.EmpresaId, s.Codigo })
            .IsUnique()
            .HasDatabaseName("IX_Sucursal_Empresa_Codigo")
            .HasFilter("[Codigo] IS NOT NULL");

        // Relación e integridad referencial restrict
        builder.HasOne(s => s.Empresa)
            .WithMany(e => e.Sucursales)
            .HasForeignKey(s => s.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
