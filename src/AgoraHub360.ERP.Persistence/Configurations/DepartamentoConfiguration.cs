using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations;

public class DepartamentoConfiguration : IEntityTypeConfiguration<Departamento>
{
    public void Configure(EntityTypeBuilder<Departamento> builder)
    {
        builder.ToTable("Departamentos", "core");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(300);

        builder.HasOne(x => x.Pais)
            .WithMany(x => x.Departamentos)
            .HasForeignKey(x => x.PaisId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Departamentos_Paises");

        builder.HasIndex(x => new { x.PaisId, x.Nombre })
            .IsUnique()
            .HasDatabaseName("UX_Departamentos_Pais_Nombre");
    }
}
