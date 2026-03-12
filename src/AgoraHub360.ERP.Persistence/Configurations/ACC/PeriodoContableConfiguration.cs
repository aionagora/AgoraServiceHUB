namespace AgoraHub360.ERP.Persistence.Configurations.ACC;

using AgoraHub360.ERP.Domain.Entities.ACC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PeriodoContableConfiguration : IEntityTypeConfiguration<PeriodoContable>
{
    public void Configure(EntityTypeBuilder<PeriodoContable> builder)
    {
        builder.ToTable("PeriodosContables", "acc");
        builder.HasKey(p => p.PeriodoContableId);
        builder.Property(p => p.PeriodoContableId).UseIdentityColumn();

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Estado).IsRequired().HasMaxLength(20);
        builder.Property(p => p.CerradoPorNombre).HasMaxLength(300);
        builder.Property(p => p.CreadoPor).HasMaxLength(100);
        builder.Property(p => p.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(p => new { p.EmpresaId, p.Anio, p.Mes }).IsUnique();
        builder.HasIndex(p => new { p.EmpresaId, p.Estado });
        builder.HasIndex(p => new { p.EmpresaId, p.CerradoPorId });

        builder.HasOne(p => p.CerradoPor)
            .WithMany()
            .HasForeignKey(p => p.CerradoPorId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
