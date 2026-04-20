namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UsuarioSucursalAccesoConfiguration : IEntityTypeConfiguration<UsuarioSucursalAcceso>
{
    public void Configure(EntityTypeBuilder<UsuarioSucursalAcceso> builder)
    {
        builder.ToTable("UsuariosSucursalesAccesos", "core");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.Property(x => x.PuedeConsultar)
            .HasDefaultValue(true);

        builder.Property(x => x.PuedeOperar)
            .HasDefaultValue(true);

        builder.HasOne(x => x.Usuario)
            .WithMany(u => u.SucursalesAcceso)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Sucursal)
            .WithMany()
            .HasForeignKey(x => x.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UsuarioId, x.EmpresaId, x.SucursalId })
            .IsUnique()
            .HasDatabaseName("IX_UsuarioSucursalAcceso_UQ");

        builder.HasIndex(x => new { x.UsuarioId, x.EmpresaId, x.EsPredeterminada })
            .IsUnique()
            .HasDatabaseName("IX_UsuarioSucursalAcceso_Default")
            .HasFilter("[EsPredeterminada] = 1 AND [Activo] = 1");
    }
}
