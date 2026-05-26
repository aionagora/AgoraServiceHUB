using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Enums;

namespace AgoraHub360.ERP.Domain.Entities.VTA;

public class VentaPago : TenantEntity
{
    public long Id { get; set; }

    public long VentaId { get; set; }
    public Venta? Venta { get; set; }

    public DateTime FechaPago { get; set; }

    public TipoPago TipoPago { get; set; }
    public ModoPago ModoPago { get; set; }

    public long? CuentaCajaBancoId { get; set; }

    public decimal Monto { get; set; }

    public string? MonedaId { get; set; }
    public string? MonedaCodigo { get; set; }

    public decimal TipoCambio { get; set; } = 1m;

    public string? Referencia { get; set; }

    public EstadoPagoVenta EstadoPago { get; set; } = EstadoPagoVenta.Pendiente;
}
