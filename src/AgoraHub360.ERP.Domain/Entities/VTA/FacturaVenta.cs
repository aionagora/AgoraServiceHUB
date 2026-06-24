using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Enums;

namespace AgoraHub360.ERP.Domain.Entities.VTA;

public class FacturaVenta : TenantEntity
{
    public long Id { get; set; }

    public long VentaId { get; set; }
    public Venta? Venta { get; set; }

    public string NumeroFactura { get; set; } = string.Empty;
    public string? NumeroAutorizacion { get; set; }
    public string? BillUuid { get; set; }
    public DateTime FechaEmision { get; set; }

    public string? ActivityCode { get; set; }
    public TipoDocumentoFactura TipoDocumentoFactura { get; set; } = TipoDocumentoFactura.Factura;
    public string? IdentityDocTypeCode { get; set; }
    public EstadoFacturaVentaComercial EstadoFactura { get; set; } = EstadoFacturaVentaComercial.Borrador;
    public EstadoSiatFactura EstadoSiat { get; set; } = EstadoSiatFactura.NoEnviada;

    public string NitFactura { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string RazonSocialFactura { get; set; } = string.Empty;
    public string? BeneficiaryName { get; set; }
    public string? EmailFactura { get; set; }
    public string? TelefonoFactura { get; set; }
    public string? PaymentMethodCode { get; set; }
    public string? CardNumber { get; set; }
    public decimal? GiftCardAmount { get; set; }
    public decimal? AdditionalDiscount { get; set; }
    public string? PieLey { get; set; }
    public string? EnlaceXml { get; set; }
    public string? EnlacePdf { get; set; }
    public string? SiatQr { get; set; }
    public string? Origen { get; set; }
    public string? DescripcionFC { get; set; }
    public int? IdDosificacion { get; set; }
    public int? CodDePago { get; set; }
    public int? IdTipo { get; set; }
    public bool Revertido { get; set; }

    public long? ClientePerfilFiscalId { get; set; }
    public ClientePerfilFiscal? ClientePerfilFiscal { get; set; }

    public string? MonedaCodigo { get; set; }
    public decimal TipoCambio { get; set; } = 1m;

    public decimal Subtotal { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal ImpuestoTotal { get; set; }
    public decimal Total { get; set; }

    public string? Observaciones { get; set; }
    public string? MotivoAnulacion { get; set; }
    public DateTime? FechaAnulacion { get; set; }

    public string? Cuf { get; set; }
    public string? Cufd { get; set; }
    public string? Cuis { get; set; }
    public string? CodigoControl { get; set; }
    public string? CodigoRecepcion { get; set; }
    public string? CodigoExcepcion { get; set; }
    public string? Leyenda { get; set; }

    public ICollection<FacturaVentaDetalle> Detalles { get; set; } = new List<FacturaVentaDetalle>();
}
