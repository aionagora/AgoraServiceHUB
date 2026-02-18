namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UomConfiguration : IEntityTypeConfiguration<Uom>
{
    public void Configure(EntityTypeBuilder<Uom> builder)
    {
        builder.ToTable("Uoms", "mdm");
        builder.HasKey(u => u.UomId);
        builder.Property(u => u.UomId).UseIdentityColumn();

        builder.Property(u => u.Code).IsRequired().HasMaxLength(10);
        builder.Property(u => u.Nombre).IsRequired().HasMaxLength(40);
        builder.Property(u => u.CreadoPor).HasMaxLength(100);
        builder.Property(u => u.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(u => u.Code).IsUnique();
    }
}
