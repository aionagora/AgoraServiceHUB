namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClienteSucursalConfiguration : IEntityTypeConfiguration<ClienteSucursal>
{
    public void Configure(EntityTypeBuilder<ClienteSucursal> builder)
    {
        builder.ToTable("ClienteSucursales", "mdm");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Direccion)
            .HasMaxLength(500);

        builder.Property(x => x.Referencia)
            .HasMaxLength(500);

        builder.Property(x => x.Observaciones)
            .HasMaxLength(1000);

        builder.Property(x => x.Latitud)
            .HasPrecision(11, 8);

        builder.Property(x => x.Longitud)
            .HasPrecision(11, 8);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasOne(x => x.Cliente)
            .WithMany(c => c.Sucursales)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EmpresaId, x.ClienteId, x.Codigo })
            .IsUnique()
            .HasDatabaseName("IX_ClienteSucursal_Empresa_Cliente_Codigo");

        builder.HasIndex(x => new { x.EmpresaId, x.ClienteId, x.EsPrincipal })
            .IsUnique()
            .HasDatabaseName("IX_ClienteSucursal_Empresa_Cliente_Principal")
            .HasFilter("[EsPrincipal] = 1 AND [Activo] = 1");
    }
}
