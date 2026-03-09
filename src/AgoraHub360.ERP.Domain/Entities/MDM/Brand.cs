namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>Commercial brand — tenant-scoped per company.</summary>
public class Brand : TenantEntity
{
    public long BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
