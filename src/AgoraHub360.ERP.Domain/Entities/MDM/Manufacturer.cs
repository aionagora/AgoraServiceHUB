namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>Product manufacturer — tenant-scoped per company.</summary>
public class Manufacturer : TenantEntity
{
    public long ManufacturerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
