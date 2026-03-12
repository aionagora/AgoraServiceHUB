namespace AgoraHub360.ERP.Persistence.Configurations.CMP;

using AgoraHub360.ERP.Domain.Entities.CMP;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrdenPedidoConfiguration : IEntityTypeConfiguration<OrdenPedido>
{
    public void Configure(EntityTypeBuilder<OrdenPedido> builder)
    {
        builder.ToTable("OrdenesPedido", "cmp");
        builder.HasKey(o => o.OrdenPedidoId);
        builder.Property(o => o.OrdenPedidoId).UseIdentityColumn();

        builder.Property(o => o.Numero).IsRequired().HasMaxLength(30);
        builder.Property(o => o.FechaEmision).IsRequired();
        builder.Property(o => o.SolicitanteId).IsRequired();
        builder.Property(o => o.CentroCosto).HasMaxLength(100);
        builder.Property(o => o.Observaciones).HasMaxLength(1000);
        builder.Property(o => o.MotivoRechazo).HasMaxLength(500);
        builder.Property(o => o.ObservacionesRevisionStock).HasMaxLength(1000);
        builder.Property(o => o.CreadoPor).HasMaxLength(100);
        builder.Property(o => o.ModificadoPor).HasMaxLength(100);

        builder.Property(o => o.Estado)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(o => o.Urgencia)
            .IsRequired()
            .HasConversion<byte>();

        builder.HasIndex(o => new { o.EmpresaId, o.Numero }).IsUnique();
        builder.HasIndex(o => new { o.EmpresaId, o.Estado });
        builder.HasIndex(o => new { o.EmpresaId, o.FechaEmision });
        builder.HasIndex(o => new { o.EmpresaId, o.SolicitanteId });

        builder.HasOne(o => o.Solicitante)
            .WithMany()
            .HasForeignKey(o => o.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.AlmacenDestino)
            .WithMany()
            .HasForeignKey(o => o.AlmacenDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Lineas)
            .WithOne(l => l.OrdenPedido)
            .HasForeignKey(l => l.OrdenPedidoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
