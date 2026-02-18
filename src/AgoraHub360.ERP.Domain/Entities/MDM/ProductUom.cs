namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Unidades de medida alternativas del producto con factor de conversión a base.
/// Ejemplo: Caja (CJ) → 12 UND, FactorToBase=12.
/// </summary>
public class ProductUom : AuditableEntity
{
    public long ProductUomId { get; set; }
    public long ProductId { get; set; }
    public int UomId { get; set; }

    public bool IsBase { get; set; }

    /// <summary>Factor de conversión a UoM base. Ej: 1 CJ = 12 UND → FactorToBase=12.</summary>
    public decimal FactorToBase { get; set; } = 1;

    public string? Barcode { get; set; }

    // Navegación
    public Product Product { get; set; } = null!;
    public Uom Uom { get; set; } = null!;
}
