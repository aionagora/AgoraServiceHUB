namespace AgoraHub360.ERP.Persistence.Configurations.CMP;

using AgoraHub360.ERP.Domain.Entities.CMP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PagoOrdenCompraConfiguration : IEntityTypeConfiguration<PagoOrdenCompra>
{
    public void Configure(EntityTypeBuilder<PagoOrdenCompra> builder)
    {
        builder.ToTable("PagosOrdenCompra", "cmp");
        builder.HasKey(p => p.PagoOrdenCompraId);
        builder.Property(p => p.PagoOrdenCompraId).UseIdentityColumn();

        builder.Property(p => p.MonedaId).IsRequired().HasMaxLength(3);
        builder.Property(p => p.MontoProgramado).HasColumnType("decimal(18,4)");
        builder.Property(p => p.TasaCambio).HasColumnType("decimal(18,6)");
        builder.Property(p => p.MontoEjecutado).HasColumnType("decimal(18,4)");
        builder.Property(p => p.ReferenciaTransferencia).HasMaxLength(200);
        builder.Property(p => p.Observaciones).HasMaxLength(1000);
        builder.Property(p => p.CreadoPor).HasMaxLength(100);
        builder.Property(p => p.ModificadoPor).HasMaxLength(100);

        builder.Property(p => p.TipoPago)
            .IsRequired()
            .HasConversion<byte>();

        builder.HasIndex(p => new { p.EmpresaId, p.OrdenCompraId });
        builder.HasIndex(p => new { p.EmpresaId, p.Ejecutado });

        builder.HasOne(p => p.OrdenCompra)
            .WithMany(o => o.Pagos)
            .HasForeignKey(p => p.OrdenCompraId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
