using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations;

public class CiudadConfiguration : IEntityTypeConfiguration<Ciudad>
{
    public void Configure(EntityTypeBuilder<Ciudad> builder)
    {
        builder.ToTable("Ciudades", "core");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(300);

        builder.HasOne(x => x.Provincia)
            .WithMany(x => x.Ciudades)
            .HasForeignKey(x => x.ProvinciaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Ciudades_Provincias");

        builder.HasIndex(x => new { x.ProvinciaId, x.Nombre })
            .IsUnique()
            .HasDatabaseName("UX_Ciudades_Provincia_Nombre");
    }
}
