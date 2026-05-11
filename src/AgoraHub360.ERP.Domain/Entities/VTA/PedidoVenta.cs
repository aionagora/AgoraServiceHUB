using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.MDM;

namespace AgoraHub360.ERP.Domain.Entities.VTA;

/// <summary>
/// Representa un Pedido de Venta formal (Sales Order) realizado por un cliente a la empresa.
/// </summary>
public class PedidoVenta : TenantEntity
{
    public long Id { get; set; }

    /// <summary>
    /// Número único auto-generado. (e.g., PV-2024-001)
    /// </summary>
    public string Numero { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; }
    public DateTime? FechaEntregaEsperada { get; set; }

    // --- Relaciones de Origen (Quién Vende) ---

    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }

    public int AlmacenId { get; set; }
    public Almacen? Almacen { get; set; }

    public int VendedorId { get; set; }
    public Usuario? Vendedor { get; set; }

    // --- Relaciones de Destino (Quién Compra) ---

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int? ClienteSucursalId { get; set; }
    public ClienteSucursal? ClienteSucursal { get; set; }

    public int? UsuarioClienteId { get; set; }
    public Usuario? UsuarioCliente { get; set; }

    // --- Estado y Montos ---

    /// <summary>
    /// Borrador, Confirmado, EnPreparacion, Despachado, Entregado, Anulado
    /// </summary>
    public string Estado { get; set; } = "Borrador";

    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }

    public string? Observaciones { get; set; }

    // Navegación
    public ICollection<PedidoVentaDetalle> Detalles { get; set; } = new List<PedidoVentaDetalle>();
}
