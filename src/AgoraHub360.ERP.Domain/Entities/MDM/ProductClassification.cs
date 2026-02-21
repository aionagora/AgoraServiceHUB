namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Classification node: Family, SubFamily, Line, Segment, etc.
/// Type: 1=Family, 2=SubFamily, 3=Line, 4=Segment, 5=Other
/// </summary>
public class ProductClassification : AuditableEntity
{
    public long ClassificationId { get; set; }
    public long CatalogId { get; set; }

    /// <summary>1=Family, 2=SubFamily, 3=Line, 4=Segment, 5=Other</summary>
    public byte Type { get; set; }

    public string Name { get; set; } = string.Empty;
    public long? ParentId { get; set; }

    // Navigation
    public ProductClassification? Parent { get; set; }
    public ICollection<ProductClassification> Children { get; set; } = new List<ProductClassification>();
    public ICollection<ProductClassificationLink> Links { get; set; } = new List<ProductClassificationLink>();
}
