namespace AgoraHub360.ERP.Persistence.Configurations.BNC;

using AgoraHub360.ERP.Domain.Entities.BNC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExtractoBancarioConfiguration : IEntityTypeConfiguration<ExtractoBancario>
{
    public void Configure(EntityTypeBuilder<ExtractoBancario> builder)
    {
        builder.ToTable("ExtractosBancarios", "bnc");

        builder.HasKey(e => e.ExtractoBancarioId);

        builder.Property(e => e.Descripcion).IsRequired().HasMaxLength(250);
        builder.Property(e => e.NumeroReferencia).HasMaxLength(100);
        builder.Property(e => e.Monto).HasPrecision(18, 2);

        builder.Property(e => e.CreadoPor).HasMaxLength(100);
        builder.Property(e => e.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(e => new { e.EmpresaId, e.CuentaContableId, e.Fecha });
        builder.HasIndex(e => e.Activo);

        builder.HasOne(e => e.CuentaContable)
            .WithMany()
            .HasForeignKey(e => e.CuentaContableId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AsientoContableLinea)
            .WithMany()
            .HasForeignKey(e => e.AsientoContableLineaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
