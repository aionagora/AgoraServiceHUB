namespace AgoraHub360.ERP.Domain.Entities.PRC;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Ítem de lista de precios: precio por CompanyProduct o Variante,
/// con soporte de precio por volumen (MinQty) y vigencia.
/// </summary>
public class PriceListItem : AuditableEntity
{
    public long ItemId { get; set; }
    public long PriceListId { get; set; }

    /// <summary>Referencia al CompanyProduct. NULL si el precio es por variante.</summary>
    public long? CompanyProductId { get; set; }

    /// <summary>Referencia al ProductVariant. NULL si el precio es por CompanyProduct.</summary>
    public long? VariantId { get; set; }

    public decimal Price { get; set; }
    public decimal? MinQty { get; set; }
    public decimal? DiscountPercent { get; set; }

    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }

    // Navegación
    public PriceList PriceList { get; set; } = null!;
}
