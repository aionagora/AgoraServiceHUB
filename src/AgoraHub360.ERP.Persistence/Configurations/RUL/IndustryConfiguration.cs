namespace AgoraHub360.ERP.Persistence.Configurations.RUL;

using AgoraHub360.ERP.Domain.Entities.RUL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class IndustryConfiguration : IEntityTypeConfiguration<Industry>
{
    public void Configure(EntityTypeBuilder<Industry> builder)
    {
        builder.ToTable("Industries", "rul");
        builder.HasKey(i => i.IndustryId);
        builder.Property(i => i.IndustryId).UseIdentityColumn();

        builder.Property(i => i.Code).IsRequired().HasMaxLength(30);
        builder.Property(i => i.Name).IsRequired().HasMaxLength(80);
        builder.Property(i => i.CreadoPor).HasMaxLength(100);
        builder.Property(i => i.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(i => i.Code).IsUnique();
    }
}
