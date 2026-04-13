namespace AgoraHub360.ERP.Persistence.Configurations.ACT;

using AgoraHub360.ERP.Domain.Entities.ACT;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DepreciacionMensualConfiguration : IEntityTypeConfiguration<DepreciacionMensual>
{
    public void Configure(EntityTypeBuilder<DepreciacionMensual> builder)
    {
        builder.ToTable("DepreciacionesMensuales", "act");

        builder.HasKey(d => d.DepreciacionMensualId);

        builder.Property(d => d.Monto).HasPrecision(18, 2);

        builder.Property(d => d.CreadoPor).HasMaxLength(100);
        builder.Property(d => d.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(d => d.Activo);

        builder.HasIndex(d => new { d.ActivoFijoId, d.PeriodoContableId }).IsUnique();

        builder.HasOne(d => d.ActivoFijo)
            .WithMany(a => a.Depreciaciones)
            .HasForeignKey(d => d.ActivoFijoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.PeriodoContable)
            .WithMany()
            .HasForeignKey(d => d.PeriodoContableId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.AsientoContable)
            .WithMany()
            .HasForeignKey(d => d.AsientoContableId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
