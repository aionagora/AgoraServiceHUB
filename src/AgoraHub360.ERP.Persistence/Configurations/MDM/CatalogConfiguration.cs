namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CatalogConfiguration : IEntityTypeConfiguration<Catalog>
{
    public void Configure(EntityTypeBuilder<Catalog> builder)
    {
        builder.ToTable("Catalogs", "mdm");
        builder.HasKey(c => c.CatalogId);
        builder.Property(c => c.CatalogId).UseIdentityColumn();

        builder.Property(c => c.Scope).IsRequired();
        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(120);
        builder.Property(c => c.CreadoPor).HasMaxLength(100);
        builder.Property(c => c.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(c => new { c.Scope, c.EmpresaId, c.Nombre }).IsUnique();
    }
}
