using AgoraHub360.ERP.Domain.Entities.FE;
using AgoraHub360.ERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations.FE;

/// <summary>
/// Configuración EF Core para la auditoría de FE (tenant-aware).
/// Esquema: cfg | Tabla: AuditoriaFacturacion
/// </summary>
public class AuditoriaFEConfiguration : IEntityTypeConfiguration<AuditoriaFacturacion>
{
    public void Configure(EntityTypeBuilder<AuditoriaFacturacion> builder)
    {
        builder.ToTable("AuditoriaFacturacion", "cfg");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        // ── Factura asociada (FK) ──────────────────────────────────────────
        builder.HasOne(x => x.FacturaVenta)
            .WithMany()
            .HasForeignKey(x => x.FacturaVentaId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Snapshots textuales ────────────────────────────────────────────
        builder.Property(x => x.ProveedorCodigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ProveedorNombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.AmbienteCodigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.AmbienteNombre)
            .IsRequired()
            .HasMaxLength(200);

        // ── Datos de la operación ──────────────────────────────────────────
        builder.Property(x => x.BillUuid)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Cuf)
            .HasMaxLength(200);

        builder.Property(x => x.EstadoSiat)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(x => x.MensajeError)
            .HasMaxLength(2000);

        builder.Property(x => x.UsuarioId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FechaHora)
            .IsRequired();

        builder.Property(x => x.TiempoRespuestaMs)
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(x => x.CodigoRespuestaProveedor)
            .HasMaxLength(100);

        builder.Property(x => x.DescripcionRespuestaProveedor)
            .HasMaxLength(2000);

        builder.Property(x => x.Exitoso)
            .IsRequired()
            .HasDefaultValue(false);

        // ── Auditoría (TenantEntity → AuditableEntity) ─────────────────────
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.FechaModificacion).IsRequired(false);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);
        builder.Property(x => x.Activo).IsRequired().HasDefaultValue(true);

        // ── Índices ────────────────────────────────────────────────────────
        builder.HasIndex(x => x.FacturaVentaId)
            .HasDatabaseName("IX_AuditoriaFE_FacturaVentaId");

        builder.HasIndex(x => new { x.EmpresaId, x.FechaHora })
            .HasDatabaseName("IX_AuditoriaFE_Empresa_Fecha");

        builder.HasIndex(x => x.Exitoso)
            .HasDatabaseName("IX_AuditoriaFE_Exitoso");
    }
}
