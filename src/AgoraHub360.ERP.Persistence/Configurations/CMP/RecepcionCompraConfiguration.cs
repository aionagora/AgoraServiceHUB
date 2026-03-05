namespace AgoraHub360.ERP.Persistence.Configurations.CMP;

using AgoraHub360.ERP.Domain.Entities.CMP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RecepcionCompraConfiguration : IEntityTypeConfiguration<RecepcionCompra>
{
    public void Configure(EntityTypeBuilder<RecepcionCompra> builder)
    {
        builder.ToTable("RecepcionesCompra", "cmp");
        builder.HasKey(r => r.RecepcionCompraId);
        builder.Property(r => r.RecepcionCompraId).UseIdentityColumn();

        builder.Property(r => r.Numero).IsRequired().HasMaxLength(30);
        builder.Property(r => r.FechaRecepcion).IsRequired();
        builder.Property(r => r.DocumentoProveedor).HasMaxLength(100);
        builder.Property(r => r.Observaciones).HasMaxLength(1000);
        builder.Property(r => r.TipoDiferencia).HasMaxLength(50);
        builder.Property(r => r.ActaDiferencias).HasMaxLength(2000);
        builder.Property(r => r.NumeroReclamo).HasMaxLength(100);
        builder.Property(r => r.UbicacionCuarentena).HasMaxLength(200);
        builder.Property(r => r.ResultadoControlCalidad).HasMaxLength(50);
        builder.Property(r => r.CreadoPor).HasMaxLength(100);
        builder.Property(r => r.ModificadoPor).HasMaxLength(100);

        // Indexes
        builder.HasIndex(r => new { r.EmpresaId, r.Numero }).IsUnique();
        builder.HasIndex(r => new { r.EmpresaId, r.OrdenCompraId });

        // Relations
        builder.HasOne(r => r.OrdenCompra)
            .WithMany()
            .HasForeignKey(r => r.OrdenCompraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Almacen)
            .WithMany()
            .HasForeignKey(r => r.AlmacenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Lineas)
            .WithOne(l => l.RecepcionCompra)
            .HasForeignKey(l => l.RecepcionCompraId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
