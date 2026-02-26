namespace AgoraHub360.ERP.Domain.Entities.MDM;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Extensión por empresa de un producto global.
/// Aquí viven: SKU propio, visibilidad por canal, parámetros de stock, método de costeo.
/// </summary>
public class CompanyProduct : TenantEntity
{
    public long CompanyProductId { get; set; }
    public long ProductId { get; set; }

    public string Sku { get; set; } = string.Empty;
    public string? CodigoInterno { get; set; }

    public int? ImpuestoProfileId { get; set; }
    public string? MonedaBaseId { get; set; }

    public bool IsVisiblePOS { get; set; } = true;
    public bool IsVisibleEcommerce { get; set; }
    public bool IsVisibleB2B { get; set; }
    public bool AllowReturns { get; set; } = true;
    public int? WarrantyDays { get; set; }

    public decimal? MinStock { get; set; }
    public decimal? MaxStock { get; set; }
    public decimal? ReorderPoint { get; set; }

    /// <summary>Override del método de costeo. NULL = usa la regla global de empresa.</summary>
    public byte? CostingMethod { get; set; }

    // Navegación
    public Product Product { get; set; } = null!;

    public ICollection<CompanyProductFeature> Features { get; set; } = new List<CompanyProductFeature>();
}
