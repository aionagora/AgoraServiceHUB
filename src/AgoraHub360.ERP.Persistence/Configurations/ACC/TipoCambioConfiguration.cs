namespace AgoraHub360.ERP.Persistence.Configurations.ACC;

using AgoraHub360.ERP.Domain.Entities.ACC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TipoCambioConfiguration : IEntityTypeConfiguration<TipoCambio>
{
    public void Configure(EntityTypeBuilder<TipoCambio> builder)
    {
        builder.ToTable("TiposCambio", "acc");
        builder.HasKey(t => t.TipoCambioId);
        builder.Property(t => t.TipoCambioId).UseIdentityColumn();

        builder.Property(t => t.Moneda).IsRequired().HasMaxLength(10);
        builder.Property(t => t.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Simbolo).IsRequired().HasMaxLength(10);
        builder.Property(t => t.TasaCompra).HasColumnType("decimal(18,6)");
        builder.Property(t => t.TasaVenta).HasColumnType("decimal(18,6)");
        builder.Property(t => t.CreadoPor).HasMaxLength(100);
        builder.Property(t => t.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(t => new { t.EmpresaId, t.Moneda, t.FechaVigencia });
    }
}
