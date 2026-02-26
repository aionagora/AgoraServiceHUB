namespace AgoraHub360.ERP.Domain.Entities.MDM;

/// <summary>Relación N:N entre producto y categoría.</summary>
public class ProductCategory
{
    public long ProductId { get; set; }
    public long CategoryId { get; set; }

    /// <summary>Si true, es la categoría principal para navegación y reportes.</summary>
    public bool IsPrimary { get; set; }

    // Navegación
    public Product Product { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
