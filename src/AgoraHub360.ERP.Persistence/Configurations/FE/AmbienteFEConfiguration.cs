using AgoraHub360.ERP.Domain.Entities.FE;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations.FE;

/// <summary>
/// Configuración EF Core para el catálogo global de ambientes FE.
/// Esquema: cfg | Tabla: AmbientesFacturacionElectronica
/// </summary>
public class AmbienteFEConfiguration : IEntityTypeConfiguration<AmbienteFacturacionElectronica>
{
    public void Configure(EntityTypeBuilder<AmbienteFacturacionElectronica> builder)
    {
        builder.ToTable("AmbientesFacturacionElectronica", "cfg");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Descripcion)
            .HasMaxLength(500);

        builder.Property(x => x.EsProduccion)
            .IsRequired()
            .HasDefaultValue(false);

        // ── Auditoría (AuditableEntity) ────────────────────────────────────
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.FechaModificacion).IsRequired(false);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);
        builder.Property(x => x.Activo).IsRequired().HasDefaultValue(true);

        // ── Índices ────────────────────────────────────────────────────────
        builder.HasIndex(x => x.Codigo)
            .IsUnique()
            .HasDatabaseName("UX_AmbientesFE_Codigo");

        builder.HasIndex(x => x.Activo)
            .HasDatabaseName("IX_AmbientesFE_Activo");

        // ── Seed de catálogo ───────────────────────────────────────────────
        builder.HasData(
            new AmbienteFacturacionElectronica
            {
                Id = 1,
                Codigo = "TEST",
                Nombre = "Pruebas",
                Descripcion = "Ambiente de pruebas para facturación electrónica",
                EsProduccion = false,
                FechaCreacion = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
                CreadoPor = "system",
                Activo = true
            },
            new AmbienteFacturacionElectronica
            {
                Id = 2,
                Codigo = "PRODUCCION",
                Nombre = "Producción",
                Descripcion = "Ambiente de producción para facturación electrónica",
                EsProduccion = true,
                FechaCreacion = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
                CreadoPor = "system",
                Activo = true
            }
        );
    }
}
