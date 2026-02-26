namespace AgoraHub360.ERP.Shared.DTOs.Inventario;

using System.ComponentModel.DataAnnotations;

public class CreateMovimientoInventarioDto
{
    [Required(ErrorMessage = "Movement type is required.")]
    public string MovementType { get; set; } = "Receipt";

    public DateTime MovementDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Company product is required.")]
    [Range(1, long.MaxValue, ErrorMessage = "Select a valid company product.")]
    public long CompanyProductId { get; set; }

    [Required(ErrorMessage = "Warehouse is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Select a valid warehouse.")]
    public int WarehouseId { get; set; }

    /// <summary>Only required for Transfers.</summary>
    public int? DestinationWarehouseId { get; set; }

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit cost (required for Receipts and positive Adjustments).
    /// For Issues the current average cost is used.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Unit cost cannot be negative.")]
    public decimal UnitCost { get; set; }

    [MaxLength(50)]
    public string? Reference { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
