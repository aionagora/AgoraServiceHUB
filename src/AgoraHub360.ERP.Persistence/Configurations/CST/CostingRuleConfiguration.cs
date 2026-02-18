namespace AgoraHub360.ERP.Persistence.Configurations.CST;

using AgoraHub360.ERP.Domain.Entities.CST;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CostingRuleConfiguration : IEntityTypeConfiguration<CostingRule>
{
    public void Configure(EntityTypeBuilder<CostingRule> builder)
    {
        builder.ToTable("CostingRules", "cst");
        builder.HasKey(r => new { r.EmpresaId, r.ProductKind });

        builder.Property(r => r.CreadoPor).HasMaxLength(100);
        builder.Property(r => r.ModificadoPor).HasMaxLength(100);
    }
}
