namespace AgoraHub360.ERP.Persistence.Configurations.PRC;

using AgoraHub360.ERP.Domain.Entities.PRC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PriceListConfiguration : IEntityTypeConfiguration<PriceList>
{
    public void Configure(EntityTypeBuilder<PriceList> builder)
    {
        builder.ToTable("PriceLists", "prc");
        builder.HasKey(pl => pl.PriceListId);
        builder.Property(pl => pl.PriceListId).UseIdentityColumn();

        builder.Property(pl => pl.Code).IsRequired().HasMaxLength(30);
        builder.Property(pl => pl.Name).IsRequired().HasMaxLength(80);
        builder.Property(pl => pl.CurrencyId).IsRequired().HasMaxLength(3);
        builder.Property(pl => pl.CreadoPor).HasMaxLength(100);
        builder.Property(pl => pl.ModificadoPor).HasMaxLength(100);

        // Code único por empresa
        builder.HasIndex(pl => new { pl.EmpresaId, pl.Code }).IsUnique();
        builder.HasIndex(pl => new { pl.EmpresaId, pl.IsDefault });
    }
}
