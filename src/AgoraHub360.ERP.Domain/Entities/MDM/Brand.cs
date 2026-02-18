namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>Marca comercial del producto. Entidad global (no tenant).</summary>
public class Brand : AuditableEntity
{
    public long BrandId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
