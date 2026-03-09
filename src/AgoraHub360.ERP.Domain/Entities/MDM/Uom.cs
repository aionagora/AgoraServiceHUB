namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Unit of measure — tenant-scoped per company.
/// Each company can define its own UOM codes and conversions.
/// </summary>
public class Uom : TenantEntity
{
    public int UomId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<ProductUom> ProductUoms { get; set; } = new List<ProductUom>();
}
