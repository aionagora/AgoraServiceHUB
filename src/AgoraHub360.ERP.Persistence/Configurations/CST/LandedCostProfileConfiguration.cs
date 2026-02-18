namespace AgoraHub360.ERP.Persistence.Configurations.CST;

using AgoraHub360.ERP.Domain.Entities.CST;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class LandedCostProfileConfiguration : IEntityTypeConfiguration<LandedCostProfile>
{
    public void Configure(EntityTypeBuilder<LandedCostProfile> builder)
    {
        builder.ToTable("LandedCostProfiles", "cst");
        builder.HasKey(lp => lp.ProfileId);
        builder.Property(lp => lp.ProfileId).UseIdentityColumn();

        builder.Property(lp => lp.Name).IsRequired().HasMaxLength(60);
        builder.Property(lp => lp.CreadoPor).HasMaxLength(100);
        builder.Property(lp => lp.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(lp => new { lp.EmpresaId, lp.IsDefault });
    }
}
