namespace AgoraHub360.ERP.Persistence.Configurations.BNC;

using AgoraHub360.ERP.Domain.Entities.BNC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ConciliacionBancariaConfiguration : IEntityTypeConfiguration<ConciliacionBancaria>
{
    public void Configure(EntityTypeBuilder<ConciliacionBancaria> builder)
    {
        builder.ToTable("ConciliacionesBancarias", "bnc");

        builder.HasKey(c => c.ConciliacionBancariaId);

        builder.Property(c => c.Estado).IsRequired().HasMaxLength(20);
        builder.Property(c => c.SaldoExtracto).HasPrecision(18, 2);
        builder.Property(c => c.SaldoContable).HasPrecision(18, 2);

        builder.Property(c => c.CreadoPor).HasMaxLength(100);
        builder.Property(c => c.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(c => new { c.EmpresaId, c.CuentaContableId, c.PeriodoContableId }).IsUnique();
        builder.HasIndex(c => c.Activo);

        builder.HasOne(c => c.CuentaContable)
            .WithMany()
            .HasForeignKey(c => c.CuentaContableId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PeriodoContable)
            .WithMany()
            .HasForeignKey(c => c.PeriodoContableId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
