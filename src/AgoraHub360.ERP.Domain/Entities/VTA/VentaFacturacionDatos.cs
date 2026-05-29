using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Enums;

namespace AgoraHub360.ERP.Domain.Entities.VTA;

public class VentaFacturacionDatos : TenantEntity
{
    public long Id { get; set; }

    public long VentaId { get; set; }
    public Venta? Venta { get; set; }

    public bool Facturar { get; set; } = false;
    public bool FacturarAlMismoCliente { get; set; } = true;

    public string? TipoDocumentoIdentidad { get; set; }
    public string? NitFactura { get; set; }
    public string? Complemento { get; set; }
    public string? RazonSocialFactura { get; set; }
    public string? EmailFactura { get; set; }
    public string? TelefonoFactura { get; set; }

    public EstadoFacturaVenta EstadoFactura { get; set; } = EstadoFacturaVenta.NoGenerada;

    public long? FacturaId { get; set; }
}
