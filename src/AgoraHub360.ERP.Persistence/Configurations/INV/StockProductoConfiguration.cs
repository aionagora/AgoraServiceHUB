namespace AgoraHub360.ERP.Persistence.Configurations.INV;

using AgoraHub360.ERP.Domain.Entities.INV;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StockProductoConfiguration : IEntityTypeConfiguration<StockProducto>
{
    public void Configure(EntityTypeBuilder<StockProducto> builder)
    {
        builder.ToTable("StockProductos", "inv");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).UseIdentityColumn();

        builder.Property(s => s.CurrentStock).HasColumnType("decimal(18,4)");
        builder.Property(s => s.AverageCost).HasColumnType("decimal(18,4)");
        builder.Property(s => s.LastUpdated).IsRequired();
        builder.Property(s => s.CreadoPor).HasMaxLength(100);
        builder.Property(s => s.ModificadoPor).HasMaxLength(100);

        // Índice único: un registro de stock por producto+almacén+empresa
        builder.HasIndex(s => new { s.EmpresaId, s.CompanyProductId, s.AlmacenId }).IsUnique();

        builder.HasOne(s => s.CompanyProduct)
            .WithMany()
            .HasForeignKey(s => s.CompanyProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Almacen)
            .WithMany()
            .HasForeignKey(s => s.AlmacenId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
