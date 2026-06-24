using AgoraHub360.ERP.Domain.Entities.VTA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations.VTA;

public class SiatMetodoPagoConfiguration : IEntityTypeConfiguration<SiatMetodoPago>
{
    public void Configure(EntityTypeBuilder<SiatMetodoPago> builder)
    {
        builder.ToTable("SiatMetodosPago", "vta");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Descripcion)
            .HasMaxLength(500);

        builder.Property(x => x.ModoPago)
            .HasMaxLength(50);

        builder.Property(x => x.EsPredeterminado)
            .HasDefaultValue(false);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique();
        builder.HasIndex(x => new { x.EmpresaId, x.ModoPago });
        builder.HasIndex(x => new { x.EmpresaId, x.EsPredeterminado });
    }
}
