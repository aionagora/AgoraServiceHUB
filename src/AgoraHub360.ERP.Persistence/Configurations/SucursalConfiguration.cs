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

        // Nuevas propiedades - Identificación
        builder.Property(s => s.CodigoInterno).HasMaxLength(50);
        builder.Property(s => s.Sigla).HasMaxLength(20);
        builder.Property(s => s.Descripcion).HasMaxLength(500);

        // Nuevas propiedades - Ubicación
        builder.Property(s => s.Pais).HasMaxLength(100);
        builder.Property(s => s.Departamento).HasMaxLength(100);
        builder.Property(s => s.Provincia).HasMaxLength(100);
        builder.Property(s => s.Zona).HasMaxLength(100);
        builder.Property(s => s.Referencia).HasMaxLength(500);
        builder.Property(s => s.Latitud).HasPrecision(11, 8);
        builder.Property(s => s.Longitud).HasPrecision(11, 8);
        builder.Property(s => s.UrlMapa).HasMaxLength(1000);

        // Nuevas propiedades - Contacto
        builder.Property(s => s.ResponsableNombre).HasMaxLength(200);
        builder.Property(s => s.ResponsableCargo).HasMaxLength(100);
        builder.Property(s => s.Celular).HasMaxLength(50);
        builder.Property(s => s.WhatsApp).HasMaxLength(50);
        builder.Property(s => s.EmailAlternativo).HasMaxLength(200);

        // Nuevas propiedades - Configuración Operativa
        builder.Property(s => s.PermiteVentas).HasDefaultValue(true);
        builder.Property(s => s.PermiteCompras).HasDefaultValue(true);
        builder.Property(s => s.PermiteInventario).HasDefaultValue(true);
        builder.Property(s => s.PermiteDespacho).HasDefaultValue(true);
        builder.Property(s => s.PermiteFacturacion).HasDefaultValue(true);
        builder.Property(s => s.ManejaAlmacen).HasDefaultValue(true);

        // Nuevas propiedades - Configuración Adicional
        builder.Property(s => s.CodigoSucursalFiscal).HasMaxLength(50);
        builder.Property(s => s.PrefijoDocumental).HasMaxLength(20);
        builder.Property(s => s.Observaciones).HasMaxLength(1000);

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
