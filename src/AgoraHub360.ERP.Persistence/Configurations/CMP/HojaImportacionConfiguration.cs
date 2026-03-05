namespace AgoraHub360.ERP.Persistence.Configurations.CMP;

using AgoraHub360.ERP.Domain.Entities.CMP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class HojaImportacionConfiguration : IEntityTypeConfiguration<HojaImportacion>
{
    public void Configure(EntityTypeBuilder<HojaImportacion> builder)
    {
        builder.ToTable("HojasImportacion", "cmp");
        builder.HasKey(h => h.HojaImportacionId);
        builder.Property(h => h.HojaImportacionId).UseIdentityColumn();

        builder.Property(h => h.Numero).IsRequired().HasMaxLength(30);
        builder.Property(h => h.Fecha).IsRequired();
        builder.Property(h => h.ReferenciaAduanera).HasMaxLength(100);
        builder.Property(h => h.Observaciones).HasMaxLength(1000);
        builder.Property(h => h.TotalGastos).HasColumnType("decimal(18,4)");
        builder.Property(h => h.CreadoPor).HasMaxLength(100);
        builder.Property(h => h.ModificadoPor).HasMaxLength(100);

        // Indexes
        builder.HasIndex(h => new { h.EmpresaId, h.Numero }).IsUnique();
        builder.HasIndex(h => new { h.EmpresaId, h.OrdenCompraId });
        builder.HasIndex(h => new { h.EmpresaId, h.ExpedienteImportacionId });

        // Relations
        builder.HasOne(h => h.OrdenCompra)
            .WithMany()
            .HasForeignKey(h => h.OrdenCompraId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasMany(h => h.Gastos)
            .WithOne(g => g.HojaImportacion)
            .HasForeignKey(g => g.HojaImportacionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.Lineas)
            .WithOne(l => l.HojaImportacion)
            .HasForeignKey(l => l.HojaImportacionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
