namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UnidadMedidaConfiguration : IEntityTypeConfiguration<UnidadMedida>
{
    public void Configure(EntityTypeBuilder<UnidadMedida> builder)
    {
        builder.ToTable("UnidadesMedida", "mdm");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Abreviatura)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(u => u.CreadoPor).HasMaxLength(100);
        builder.Property(u => u.ModificadoPor).HasMaxLength(100);

        // Nombre único por empresa
        builder.HasIndex(u => new { u.EmpresaId, u.Nombre }).IsUnique();
        builder.HasIndex(u => new { u.EmpresaId, u.Abreviatura }).IsUnique();
    }
}
