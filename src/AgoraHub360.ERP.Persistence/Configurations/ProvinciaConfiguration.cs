using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations;

public class ProvinciaConfiguration : IEntityTypeConfiguration<Provincia>
{
    public void Configure(EntityTypeBuilder<Provincia> builder)
    {
        builder.ToTable("Provincias", "core");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(300);

        builder.HasOne(x => x.Departamento)
            .WithMany(x => x.Provincias)
            .HasForeignKey(x => x.DepartamentoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Provincias_Departamentos");

        builder.HasIndex(x => new { x.DepartamentoId, x.Nombre })
            .IsUnique()
            .HasDatabaseName("UX_Provincias_Departamento_Nombre");
    }
}
