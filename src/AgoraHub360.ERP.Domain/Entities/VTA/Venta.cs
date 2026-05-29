using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Enums;

namespace AgoraHub360.ERP.Domain.Entities.VTA;

public class Venta : TenantEntity
{
    public long Id { get; set; }

    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }

    public int? AlmacenId { get; set; }
    public Almacen? Almacen { get; set; }

    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public long? PedidoVentaId { get; set; }
    public PedidoVenta? PedidoVenta { get; set; }

    public string NumeroVenta { get; set; } = string.Empty;
    public DateTime FechaVenta { get; set; }

    public TipoVenta TipoVenta { get; set; } = TipoVenta.Directa;
    public EstadoVenta EstadoVenta { get; set; } = EstadoVenta.Borrador;
    public EstadoPagoVenta EstadoPago { get; set; } = EstadoPagoVenta.Pendiente;

    public string? MonedaId { get; set; }
    public string? MonedaCodigo { get; set; }
    public Moneda? Moneda { get; set; }

    public decimal TipoCambio { get; set; } = 1m;

    public decimal Subtotal { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal ImpuestoTotal { get; set; }
    public decimal Total { get; set; }

    public string? Observaciones { get; set; }

    public bool FacturaGenerada { get; set; } = false;
    public bool InventarioDescontado { get; set; } = false;

    public VentaFacturacionDatos? DatosFacturacion { get; set; }
    public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
    public ICollection<VentaPago> Pagos { get; set; } = new List<VentaPago>();
}
