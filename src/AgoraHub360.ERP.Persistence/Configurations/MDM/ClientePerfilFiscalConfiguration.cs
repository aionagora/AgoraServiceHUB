namespace AgoraHub360.ERP.Persistence.Configurations.MDM;

using AgoraHub360.ERP.Domain.Entities.MDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClientePerfilFiscalConfiguration : IEntityTypeConfiguration<ClientePerfilFiscal>
{
    public void Configure(EntityTypeBuilder<ClientePerfilFiscal> builder)
    {
        builder.ToTable("ClientePerfilesFiscales", "mdm");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.Alias)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.TipoDocumentoIdentidad)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.NumeroDocumento)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Complemento)
            .HasMaxLength(20);

        builder.Property(x => x.RazonSocial)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TipoPersona)
            .HasMaxLength(20);

        builder.Property(x => x.TipoPerfilFiscal)
            .HasMaxLength(50);

        builder.Property(x => x.EmailFactura)
            .HasMaxLength(200);

        builder.Property(x => x.TelefonoFactura)
            .HasMaxLength(50);

        builder.Property(x => x.RequiereEmail)
            .HasDefaultValue(false);

        builder.Property(x => x.EsPredeterminado)
            .HasDefaultValue(false);

        builder.Property(x => x.ValidadoFacturacion)
            .HasDefaultValue(false);

        builder.Property(x => x.CodigoClienteApi)
            .HasMaxLength(100);

        builder.Property(x => x.CodigoExternoFacturacion)
            .HasMaxLength(100);

        builder.Property(x => x.Observaciones)
            .HasMaxLength(1000);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        builder.HasOne(x => x.Cliente)
            .WithMany(c => c.PerfilesFiscales)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ClienteSucursal)
            .WithMany(s => s.PerfilesFiscales)
            .HasForeignKey(x => x.ClienteSucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EmpresaId, x.ClienteId })
            .HasDatabaseName("IX_ClientePerfilFiscal_Empresa_Cliente");

        builder.HasIndex(x => new { x.EmpresaId, x.NumeroDocumento })
            .HasDatabaseName("IX_ClientePerfilFiscal_Empresa_Documento");

        builder.HasIndex(x => new { x.EmpresaId, x.ClienteId, x.NumeroDocumento })
            .HasDatabaseName("IX_ClientePerfilFiscal_Empresa_Cliente_Documento");

        builder.HasIndex(x => new { x.EmpresaId, x.ClienteId, x.EsPredeterminado })
            .HasDatabaseName("IX_ClientePerfilFiscal_Empresa_Cliente_EsPredeterminado");

        builder.HasIndex(x => new { x.EmpresaId, x.ClienteId, x.NumeroDocumento, x.RazonSocial, x.Activo })
            .IsUnique()
            .HasDatabaseName("UX_ClientePerfilFiscal_Empresa_Cliente_Documento_RazonSocial_Activo");
    }
}
