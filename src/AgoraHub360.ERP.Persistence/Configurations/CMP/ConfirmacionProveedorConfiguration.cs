namespace AgoraHub360.ERP.Persistence.Configurations.CMP;

using AgoraHub360.ERP.Domain.Entities.CMP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ConfirmacionProveedorConfiguration : IEntityTypeConfiguration<ConfirmacionProveedor>
{
    public void Configure(EntityTypeBuilder<ConfirmacionProveedor> builder)
    {
        builder.ToTable("ConfirmacionesProveedor", "cmp");
        builder.HasKey(c => c.ConfirmacionProveedorId);
        builder.Property(c => c.ConfirmacionProveedorId).UseIdentityColumn();

        builder.Property(c => c.NumeroProforma).HasMaxLength(100);
        builder.Property(c => c.ObservacionesProveedor).HasMaxLength(2000);
        builder.Property(c => c.Observaciones).HasMaxLength(1000);
        builder.Property(c => c.CreadoPor).HasMaxLength(100);
        builder.Property(c => c.ModificadoPor).HasMaxLength(100);

        builder.Property(c => c.Estado)
            .IsRequired()
            .HasConversion<byte>();

        builder.HasIndex(c => new { c.OrdenCompraId, c.IteracionNegociacion });

        builder.HasOne(c => c.OrdenCompra)
            .WithMany(o => o.ConfirmacionesProveedor)
            .HasForeignKey(c => c.OrdenCompraId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
