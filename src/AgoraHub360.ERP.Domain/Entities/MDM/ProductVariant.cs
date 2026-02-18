namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// SKU hijo: variante específica de un producto padre (color+talla, sabor+tamaño, etc.).
/// Es tenant-aware porque el SKU es por empresa.
/// </summary>
public class ProductVariant : TenantEntity
{
    public long VariantId { get; set; }
    public long ParentProductId { get; set; }

    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }

    /// <summary>Nombre autogenerado: "Talla M / Color Negro".</summary>
    public string? VariantName { get; set; }

    // Navegación
    public Product ParentProduct { get; set; } = null!;
    public ICollection<VariantAttributeValue> AttributeValues { get; set; } = new List<VariantAttributeValue>();
}
