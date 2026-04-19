using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations;

public class ZonaConfiguration : IEntityTypeConfiguration<Zona>
{
    public void Configure(EntityTypeBuilder<Zona> builder)
    {
        builder.ToTable("Zonas", "core");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(300);

        builder.HasOne(x => x.Ciudad)
            .WithMany(x => x.Zonas)
            .HasForeignKey(x => x.CiudadId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Zonas_Ciudades");

        builder.HasIndex(x => new { x.CiudadId, x.Nombre })
            .IsUnique()
            .HasDatabaseName("UX_Zonas_Ciudad_Nombre");
    }
}
