namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>Fabricante/productor del producto. Entidad global (no tenant).</summary>
public class Manufacturer : AuditableEntity
{
    public long ManufacturerId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Pais { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
