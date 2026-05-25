namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresas", "core");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.NIT)
            .HasMaxLength(50);

        builder.Property(e => e.Direccion)
            .HasMaxLength(500);

        builder.Property(e => e.Telefono)
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .HasMaxLength(200);

        builder.Property(e => e.CreadoPor).HasMaxLength(100);
        builder.Property(e => e.ModificadoPor).HasMaxLength(100);

        builder.HasOne(e => e.MonedaBase)
            .WithMany()
            .HasForeignKey(e => e.MonedaBaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.NIT).IsUnique().HasFilter("[NIT] IS NOT NULL");
        builder.HasIndex(e => e.Nombre);

        // ?? Configuración de productos ??????????????????????????????????????
        builder.Property(e => e.IndustriaId).HasDefaultValue((byte)7);
        builder.Property(e => e.MetodoCosteoDefault).HasDefaultValue((byte)1);
        builder.Property(e => e.PermiteVariantes).HasDefaultValue(true);
        builder.Property(e => e.PermiteLotes).HasDefaultValue(false);
        builder.Property(e => e.PermiteServicios).HasDefaultValue(true);
        builder.Property(e => e.AutoGeneraSku).HasDefaultValue(true);
        builder.Property(e => e.PrefijoSku).HasMaxLength(20);
    }
}
