namespace AgoraHub360.ERP.Persistence.Configurations.INV;

using AgoraHub360.ERP.Domain.Entities.INV;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> builder)
    {
        builder.ToTable("MovimientosInventario", "inv");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).UseIdentityColumn();

        builder.Property(m => m.Number).IsRequired().HasMaxLength(30);
        builder.Property(m => m.MovementType).IsRequired().HasMaxLength(20);
        builder.Property(m => m.MovementDate).IsRequired();

        builder.Property(m => m.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(m => m.UnitCost).HasColumnType("decimal(18,4)");
        builder.Property(m => m.TotalCost).HasColumnType("decimal(18,4)");

        builder.Property(m => m.Reference).HasMaxLength(50);
        builder.Property(m => m.Notes).HasMaxLength(500);
        builder.Property(m => m.CreadoPor).HasMaxLength(100);
        builder.Property(m => m.ModificadoPor).HasMaxLength(100);

        // Indexes
        builder.HasIndex(m => new { m.EmpresaId, m.Number }).IsUnique();
        builder.HasIndex(m => new { m.EmpresaId, m.CompanyProductId, m.MovementDate });
        builder.HasIndex(m => new { m.EmpresaId, m.WarehouseId });

        // Relations (no cascade delete to preserve history)
        builder.HasOne(m => m.CompanyProduct)
            .WithMany()
            .HasForeignKey(m => m.CompanyProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Warehouse)
            .WithMany()
            .HasForeignKey(m => m.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.DestinationWarehouse)
            .WithMany()
            .HasForeignKey(m => m.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
