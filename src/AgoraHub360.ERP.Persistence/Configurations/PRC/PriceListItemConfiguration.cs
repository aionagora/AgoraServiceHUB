namespace AgoraHub360.ERP.Persistence.Configurations.PRC;

using AgoraHub360.ERP.Domain.Entities.PRC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PriceListItemConfiguration : IEntityTypeConfiguration<PriceListItem>
{
    public void Configure(EntityTypeBuilder<PriceListItem> builder)
    {
        builder.ToTable("PriceListItems", "prc");
        builder.HasKey(i => i.ItemId);
        builder.Property(i => i.ItemId).UseIdentityColumn();

        builder.Property(i => i.Price).HasPrecision(18, 4);
        builder.Property(i => i.MinQty).HasPrecision(18, 4);
        builder.Property(i => i.DiscountPercent).HasPrecision(9, 4);
        builder.Property(i => i.CreadoPor).HasMaxLength(100);
        builder.Property(i => i.ModificadoPor).HasMaxLength(100);

        builder.HasOne(i => i.PriceList)
            .WithMany(pl => pl.Items)
            .HasForeignKey(i => i.PriceListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => new { i.PriceListId, i.CompanyProductId, i.MinQty, i.ValidFrom });
        builder.HasIndex(i => new { i.PriceListId, i.VariantId, i.MinQty, i.ValidFrom });
    }
}
