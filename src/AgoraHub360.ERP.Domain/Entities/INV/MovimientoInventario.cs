namespace AgoraHub360.ERP.Domain.Entities.INV;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;

/// <summary>
/// Record of an inventory movement (receipt, issue, adjustment or transfer).
/// Each movement updates the stock and average cost of the product in the warehouse.
/// </summary>
public class MovimientoInventario : TenantEntity
{
    public int Id { get; set; }

    /// <summary>Unique movement number. E.g.: MOV-2026-00001</summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>Receipt | Issue | Adjustment | Transfer</summary>
    public string MovementType { get; set; } = string.Empty;

    public DateTime MovementDate { get; set; }

    public long CompanyProductId { get; set; }
    public CompanyProduct? CompanyProduct { get; set; }

    /// <summary>Source warehouse (or single warehouse for receipts/issues/adjustments).</summary>
    public int WarehouseId { get; set; }
    public Almacen? Warehouse { get; set; }

    /// <summary>Destination warehouse (only applies to Transfers).</summary>
    public int? DestinationWarehouseId { get; set; }
    public Almacen? DestinationWarehouse { get; set; }

    /// <summary>Movement quantity (always positive; sign is determined by MovementType).</summary>
    public decimal Quantity { get; set; }

    /// <summary>Unit cost of the movement (average cost on issue, or purchase price on receipt).</summary>
    public decimal UnitCost { get; set; }

    /// <summary>UnitCost × Quantity.</summary>
    public decimal TotalCost { get; set; }

    /// <summary>Reference to the source document (PO-001, INV-001, etc.).</summary>
    public string? Reference { get; set; }

    public string? Notes { get; set; }
}
