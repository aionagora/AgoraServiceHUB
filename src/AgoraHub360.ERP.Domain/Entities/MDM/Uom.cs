namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Unidad de medida base global (UND, CJ, KG, G, L, ML, etc.).
/// Reemplaza/complementa UnidadMedida existente como catálogo global.
/// </summary>
public class Uom : AuditableEntity
{
    public int UomId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<ProductUom> ProductUoms { get; set; } = new List<ProductUom>();
}
