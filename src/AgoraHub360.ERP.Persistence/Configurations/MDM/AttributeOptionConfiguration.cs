namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AttributeOptionConfiguration : IEntityTypeConfiguration<AttributeOption>
{
    public void Configure(EntityTypeBuilder<AttributeOption> builder)
    {
        builder.ToTable("AttributeOptions", "mdm");
        builder.HasKey(o => o.OptionId);
        builder.Property(o => o.OptionId).UseIdentityColumn();

        builder.Property(o => o.Value).IsRequired().HasMaxLength(120);
        builder.Property(o => o.CreadoPor).HasMaxLength(100);
        builder.Property(o => o.ModificadoPor).HasMaxLength(100);

        builder.HasOne(o => o.AttributeDefinition)
            .WithMany(a => a.Options)
            .HasForeignKey(o => o.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.AttributeId, o.Value }).IsUnique();
    }
}
