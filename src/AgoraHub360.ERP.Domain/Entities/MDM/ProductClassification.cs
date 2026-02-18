namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Nodo de clasificación: Familia, SubFamilia, Línea, Segmento, etc.
/// Type: 1=Familia, 2=SubFamilia, 3=Linea, 4=Segmento, 5=Otro
/// </summary>
public class ProductClassification : AuditableEntity
{
    public long ClassificationId { get; set; }
    public long CatalogId { get; set; }

    /// <summary>1=Familia, 2=SubFamilia, 3=Linea, 4=Segmento, 5=Otro</summary>
    public byte Type { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public long? ParentId { get; set; }

    // Navegación
    public ProductClassification? Parent { get; set; }
    public ICollection<ProductClassification> Children { get; set; } = new List<ProductClassification>();
    public ICollection<ProductClassificationLink> Links { get; set; } = new List<ProductClassificationLink>();
}
