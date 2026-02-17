namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UbicacionAlmacenConfiguration : IEntityTypeConfiguration<UbicacionAlmacen>
{
    public void Configure(EntityTypeBuilder<UbicacionAlmacen> builder)
    {
        builder.ToTable("UbicacionesAlmacen", "mdm");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Codigo).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Descripcion).HasMaxLength(500);
        builder.Property(u => u.CreadoPor).HasMaxLength(100);
        builder.Property(u => u.ModificadoPor).HasMaxLength(100);
        builder.HasIndex(u => new { u.EmpresaId, u.AlmacenId, u.Codigo }).IsUnique();
        builder.HasIndex(u => u.AlmacenId);
    }
}
