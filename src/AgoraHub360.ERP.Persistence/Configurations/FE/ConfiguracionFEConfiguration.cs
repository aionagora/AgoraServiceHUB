using AgoraHub360.ERP.Domain.Entities.FE;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations.FE;

/// <summary>
/// Configuración EF Core para la configuración FE por empresa (tenant-aware).
/// Esquema: cfg | Tabla: ConfiguracionFacturacionElectronica
/// </summary>
public class ConfiguracionFEConfiguration : IEntityTypeConfiguration<ConfiguracionFacturacionElectronica>
{
    public void Configure(EntityTypeBuilder<ConfiguracionFacturacionElectronica> builder)
    {
        builder.ToTable("ConfiguracionFacturacionElectronica", "cfg");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.NombreConfiguracion)
            .IsRequired()
            .HasMaxLength(200);

        // ── Proveedor y Ambiente (FK) ──────────────────────────────────────
        builder.HasOne(x => x.ProveedorFacturacionElectronica)
            .WithMany(p => p.Configuraciones)
            .HasForeignKey(x => x.ProveedorFacturacionElectronicaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AmbienteFacturacionElectronica)
            .WithMany(a => a.Configuraciones)
            .HasForeignKey(x => x.AmbienteFacturacionElectronicaId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Credenciales (siempre cifradas) ────────────────────────────────
        builder.Property(x => x.ClientId)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.ClientSecretEncrypted)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.TokenUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ApiManagementUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ApiBillingUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.PosTokenEncrypted)
            .IsRequired()
            .HasMaxLength(2000);

        // ── Configuración fiscal ───────────────────────────────────────────
        builder.Property(x => x.SucursalFiscal)
            .HasMaxLength(50);

        builder.Property(x => x.PuntoVentaFiscal)
            .HasMaxLength(50);

        builder.Property(x => x.ActivityCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.NitEmisor)
            .IsRequired()
            .HasMaxLength(50);

        // ── Configuración técnica ──────────────────────────────────────────
        builder.Property(x => x.TimeoutSegundos)
            .IsRequired()
            .HasDefaultValue(10);

        builder.Property(x => x.EsConfiguracionActiva)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.Observaciones)
            .HasMaxLength(1000);

        // ── Auditoría (TenantEntity → AuditableEntity) ─────────────────────
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.FechaModificacion).IsRequired(false);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);
        builder.Property(x => x.Activo).IsRequired().HasDefaultValue(true);

        // ── Índices ────────────────────────────────────────────────────────
        // Una sola configuración activa por empresa (filtro condicional)
        builder.HasIndex(x => new { x.EmpresaId, x.EsConfiguracionActiva })
            .HasDatabaseName("IX_ConfigFE_Empresa_Activa")
            .HasFilter("[EsConfiguracionActiva] = 1");

        builder.HasIndex(x => new { x.EmpresaId, x.ProveedorFacturacionElectronicaId })
            .HasDatabaseName("IX_ConfigFE_Empresa_Proveedor");

        builder.HasIndex(x => x.EmpresaId)
            .HasDatabaseName("IX_ConfigFE_EmpresaId");
    }
}
