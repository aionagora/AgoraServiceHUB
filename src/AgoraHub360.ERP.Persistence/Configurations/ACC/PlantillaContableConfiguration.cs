namespace AgoraHub360.ERP.Persistence.Configurations.ACC;

using AgoraHub360.ERP.Domain.Entities.ACC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PlantillaContableConfiguration : IEntityTypeConfiguration<PlantillaContable>
{
    public void Configure(EntityTypeBuilder<PlantillaContable> builder)
    {
        builder.ToTable("PlantillasContables", "acc");
        builder.HasKey(p => p.PlantillaContableId);
        builder.Property(p => p.PlantillaContableId).UseIdentityColumn();

        builder.Property(p => p.Codigo).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(p => p.TipoDocumento).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Descripcion).HasMaxLength(500);
        builder.Property(p => p.GlosaPlantilla).IsRequired().HasMaxLength(500);
        builder.Property(p => p.CreadoPor).HasMaxLength(100);
        builder.Property(p => p.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(p => new { p.EmpresaId, p.Codigo }).IsUnique();
        builder.HasIndex(p => new { p.EmpresaId, p.TipoDocumento });

        builder.HasMany(p => p.Lineas)
            .WithOne(l => l.PlantillaContable)
            .HasForeignKey(l => l.PlantillaContableId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
