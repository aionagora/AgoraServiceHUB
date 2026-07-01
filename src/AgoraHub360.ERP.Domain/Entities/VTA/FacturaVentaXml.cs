using AgoraHub360.ERP.Domain.Common;

namespace AgoraHub360.ERP.Domain.Entities.VTA;

/// <summary>
/// Factura fiscal importada manualmente desde XML (sistema externo).
/// No interfiere con el flujo SIAT ni CxC existentes.
/// Se vincula a una Venta existente.
/// </summary>
public class FacturaVentaXml : TenantEntity
{
    public long Id { get; set; }

    /// <summary>Venta a la que se asocia esta factura importada.</summary>
    public long VentaId { get; set; }
    public Venta? Venta { get; set; }

    // ── Datos del emisor (del XML) ──
    public long NitEmisor { get; set; }
    public string RazonSocialEmisor { get; set; } = string.Empty;
    public string? Municipio { get; set; }
    public string? Telefono { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public int CodigoSucursal { get; set; }
    public int? CodigoPuntoVenta { get; set; }

    // ── Datos del documento fiscal ──
    public int NumeroFactura { get; set; }
    public string Cuf { get; set; } = string.Empty;
    public string? Cufd { get; set; }
    public DateTime FechaEmision { get; set; }
    public int CodigoDocumentoSector { get; set; }

    // ── Datos del cliente ──
    public string NombreRazonSocialCliente { get; set; } = string.Empty;
    public int CodigoTipoDocumentoIdentidad { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string CodigoCliente { get; set; } = string.Empty;

    // ── Montos ──
    public int CodigoMetodoPago { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal MontoTotalSujetoIva { get; set; }
    public int CodigoMoneda { get; set; }
    public decimal TipoCambio { get; set; }
    public decimal MontoTotalMoneda { get; set; }
    public decimal MontoGiftCard { get; set; }
    public decimal DescuentoAdicional { get; set; }
    public int? CodigoExcepcion { get; set; }
    public string? Leyenda { get; set; }
    public string? Usuario { get; set; }

    // ── XML original y hash ──
    public string XmlOriginal { get; set; } = string.Empty;
    public string XmlHashSha256 { get; set; } = string.Empty;

    // ── Estado ──
    public string Estado { get; set; } = "Importada";
    public string? Observacion { get; set; }

    // ── Detalle ──
    public ICollection<FacturaVentaXmlDetalle> Detalles { get; set; } = new List<FacturaVentaXmlDetalle>();
}
