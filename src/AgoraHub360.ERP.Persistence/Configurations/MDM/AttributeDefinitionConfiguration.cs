namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AttributeDefinitionConfiguration : IEntityTypeConfiguration<AttributeDefinition>
{
    public void Configure(EntityTypeBuilder<AttributeDefinition> builder)
    {
        builder.ToTable("AttributeDefinitions", "mdm");
        builder.HasKey(a => a.AttributeId);
        builder.Property(a => a.AttributeId).UseIdentityColumn();

        builder.Property(a => a.Code).IsRequired().HasMaxLength(60);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(120);
        builder.Property(a => a.ValidationRegex).HasMaxLength(200);
        builder.Property(a => a.UnitHint).HasMaxLength(20);
        builder.Property(a => a.MinValue).HasPrecision(18, 4);
        builder.Property(a => a.MaxValue).HasPrecision(18, 4);
        builder.Property(a => a.CreadoPor).HasMaxLength(100);
        builder.Property(a => a.ModificadoPor).HasMaxLength(100);

        // Código único por industria
        builder.HasIndex(a => new { a.IndustryId, a.Code }).IsUnique();
        builder.HasIndex(a => a.IsVariantAxis);
    }
}
