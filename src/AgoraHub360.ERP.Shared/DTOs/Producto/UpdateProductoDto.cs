namespace AgoraHub360.ERP.Shared.DTOs.Producto;

using System.ComponentModel.DataAnnotations;

public class UpdateProductoDto
{
    [Required(ErrorMessage = "El código es requerido.")]
    [MaxLength(50, ErrorMessage = "El código no puede exceder 50 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es requerido.")]
    [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "La categoría es requerida.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría.")]
    public int CategoriaProductoId { get; set; }

    [Required(ErrorMessage = "La unidad de medida es requerida.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una unidad de medida.")]
    public int UnidadMedidaId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El precio de compra debe ser mayor o igual a 0.")]
    public decimal PrecioCompra { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor o igual a 0.")]
    public decimal PrecioVenta { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El stock mínimo debe ser mayor o igual a 0.")]
    public decimal StockMinimo { get; set; }

    [MaxLength(100, ErrorMessage = "El SKU no puede exceder 100 caracteres.")]
    public string? Sku { get; set; }

    [Required(ErrorMessage = "El tipo de producto es requerido.")]
    public string TipoProducto { get; set; } = "ProductoTerminado";

    public bool ControlStock { get; set; } = true;

    [Range(0, double.MaxValue, ErrorMessage = "El costo base debe ser mayor o igual a 0.")]
    public decimal CostoBase { get; set; }

    public bool Activo { get; set; } = true;
}
