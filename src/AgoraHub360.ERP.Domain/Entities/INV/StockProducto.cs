namespace AgoraHub360.ERP.Domain.Entities.INV;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;

/// <summary>
/// Saldo de stock actual de un producto en un almacén específico.
/// Se actualiza automáticamente con cada MovimientoInventario.
/// El CostoPromedio se recalcula usando el método de Costo Promedio Ponderado (CPP).
/// </summary>
public class StockProducto : TenantEntity
{
    public int Id { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public int AlmacenId { get; set; }
    public Almacen? Almacen { get; set; }

    /// <summary>Cantidad disponible actual en el almacén.</summary>
    public decimal StockActual { get; set; }

    /// <summary>Costo promedio ponderado vigente del producto en este almacén.</summary>
    public decimal CostoPromedio { get; set; }

    /// <summary>Fecha y hora del último movimiento que actualizó este saldo.</summary>
    public DateTime UltimaActualizacion { get; set; }
}
