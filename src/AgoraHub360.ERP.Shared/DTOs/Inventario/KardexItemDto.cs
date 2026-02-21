namespace AgoraHub360.ERP.Shared.DTOs.Inventario;

/// <summary>
/// A single Kardex line for a product, with running balance.
/// </summary>
public class KardexItemDto
{
    public int MovementId { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime MovementDate { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public string? Reference { get; set; }

    public decimal In { get; set; }
    public decimal Out { get; set; }

    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    public decimal BalanceUnits { get; set; }
    public decimal AverageCost { get; set; }
    public decimal BalanceValue { get; set; }

    public string? Notes { get; set; }
}

public class KardexDto
{
    public long CompanyProductId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal AverageCost { get; set; }
    public List<KardexItemDto> Movements { get; set; } = new();
}
