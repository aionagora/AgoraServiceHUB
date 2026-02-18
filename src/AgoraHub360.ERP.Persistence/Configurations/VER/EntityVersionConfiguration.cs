namespace AgoraHub360.ERP.Persistence.Configurations.VER;

using AgoraHub360.ERP.Domain.Entities.VER;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EntityVersionConfiguration : IEntityTypeConfiguration<EntityVersion>
{
    public void Configure(EntityTypeBuilder<EntityVersion> builder)
    {
        builder.ToTable("EntityVersions", "ver");
        builder.HasKey(v => v.VersionId);
        builder.Property(v => v.VersionId).UseIdentityColumn();

        builder.Property(v => v.EntityName).IsRequired().HasMaxLength(80);
        builder.Property(v => v.EntityId).IsRequired().HasMaxLength(80);
        builder.Property(v => v.SnapshotJson).IsRequired();
        builder.Property(v => v.ChangedBy).HasMaxLength(120);

        builder.HasIndex(v => new { v.EntityName, v.EntityId, v.VersionNo }).IsUnique();
        builder.HasIndex(v => new { v.EntityName, v.EntityId });
        builder.HasIndex(v => v.EmpresaId);
    }
}
