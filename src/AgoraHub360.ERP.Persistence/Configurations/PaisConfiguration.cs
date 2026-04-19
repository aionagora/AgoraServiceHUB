using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations;

public class PaisConfiguration : IEntityTypeConfiguration<Pais>
{
    public void Configure(EntityTypeBuilder<Pais> builder)
    {
        builder.ToTable("Paises", "core");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(300);
        builder.Property(x => x.CodigoIso2).HasMaxLength(20);
        builder.Property(x => x.CodigoIso3).HasMaxLength(20);

        builder.HasIndex(x => x.Nombre).IsUnique().HasDatabaseName("UX_Paises_Nombre");
    }
}
