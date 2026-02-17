namespace AgoraHub360.ERP.Persistence.Configurations;

using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs", "core");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Entidad)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(a => a.EntidadId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(a => a.Accion)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.ValoresAnteriores)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.ValoresNuevos)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.CamposModificados)
            .HasMaxLength(1000);

        builder.Property(a => a.Usuario)
            .HasMaxLength(256);

        builder.Property(a => a.FechaHora)
            .IsRequired();

        // Índices para consultas frecuentes
        builder.HasIndex(a => a.Entidad);
        builder.HasIndex(a => a.FechaHora);
        builder.HasIndex(a => a.EmpresaId);
        builder.HasIndex(a => new { a.Entidad, a.EntidadId });
    }
}
