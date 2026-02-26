namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class VariantAttributeValueConfiguration : IEntityTypeConfiguration<VariantAttributeValue>
{
    public void Configure(EntityTypeBuilder<VariantAttributeValue> builder)
    {
        builder.ToTable("VariantAttributeValues", "mdm");
        builder.HasKey(v => new { v.VariantId, v.AttributeId });

        builder.Property(v => v.ValueString).HasMaxLength(120);

        builder.HasOne(v => v.Variant)
            .WithMany(pv => pv.AttributeValues)
            .HasForeignKey(v => v.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.AttributeDefinition)
            .WithMany()
            .HasForeignKey(v => v.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Option)
            .WithMany()
            .HasForeignKey(v => v.OptionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
