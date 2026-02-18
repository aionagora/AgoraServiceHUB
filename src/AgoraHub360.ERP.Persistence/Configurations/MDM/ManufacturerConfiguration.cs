namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ManufacturerConfiguration : IEntityTypeConfiguration<Manufacturer>
{
    public void Configure(EntityTypeBuilder<Manufacturer> builder)
    {
        builder.ToTable("Manufacturers", "mdm");
        builder.HasKey(m => m.ManufacturerId);
        builder.Property(m => m.ManufacturerId).UseIdentityColumn();

        builder.Property(m => m.Nombre).IsRequired().HasMaxLength(160);
        builder.Property(m => m.Pais).HasMaxLength(80);
        builder.Property(m => m.CreadoPor).HasMaxLength(100);
        builder.Property(m => m.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(m => m.Nombre);
    }
}
