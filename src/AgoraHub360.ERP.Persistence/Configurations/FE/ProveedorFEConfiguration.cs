using AgoraHub360.ERP.Domain.Entities.FE;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations.FE;

/// <summary>
/// Configuración EF Core para el catálogo global de proveedores FE.
/// Esquema: cfg | Tabla: ProveedoresFacturacionElectronica
/// </summary>
public class ProveedorFEConfiguration : IEntityTypeConfiguration<ProveedorFacturacionElectronica>
{
    public void Configure(EntityTypeBuilder<ProveedorFacturacionElectronica> builder)
    {
        builder.ToTable("ProveedoresFacturacionElectronica", "cfg");

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

        builder.Property(x => x.RequierePosToken)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.SoportaAnulacion)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.SoportaConsultaEstado)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.SoportaModoOffline)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ClaseProvider)
            .HasMaxLength(500);

        // ── Auditoría (AuditableEntity) ────────────────────────────────────
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.FechaModificacion).IsRequired(false);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);
        builder.Property(x => x.Activo).IsRequired().HasDefaultValue(true);

        // ── Índices ────────────────────────────────────────────────────────
        builder.HasIndex(x => x.Codigo)
            .IsUnique()
            .HasDatabaseName("UX_ProveedoresFE_Codigo");

        builder.HasIndex(x => x.Activo)
            .HasDatabaseName("IX_ProveedoresFE_Activo");

        // ── Seed de catálogo ───────────────────────────────────────────────
        builder.HasData(
            new ProveedorFacturacionElectronica
            {
                Id = 1,
                Codigo = "CIRRUS",
                Nombre = "Cirrus",
                Descripcion = "Proveedor de facturación electrónica Cirrus (SIAT Bolivia)",
                RequierePosToken = true,
                SoportaAnulacion = true,
                SoportaConsultaEstado = true,
                SoportaModoOffline = false,
                ClaseProvider = "AgoraHub360.ERP.Infrastructure.Services.FE.CirrusFacturacionProvider",
                FechaCreacion = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
                CreadoPor = "system",
                Activo = true
            },
            new ProveedorFacturacionElectronica
            {
                Id = 2,
                Codigo = "AGORAFC",
                Nombre = "AgoraFC",
                Descripcion = "Proveedor de facturación electrónica AgoraFC",
                RequierePosToken = false,
                SoportaAnulacion = true,
                SoportaConsultaEstado = true,
                SoportaModoOffline = true,
                ClaseProvider = "AgoraHub360.ERP.Infrastructure.Services.FE.AgoraFCProvider",
                FechaCreacion = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
                CreadoPor = "system",
                Activo = true
            },
            new ProveedorFacturacionElectronica
            {
                Id = 3,
                Codigo = "SIAT_DIRECTO",
                Nombre = "SIAT Directo",
                Descripcion = "Integración directa con el SIAT de Bolivia (futuro)",
                RequierePosToken = false,
                SoportaAnulacion = true,
                SoportaConsultaEstado = true,
                SoportaModoOffline = false,
                ClaseProvider = "AgoraHub360.ERP.Infrastructure.Services.FE.SIATDirectoProvider",
                FechaCreacion = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
                CreadoPor = "system",
                Activo = true
            }
        );
    }
}
