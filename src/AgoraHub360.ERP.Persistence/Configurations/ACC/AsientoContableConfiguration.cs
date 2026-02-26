namespace AgoraHub360.ERP.Persistence.Configurations.ACC;

using AgoraHub360.ERP.Domain.Entities.ACC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AsientoContableConfiguration : IEntityTypeConfiguration<AsientoContable>
{
    public void Configure(EntityTypeBuilder<AsientoContable> builder)
    {
        builder.ToTable("AsientosContables", "acc");
        builder.HasKey(a => a.AsientoContableId);
        builder.Property(a => a.AsientoContableId).UseIdentityColumn();

        builder.Property(a => a.Numero).IsRequired().HasMaxLength(30);
        builder.Property(a => a.Fecha).IsRequired();
        builder.Property(a => a.TipoRegistro).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Estado).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Concepto).HasMaxLength(300);
        builder.Property(a => a.Glosa).IsRequired().HasMaxLength(500);
        builder.Property(a => a.ValorTipoCambio).HasColumnType("decimal(18,6)");
        builder.Property(a => a.NumeroDocumentoPago).HasMaxLength(100);
        builder.Property(a => a.RegistradoPor).HasMaxLength(100);
        builder.Property(a => a.OrigenTipo).HasMaxLength(50);
        builder.Property(a => a.OrigenReferencia).HasMaxLength(100);
        builder.Property(a => a.TotalDebe).HasColumnType("decimal(18,4)");
        builder.Property(a => a.TotalHaber).HasColumnType("decimal(18,4)");
        builder.Property(a => a.CreadoPor).HasMaxLength(100);
        builder.Property(a => a.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(a => new { a.EmpresaId, a.Numero }).IsUnique();
        builder.HasIndex(a => new { a.EmpresaId, a.Fecha });
        builder.HasIndex(a => new { a.EmpresaId, a.Estado });
        builder.HasIndex(a => new { a.EmpresaId, a.Gestion });
        builder.HasIndex(a => new { a.OrigenTipo, a.OrigenId });

        builder.HasOne(a => a.TipoComprobante)
            .WithMany()
            .HasForeignKey(a => a.TipoComprobanteId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.TipoCambio)
            .WithMany()
            .HasForeignKey(a => a.TipoCambioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.TipoPago)
            .WithMany()
            .HasForeignKey(a => a.TipoPagoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Lineas)
            .WithOne(l => l.AsientoContable)
            .HasForeignKey(l => l.AsientoContableId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
