using AgoraHub360.ERP.Domain.Entities.CXC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgoraHub360.ERP.Persistence.Configurations.CXC;

public class ClienteCreditoConfiguracionConfiguration : IEntityTypeConfiguration<ClienteCreditoConfiguracion>
{
    public void Configure(EntityTypeBuilder<ClienteCreditoConfiguracion> builder)
    {
        builder.ToTable("ClienteCreditoConfiguraciones", "cxc");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DiasCredito).IsRequired();
        builder.Property(x => x.LimiteCredito).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Observaciones).HasMaxLength(500);

        builder.HasIndex(x => new { x.EmpresaId, x.ClienteId }).IsUnique();

        builder.HasOne(x => x.Cliente)
            .WithMany()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
