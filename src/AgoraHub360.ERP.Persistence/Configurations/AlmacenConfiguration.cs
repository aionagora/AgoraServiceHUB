namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AlmacenConfiguration : IEntityTypeConfiguration<Almacen>
{
    public void Configure(EntityTypeBuilder<Almacen> builder)
    {
        builder.ToTable("Almacenes", "mdm");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Codigo).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Direccion).HasMaxLength(300);
        builder.Property(a => a.Responsable).HasMaxLength(200);
        builder.Property(a => a.Telefono).HasMaxLength(50);
        builder.Property(a => a.CreadoPor).HasMaxLength(100);
        builder.Property(a => a.ModificadoPor).HasMaxLength(100);
        builder.HasIndex(a => new { a.EmpresaId, a.Codigo }).IsUnique();
    }
}
