namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Estados de ciclo de vida del producto.
/// Ejemplos: Borrador, Activo, Bloqueado, Descontinuado.
/// </summary>
public class ProductStatus : AuditableEntity
{
    public int ProductStatusId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
