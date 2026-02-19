namespace AgoraHub360.ERP.Shared.DTOs.Inventario;

using System.ComponentModel.DataAnnotations;

public class CreateMovimientoInventarioDto
{
    [Required(ErrorMessage = "El tipo de movimiento es requerido.")]
    public string TipoMovimiento { get; set; } = "Entrada";

    public DateTime FechaMovimiento { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "El producto es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto válido.")]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "El almacén es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un almacén válido.")]
    public int AlmacenId { get; set; }

    /// <summary>Solo requerido para Transferencias.</summary>
    public int? AlmacenDestinoId { get; set; }

    [Required(ErrorMessage = "La cantidad es requerida.")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Costo unitario (requerido para Entradas y Ajustes positivos).
    /// Para Salidas se usa el costo promedio actual.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El costo unitario no puede ser negativo.")]
    public decimal CostoUnitario { get; set; }

    [MaxLength(50)]
    public string? Referencia { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }
}
