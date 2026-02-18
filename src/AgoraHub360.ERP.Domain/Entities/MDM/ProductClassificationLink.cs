namespace AgoraHub360.ERP.Domain.Entities.MDM;

/// <summary>Relación N:N entre producto y clasificación (familia/línea/segmento).</summary>
public class ProductClassificationLink
{
    public long ProductId { get; set; }
    public long ClassificationId { get; set; }

    // Navegación
    public Product Product { get; set; } = null!;
    public ProductClassification Classification { get; set; } = null!;
}
