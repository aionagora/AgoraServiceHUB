namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UsuarioEmpresaConfiguration : IEntityTypeConfiguration<UsuarioEmpresa>
{
    public void Configure(EntityTypeBuilder<UsuarioEmpresa> builder)
    {
        builder.ToTable("UsuarioEmpresas", "core");

        builder.HasKey(ue => new { ue.UsuarioId, ue.EmpresaId });

        builder.Property(ue => ue.Rol)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Viewer");

        builder.HasOne(ue => ue.Usuario)
            .WithMany(u => u.Empresas)
            .HasForeignKey(ue => ue.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ue => ue.Empresa)
            .WithMany()
            .HasForeignKey(ue => ue.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
