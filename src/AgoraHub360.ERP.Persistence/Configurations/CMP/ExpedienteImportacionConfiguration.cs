namespace AgoraHub360.ERP.Persistence.Configurations.CMP;

using AgoraHub360.ERP.Domain.Entities.CMP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExpedienteImportacionConfiguration : IEntityTypeConfiguration<ExpedienteImportacion>
{
    public void Configure(EntityTypeBuilder<ExpedienteImportacion> builder)
    {
        builder.ToTable("ExpedientesImportacion", "cmp");
        builder.HasKey(e => e.ExpedienteImportacionId);
        builder.Property(e => e.ExpedienteImportacionId).UseIdentityColumn();

        builder.Property(e => e.Numero).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Incoterm).HasMaxLength(10);
        builder.Property(e => e.ModalidadTransporte).HasMaxLength(30);
        builder.Property(e => e.PaisOrigen).HasMaxLength(100);
        builder.Property(e => e.PuertoOrigen).HasMaxLength(200);
        builder.Property(e => e.PuertoDestino).HasMaxLength(200);
        builder.Property(e => e.Forwarder).HasMaxLength(200);
        builder.Property(e => e.Aseguradora).HasMaxLength(200);
        builder.Property(e => e.NumeroPólizaSeguro).HasMaxLength(100);
        builder.Property(e => e.NumeroBLAWB).HasMaxLength(100);
        builder.Property(e => e.NumeroDUIDIM).HasMaxLength(100);
        builder.Property(e => e.Despachante).HasMaxLength(200);
        builder.Property(e => e.TotalTributos).HasColumnType("decimal(18,4)");
        builder.Property(e => e.DetalleObservacionAduana).HasMaxLength(2000);
        builder.Property(e => e.Observaciones).HasMaxLength(1000);
        builder.Property(e => e.CreadoPor).HasMaxLength(100);
        builder.Property(e => e.ModificadoPor).HasMaxLength(100);

        builder.Property(e => e.Estado)
            .IsRequired()
            .HasConversion<byte>();

        builder.HasIndex(e => new { e.EmpresaId, e.Numero }).IsUnique();
        builder.HasIndex(e => new { e.EmpresaId, e.Estado });

        builder.HasMany(e => e.OrdenesCompra)
            .WithOne(o => o.ExpedienteImportacion)
            .HasForeignKey(o => o.ExpedienteImportacionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Hitos)
            .WithOne(h => h.ExpedienteImportacion)
            .HasForeignKey(h => h.ExpedienteImportacionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.HojasImportacion)
            .WithOne(h => h.ExpedienteImportacion)
            .HasForeignKey(h => h.ExpedienteImportacionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
