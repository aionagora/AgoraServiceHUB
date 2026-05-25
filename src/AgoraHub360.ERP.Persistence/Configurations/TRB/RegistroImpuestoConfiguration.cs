namespace AgoraHub360.ERP.Persistence.Configurations.TRB;

using AgoraHub360.ERP.Domain.Entities.TRB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RegistroImpuestoConfiguration : IEntityTypeConfiguration<RegistroImpuesto>
{
    public void Configure(EntityTypeBuilder<RegistroImpuesto> builder)
    {
        builder.ToTable("RegistrosImpuesto", "trb");
        
        builder.HasKey(r => r.RegistroImpuestoId);
        
        builder.Property(r => r.TipoImpuesto).IsRequired().HasMaxLength(20);
        builder.Property(r => r.Estado).IsRequired().HasMaxLength(20);
        builder.Property(r => r.NumeroCertificado).HasMaxLength(100);

        builder.Property(r => r.BaseImponible).HasPrecision(18, 2);
        builder.Property(r => r.Tasa).HasPrecision(18, 2);
        builder.Property(r => r.MontoCalculado).HasPrecision(18, 2);
        builder.Property(r => r.CreditoFiscal).HasPrecision(18, 2);
        builder.Property(r => r.DebitoFiscal).HasPrecision(18, 2);
        builder.Property(r => r.SaldoAFavor).HasPrecision(18, 2);
        builder.Property(r => r.MontoAPagar).HasPrecision(18, 2);

        // Auditoría base (AuditableEntity)
        builder.Property(r => r.CreadoPor).HasMaxLength(100);
        builder.Property(r => r.ModificadoPor).HasMaxLength(100);

        // Soft delete (query filter de TenantEntity a veces también lo verifica, mejor no configurar query filter manual para no pisar el global, solo index)
        builder.HasIndex(r => r.Activo);

        // Índice Único solicitado
        builder.HasIndex(r => new { r.EmpresaId, r.PeriodoContableId, r.TipoImpuesto }).IsUnique();

        // Relaciones
        builder.HasOne(r => r.PeriodoContable)
            .WithMany()
            .HasForeignKey(r => r.PeriodoContableId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AsientoContable)
            .WithMany()
            .HasForeignKey(r => r.AsientoContableId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
