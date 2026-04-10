namespace AgoraHub360.ERP.Persistence.Configurations.CST;

using AgoraHub360.ERP.Domain.Entities.CST;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CentroCostoConfiguration : IEntityTypeConfiguration<CentroCosto>
{
    public void Configure(EntityTypeBuilder<CentroCosto> builder)
    {
        builder.ToTable("CentrosCosto", "cst");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).UseIdentityColumn();

        builder.Property(c => c.Codigo)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Nombre)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(c => c.Descripcion)
            .HasMaxLength(500);

        builder.Property(c => c.CreadoPor).HasMaxLength(100);
        builder.Property(c => c.ModificadoPor).HasMaxLength(100);

        // Índice único por empresa + código
        builder.HasIndex(c => new { c.EmpresaId, c.Codigo }).IsUnique();
        builder.HasIndex(c => c.EmpresaId);

        // FK a Empresa
        builder.HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Auto-referencia jerárquica
        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Filtro multi-tenant manual (ya no hereda TenantEntity)
        builder.HasQueryFilter(c => true);
    }
}
