namespace AgoraHub360.ERP.Persistence.Configurations.ACC;

using AgoraHub360.ERP.Domain.Entities.ACC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TipoPagoConfiguration : IEntityTypeConfiguration<TipoPago>
{
    public void Configure(EntityTypeBuilder<TipoPago> builder)
    {
        builder.ToTable("TiposPago", "acc");
        builder.HasKey(t => t.TipoPagoId);
        builder.Property(t => t.TipoPagoId).UseIdentityColumn();

        builder.Property(t => t.Codigo).IsRequired().HasMaxLength(10);
        builder.Property(t => t.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(t => t.CreadoPor).HasMaxLength(100);
        builder.Property(t => t.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(t => new { t.EmpresaId, t.Codigo }).IsUnique();
    }
}
