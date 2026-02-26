namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Global unit of measure (UND, BOX, KG, G, L, ML, etc.).
/// Replaces the legacy tenant-scoped UnidadMedida.
/// </summary>
public class Uom : AuditableEntity
{
    public int UomId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<ProductUom> ProductUoms { get; set; } = new List<ProductUom>();
}
