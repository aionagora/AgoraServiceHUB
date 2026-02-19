namespace AgoraHub360.ERP.Shared.DTOs.Inventario;

public class StockProductoDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoCodigo { get; set; } = string.Empty;
    public string ProductoNombre { get; set; } = string.Empty;
    public int AlmacenId { get; set; }
    public string AlmacenNombre { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal CostoPromedio { get; set; }
    public decimal ValorTotal => StockActual * CostoPromedio;
    public DateTime UltimaActualizacion { get; set; }
}
