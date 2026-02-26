namespace AgoraHub360.ERP.Persistence.Configurations.CMP;

using AgoraHub360.ERP.Domain.Entities.CMP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GastoImportacionConfiguration : IEntityTypeConfiguration<GastoImportacion>
{
    public void Configure(EntityTypeBuilder<GastoImportacion> builder)
    {
        builder.ToTable("GastosImportacion", "cmp");
        builder.HasKey(g => g.GastoImportacionId);
        builder.Property(g => g.GastoImportacionId).UseIdentityColumn();

        builder.Property(g => g.TipoGasto).IsRequired().HasMaxLength(50);
        builder.Property(g => g.Descripcion).HasMaxLength(300);
        builder.Property(g => g.Monto).HasColumnType("decimal(18,4)");
        builder.Property(g => g.MonedaId).IsRequired().HasMaxLength(3);
        builder.Property(g => g.TasaCambio).HasColumnType("decimal(18,6)");
        builder.Property(g => g.MontoBase).HasColumnType("decimal(18,4)");
        builder.Property(g => g.Referencia).HasMaxLength(100);
        builder.Property(g => g.CreadoPor).HasMaxLength(100);
        builder.Property(g => g.ModificadoPor).HasMaxLength(100);
    }
}
