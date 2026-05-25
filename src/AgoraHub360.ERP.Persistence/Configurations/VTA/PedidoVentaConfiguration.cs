using AgoraHub360.ERP.Domain.Entities.VTA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations.VTA;

public class PedidoVentaConfiguration : IEntityTypeConfiguration<PedidoVenta>
{
    public void Configure(EntityTypeBuilder<PedidoVenta> builder)
    {
        builder.ToTable("PedidosVenta", "vta");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Numero)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Estado)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.ReservaAplicada)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.InventarioDescontado)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.FechaConfirmacion)
            .IsRequired(false);

        builder.Property(x => x.FechaDespacho)
            .IsRequired(false);

        builder.Property(x => x.FechaAnulacion)
            .IsRequired(false);

        builder.Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Property(x => x.Impuestos).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.Property(x => x.Observaciones).HasMaxLength(1000);

        builder.Property(x => x.CreadoPor).HasMaxLength(100);
        builder.Property(x => x.ModificadoPor).HasMaxLength(100);

        // Relaciones obligatorias
        builder.HasOne(x => x.Sucursal)
            .WithMany()
            .HasForeignKey(x => x.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Almacen)
            .WithMany()
            .HasForeignKey(x => x.AlmacenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cliente)
            .WithMany()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Vendedor)
            .WithMany()
            .HasForeignKey(x => x.VendedorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relaciones Opcionales
        builder.HasOne(x => x.ClienteSucursal)
            .WithMany()
            .HasForeignKey(x => x.ClienteSucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UsuarioCliente)
            .WithMany()
            .HasForeignKey(x => x.UsuarioClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tenant Uniqueness
        builder.HasIndex(x => new { x.EmpresaId, x.Numero }).IsUnique();
    }
}
