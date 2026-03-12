namespace AgoraHub360.ERP.Persistence.Configurations.LOG;

using AgoraHub360.ERP.Domain.Entities.LOG;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class HojaRutaHistorialConfiguration : IEntityTypeConfiguration<HojaRutaHistorial>
{
    public void Configure(EntityTypeBuilder<HojaRutaHistorial> builder)
    {
        builder.ToTable("HojaRutaHistorial", "log");
        builder.HasKey(e => e.HojaRutaHistorialId);
        builder.Property(e => e.HojaRutaHistorialId).UseIdentityColumn();

        builder.Property(e => e.EstadoAnterior).IsRequired().HasMaxLength(20);
        builder.Property(e => e.SubEstadoAnterior).IsRequired().HasMaxLength(20);
        builder.Property(e => e.EstadoNuevo).IsRequired().HasMaxLength(20);
        builder.Property(e => e.SubEstadoNuevo).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Usuario).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Observaciones).HasMaxLength(2000);
        builder.Property(e => e.CreadoPor).HasMaxLength(100);
        builder.Property(e => e.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(e => e.HojaRutaId);
    }
}
