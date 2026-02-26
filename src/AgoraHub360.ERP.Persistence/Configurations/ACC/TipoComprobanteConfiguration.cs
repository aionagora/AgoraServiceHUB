namespace AgoraHub360.ERP.Persistence.Configurations.ACC;

using AgoraHub360.ERP.Domain.Entities.ACC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TipoComprobanteConfiguration : IEntityTypeConfiguration<TipoComprobante>
{
    public void Configure(EntityTypeBuilder<TipoComprobante> builder)
    {
        builder.ToTable("TiposComprobante", "acc");
        builder.HasKey(t => t.TipoComprobanteId);
        builder.Property(t => t.TipoComprobanteId).UseIdentityColumn();

        builder.Property(t => t.Codigo).IsRequired().HasMaxLength(10);
        builder.Property(t => t.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Prefijo).IsRequired().HasMaxLength(10);
        builder.Property(t => t.CreadoPor).HasMaxLength(100);
        builder.Property(t => t.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(t => new { t.EmpresaId, t.Codigo }).IsUnique();
    }
}
