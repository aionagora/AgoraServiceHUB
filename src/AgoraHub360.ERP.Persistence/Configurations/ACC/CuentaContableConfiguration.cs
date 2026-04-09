namespace AgoraHub360.ERP.Persistence.Configurations.ACC;

using AgoraHub360.ERP.Domain.Entities.ACC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CuentaContableConfiguration : IEntityTypeConfiguration<CuentaContable>
{
    public void Configure(EntityTypeBuilder<CuentaContable> builder)
    {
        builder.ToTable("CuentasContables", "acc");
        builder.HasKey(c => c.CuentaContableId);
        builder.Property(c => c.CuentaContableId).UseIdentityColumn();

        builder.Property(c => c.Codigo).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Descripcion).HasMaxLength(500);
        builder.Property(c => c.SaldoActual).HasColumnType("decimal(18,4)");
        builder.Property(c => c.ClasificacionFlujo)
            .HasConversion<int>()
            .HasDefaultValue(ClasificacionFlujoEfectivo.NoAplica)
            .HasColumnName("ClasificacionFlujo");
        builder.Property(c => c.CreadoPor).HasMaxLength(100);
        builder.Property(c => c.ModificadoPor).HasMaxLength(100);

        // Indexes
        builder.HasIndex(c => new { c.EmpresaId, c.Codigo }).IsUnique();
        builder.HasIndex(c => new { c.EmpresaId, c.Tipo });
        builder.HasIndex(c => new { c.EmpresaId, c.CuentaPadreId });

        // Self-referencing hierarchy
        builder.HasOne(c => c.CuentaPadre)
            .WithMany(c => c.SubCuentas)
            .HasForeignKey(c => c.CuentaPadreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
