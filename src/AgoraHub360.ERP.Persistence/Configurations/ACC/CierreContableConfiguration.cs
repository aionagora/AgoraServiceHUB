namespace AgoraHub360.ERP.Persistence.Configurations.ACC;

using AgoraHub360.ERP.Domain.Entities.ACC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CierreContableConfiguration : IEntityTypeConfiguration<CierreContable>
{
    public void Configure(EntityTypeBuilder<CierreContable> builder)
    {
        builder.ToTable("ACC_CierresContables");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Gestion).IsRequired();
        builder.Property(c => c.FechaCierre).IsRequired();
        
        builder.Property(c => c.Estado).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Observaciones).HasMaxLength(500);

        builder.HasIndex(c => new { c.EmpresaId, c.Gestion }).IsUnique();
    }
}
