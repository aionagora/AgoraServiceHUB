namespace AgoraHub360.ERP.Domain.Entities.INV;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;

/// <summary>
/// Current stock balance for a CompanyProduct in a specific warehouse.
/// Updated automatically with each InventoryMovement.
/// The AverageCost is recalculated using Weighted Average Cost (WAC).
/// </summary>
public class StockProducto : TenantEntity
{
    public int Id { get; set; }

    public long CompanyProductId { get; set; }
    public CompanyProduct? CompanyProduct { get; set; }

    public int AlmacenId { get; set; }
    public Almacen? Almacen { get; set; }

    /// <summary>Current available quantity in the warehouse.</summary>
    public decimal CurrentStock { get; set; }

    /// <summary>Quantity logically reserved for confirmed sales orders.</summary>
    public decimal ReservedStock { get; set; } = 0m;

    /// <summary>Weighted average cost (WAC) currently in effect.</summary>
    public decimal AverageCost { get; set; }

    /// <summary>Date and time of the last movement that updated this balance.</summary>
    public DateTime LastUpdated { get; set; }
}
