namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public class ProductoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CategoriaProductoId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public int UnidadMedidaId { get; set; }
    public string UnidadMedidaNombre { get; set; } = string.Empty;
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal StockMinimo { get; set; }
    public string? Sku { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
