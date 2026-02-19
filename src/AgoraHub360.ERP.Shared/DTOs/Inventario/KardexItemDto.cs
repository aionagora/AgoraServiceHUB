namespace AgoraHub360.ERP.Shared.DTOs.Inventario;

/// <summary>
/// Una línea del Kardex de un producto, con saldo acumulado.
/// </summary>
public class KardexItemDto
{
    public int MovimientoId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime FechaMovimiento { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public string? Referencia { get; set; }

    /// <summary>Cantidad que entra (positiva) para este movimiento.</summary>
    public decimal Entrada { get; set; }

    /// <summary>Cantidad que sale (positiva) para este movimiento.</summary>
    public decimal Salida { get; set; }

    public decimal CostoUnitario { get; set; }
    public decimal CostoTotal { get; set; }

    /// <summary>Saldo de unidades acumulado después de este movimiento.</summary>
    public decimal SaldoUnidades { get; set; }

    /// <summary>Costo promedio vigente después de este movimiento.</summary>
    public decimal CostoPromedio { get; set; }

    /// <summary>Valor total del inventario (SaldoUnidades × CostoPromedio).</summary>
    public decimal SaldoValor { get; set; }

    public string? Observaciones { get; set; }
}

public class KardexDto
{
    public int ProductoId { get; set; }
    public string ProductoCodigo { get; set; } = string.Empty;
    public string ProductoNombre { get; set; } = string.Empty;
    public int? AlmacenId { get; set; }
    public string? AlmacenNombre { get; set; }
    public decimal StockActual { get; set; }
    public decimal CostoPromedio { get; set; }
    public List<KardexItemDto> Movimientos { get; set; } = new();
}
