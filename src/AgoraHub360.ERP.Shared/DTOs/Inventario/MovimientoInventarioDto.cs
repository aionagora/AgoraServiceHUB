namespace AgoraHub360.ERP.Shared.DTOs.Inventario;

public class MovimientoInventarioDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string TipoMovimiento { get; set; } = string.Empty;
    public DateTime FechaMovimiento { get; set; }

    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoCodigo { get; set; } = string.Empty;

    public int AlmacenId { get; set; }
    public string AlmacenNombre { get; set; } = string.Empty;

    public int? AlmacenDestinoId { get; set; }
    public string? AlmacenDestinoNombre { get; set; }

    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal CostoTotal { get; set; }
    public string? Referencia { get; set; }
    public string? Observaciones { get; set; }
    public bool Activo { get; set; }
    public int EmpresaId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? CreadoPor { get; set; }
}
