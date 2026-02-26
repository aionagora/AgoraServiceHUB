namespace AgoraHub360.ERP.Domain.Entities.PRC;

using AgoraHub360.ERP.Domain.Common;

/// <summary>Lista de precios por empresa, moneda y canal.</summary>
public class PriceList : TenantEntity
{
    public long PriceListId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CurrencyId { get; set; } = string.Empty;
    public int? ChannelId { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsDefault { get; set; }

    public ICollection<PriceListItem> Items { get; set; } = new List<PriceListItem>();
}
