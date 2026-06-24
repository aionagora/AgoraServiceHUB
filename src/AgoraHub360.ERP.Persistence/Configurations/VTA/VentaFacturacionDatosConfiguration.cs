using AgoraHub360.ERP.Domain.Entities.VTA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations.VTA;

public class VentaFacturacionDatosConfiguration : IEntityTypeConfiguration<VentaFacturacionDatos>
{
    public void Configure(EntityTypeBuilder<VentaFacturacionDatos> builder)
    {
        builder.ToTable("VentaFacturacionDatos", "vta");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.TipoDocumentoIdentidad).HasMaxLength(50);
        builder.Property(x => x.NitFactura).HasMaxLength(50);
        builder.Property(x => x.Complemento).HasMaxLength(50);
        builder.Property(x => x.RazonSocialFactura).HasMaxLength(250);
        builder.Property(x => x.EmailFactura).HasMaxLength(150);
        builder.Property(x => x.TelefonoFactura).HasMaxLength(50);

        builder.Property(x => x.EstadoFactura)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasOne(x => x.Venta)
            .WithOne(x => x.DatosFacturacion)
            .HasForeignKey<VentaFacturacionDatos>(x => x.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ClientePerfilFiscal)
            .WithMany()
            .HasForeignKey(x => x.ClientePerfilFiscalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.VentaId).IsUnique();
        builder.HasIndex(x => x.ClientePerfilFiscalId);
    }
}
