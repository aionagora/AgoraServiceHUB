namespace AgoraHub360.ERP.Persistence.Configurations.RUL;

using AgoraHub360.ERP.Domain.Entities.RUL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductIndustryRuleConfiguration : IEntityTypeConfiguration<ProductIndustryRule>
{
    public void Configure(EntityTypeBuilder<ProductIndustryRule> builder)
    {
        builder.ToTable("ProductIndustryRules", "rul");
        builder.HasKey(r => r.RuleId);
        builder.Property(r => r.RuleId).UseIdentityColumn();

        builder.Property(r => r.ConditionJson).IsRequired();
        builder.Property(r => r.ActionsJson).IsRequired();
        builder.Property(r => r.CreadoPor).HasMaxLength(100);
        builder.Property(r => r.ModificadoPor).HasMaxLength(100);

        builder.HasOne(r => r.Industry)
            .WithMany(i => i.Rules)
            .HasForeignKey(r => r.IndustryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.IndustryId, r.Priority });
    }
}
