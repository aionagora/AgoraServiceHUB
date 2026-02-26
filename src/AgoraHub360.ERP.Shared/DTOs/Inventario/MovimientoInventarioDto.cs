namespace AgoraHub360.ERP.Shared.DTOs.Inventario;

public class MovimientoInventarioDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public DateTime MovementDate { get; set; }

    public long CompanyProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;

    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;

    public int? DestinationWarehouseId { get; set; }
    public string? DestinationWarehouseName { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public bool Activo { get; set; }
    public int EmpresaId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? CreadoPor { get; set; }
}
