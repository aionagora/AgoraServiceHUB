namespace AgoraHub360.ERP.Persistence.Configurations.ACT;

using AgoraHub360.ERP.Domain.Entities.ACT;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ActivoFijoConfiguration : IEntityTypeConfiguration<ActivoFijo>
{
    public void Configure(EntityTypeBuilder<ActivoFijo> builder)
    {
        builder.ToTable("ActivosFijos", "act");

        builder.HasKey(a => a.ActivoFijoId);

        builder.Property(a => a.Codigo).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Descripcion).IsRequired().HasMaxLength(250);
        builder.Property(a => a.CategoriaActivo).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Estado).IsRequired().HasMaxLength(20);

        builder.Property(a => a.CostoAdquisicion).HasPrecision(18, 2);
        builder.Property(a => a.ValorResidual).HasPrecision(18, 2);
        builder.Property(a => a.TasaAnualDS24051).HasPrecision(9, 4); // Porcentaje decimal
        builder.Property(a => a.DepreciacionAcumulada).HasPrecision(18, 2);

        builder.Property(a => a.CreadoPor).HasMaxLength(100);
        builder.Property(a => a.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(a => a.Activo);

        builder.HasIndex(a => new { a.EmpresaId, a.Codigo }).IsUnique();

        builder.HasOne(a => a.CuentaContable)
            .WithMany()
            .HasForeignKey(a => a.CuentaContableId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.CuentaDepreciacion)
            .WithMany()
            .HasForeignKey(a => a.CuentaDepreciacionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.CuentaGastoDepreciacion)
            .WithMany()
            .HasForeignKey(a => a.CuentaGastoDepreciacionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(a => a.Depreciaciones)
            .WithOne(d => d.ActivoFijo)
            .HasForeignKey(d => d.ActivoFijoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
