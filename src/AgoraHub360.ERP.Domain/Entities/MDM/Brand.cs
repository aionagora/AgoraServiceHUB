namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>Commercial brand of a product. Global entity (not tenant-scoped).</summary>
public class Brand : AuditableEntity
{
    public long BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
