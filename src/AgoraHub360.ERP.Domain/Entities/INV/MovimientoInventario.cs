namespace AgoraHub360.ERP.Domain.Entities.INV;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;

/// <summary>
/// Registro de un movimiento de inventario (entrada, salida, ajuste o transferencia).
/// Cada movimiento actualiza el stock y el costo promedio del producto en el almacén.
/// </summary>
public class MovimientoInventario : TenantEntity
{
    public int Id { get; set; }

    /// <summary>Número único del movimiento. Ej: MOV-2026-00001</summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>Entrada | Salida | Ajuste | Transferencia</summary>
    public string TipoMovimiento { get; set; } = string.Empty;

    public DateTime FechaMovimiento { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    /// <summary>Almacén origen (o almacén único para entradas/salidas/ajustes).</summary>
    public int AlmacenId { get; set; }
    public Almacen? Almacen { get; set; }

    /// <summary>Almacén destino (solo aplica para Transferencias).</summary>
    public int? AlmacenDestinoId { get; set; }
    public Almacen? AlmacenDestino { get; set; }

    /// <summary>Cantidad del movimiento (siempre positiva; el signo lo determina el TipoMovimiento).</summary>
    public decimal Cantidad { get; set; }

    /// <summary>Costo unitario del movimiento (costo promedio al momento de la salida, o precio de compra en entrada).</summary>
    public decimal CostoUnitario { get; set; }

    /// <summary>CostoUnitario × Cantidad.</summary>
    public decimal CostoTotal { get; set; }

    /// <summary>Referencia al documento origen (OC-001, FAC-001, etc.).</summary>
    public string? Referencia { get; set; }

    public string? Observaciones { get; set; }
}
