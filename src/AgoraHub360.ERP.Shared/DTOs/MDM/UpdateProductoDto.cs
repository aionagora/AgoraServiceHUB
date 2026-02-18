namespace AgoraHub360.ERP.Shared.DTOs.MDM;

using System.ComponentModel.DataAnnotations;

public class UpdateProductoDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría.")]
    public int CategoriaProductoId { get; set; }

    [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una unidad de medida.")]
    public int UnidadMedidaId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El precio de compra no puede ser negativo.")]
    public decimal PrecioCompra { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El precio de venta no puede ser negativo.")]
    public decimal PrecioVenta { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    public decimal StockMinimo { get; set; }

    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string? Sku { get; set; }

    [Required(ErrorMessage = "El tipo de producto es obligatorio.")]
    [MaxLength(50)]
    public string TipoProducto { get; set; } = "ProductoTerminado";

    public bool ControlStock { get; set; } = true;

    [Range(0, double.MaxValue, ErrorMessage = "El costo base no puede ser negativo.")]
    public decimal CostoBase { get; set; }

    public bool Activo { get; set; } = true;
}
