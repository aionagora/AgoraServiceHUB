using AgoraHub360.ERP.Domain.Entities.CXC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations.CXC;

public class CuentaPorCobrarConfiguration : IEntityTypeConfiguration<CuentaPorCobrar>
{
    public void Configure(EntityTypeBuilder<CuentaPorCobrar> builder)
    {
        builder.ToTable("CuentasPorCobrar", "cxc");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        // ── Factura ──────────────────────────────────────────────────────────
        builder.HasOne(x => x.FacturaVenta)
            .WithMany()
            .HasForeignKey(x => x.FacturaVentaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.NumeroFactura).IsRequired().HasMaxLength(250);
        builder.Property(x => x.NumeroVenta).HasMaxLength(50);

        builder.Property(x => x.FechaEmision).IsRequired();

        builder.Property(x => x.MonedaCodigo).HasMaxLength(10);
        builder.Property(x => x.TipoCambio).HasColumnType("decimal(18,6)");

        // ── Cliente (denormalizado) ─────────────────────────────────────────
        builder.Property(x => x.ClienteNombre).HasMaxLength(500);
        builder.Property(x => x.ClienteNit).HasMaxLength(50);

        // ── Montos ──────────────────────────────────────────────────────────
        builder.Property(x => x.TotalFactura).HasPrecision(18, 2);
        builder.Property(x => x.TotalPagado).HasPrecision(18, 2);

        // ── SaldoPendiente es computed column ───────────────────────────────
        builder.Property(x => x.SaldoPendiente)
            .HasPrecision(18, 2)
            .HasComputedColumnSql("[TotalFactura] - [TotalPagado]", stored: true);

        // ── Estado ──────────────────────────────────────────────────────────
        builder.Property(x => x.Estado)
            .IsRequired()
            .HasConversion<byte>();

        // ── Auditoría ───────────────────────────────────────────────────────
        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        // ── Índices ─────────────────────────────────────────────────────────
        builder.HasIndex(x => new { x.EmpresaId, x.FacturaVentaId }).IsUnique();
        builder.HasIndex(x => new { x.EmpresaId, x.Estado });
        builder.HasIndex(x => new { x.EmpresaId, x.ClienteId });
        builder.HasIndex(x => new { x.EmpresaId, x.FechaVencimiento });
        builder.HasIndex(x => x.FacturaVentaId);
    }
}
