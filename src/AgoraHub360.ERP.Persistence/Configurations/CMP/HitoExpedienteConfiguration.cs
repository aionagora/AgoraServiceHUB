namespace AgoraHub360.ERP.Persistence.Configurations.CMP;

using AgoraHub360.ERP.Domain.Entities.CMP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class HitoExpedienteConfiguration : IEntityTypeConfiguration<HitoExpediente>
{
    public void Configure(EntityTypeBuilder<HitoExpediente> builder)
    {
        builder.ToTable("HitosExpediente", "cmp");
        builder.HasKey(h => h.HitoExpedienteId);
        builder.Property(h => h.HitoExpedienteId).UseIdentityColumn();

        builder.Property(h => h.TipoHito).IsRequired().HasMaxLength(50);
        builder.Property(h => h.Descripcion).HasMaxLength(1000);
        builder.Property(h => h.ReferenciaDocumento).HasMaxLength(200);
        builder.Property(h => h.CreadoPor).HasMaxLength(100);
        builder.Property(h => h.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(h => new { h.ExpedienteImportacionId, h.FechaHito });
    }
}
