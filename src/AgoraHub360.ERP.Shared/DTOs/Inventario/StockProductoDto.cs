namespace AgoraHub360.ERP.Shared.DTOs.Inventario;

public class StockProductoDto
{
    public int Id { get; set; }
    public long CompanyProductId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal AverageCost { get; set; }
    public decimal TotalValue => CurrentStock * AverageCost;
    public DateTime LastUpdated { get; set; }
}
