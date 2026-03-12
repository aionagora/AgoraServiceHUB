namespace AgoraHub360.ERP.Persistence.Configurations.LOG;

using AgoraHub360.ERP.Domain.Entities.LOG;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class HojaRutaConfiguration : IEntityTypeConfiguration<HojaRuta>
{
    public void Configure(EntityTypeBuilder<HojaRuta> builder)
    {
        builder.ToTable("HojasRuta", "log");
        builder.HasKey(e => e.HojaRutaId);
        builder.Property(e => e.HojaRutaId).UseIdentityColumn();

        builder.Property(e => e.NumeroHojaRuta).IsRequired().HasMaxLength(20);
        builder.Property(e => e.TipoOP).IsRequired().HasMaxLength(20);
        builder.Property(e => e.SubTipo).IsRequired(false).HasMaxLength(30);
        builder.Property(e => e.ProveedorCliente).HasMaxLength(200);
        builder.Property(e => e.DireccionEntrega).HasMaxLength(300);
        builder.Property(e => e.ContactoCliente).HasMaxLength(150);
        builder.Property(e => e.ResponsableUsuario).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Estado).IsRequired().HasMaxLength(20);
        builder.Property(e => e.SubEstado).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Observaciones).HasMaxLength(2000);
        builder.Property(e => e.CreadoPor).HasMaxLength(100);
        builder.Property(e => e.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(e => new { e.EmpresaId, e.NumeroHojaRuta }).IsUnique();
        builder.HasIndex(e => new { e.EmpresaId, e.Estado });
        builder.HasIndex(e => new { e.EmpresaId, e.TipoOP });
        builder.HasIndex(e => new { e.EmpresaId, e.FechaDocumento });

        builder.Property(e => e.OrdenPedidoId).IsRequired(false);
        builder.HasIndex(e => new { e.EmpresaId, e.OrdenPedidoId })
               .HasDatabaseName("IX_HojasRuta_OrdenPedidoId");

        builder.HasMany(e => e.Historial)
            .WithOne(h => h.HojaRuta)
            .HasForeignKey(h => h.HojaRutaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
