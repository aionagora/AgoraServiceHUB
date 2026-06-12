namespace AgoraHub360.ERP.Shared.DTOs.Ventas;

public class FacturaVentaDto
{
    public long Id { get; set; }
    public long VentaId { get; set; }
    public long? ClientePerfilFiscalId { get; set; }

    public string NumeroFactura { get; set; } = string.Empty;
    public string? NumeroAutorizacion { get; set; }
    public string? BillUuid { get; set; }
    public DateTime FechaEmision { get; set; }

    public string? ActivityCode { get; set; }
    public string TipoDocumentoFactura { get; set; } = string.Empty;
    public string? IdentityDocTypeCode { get; set; }
    public string EstadoFactura { get; set; } = string.Empty;
    public string EstadoSiat { get; set; } = string.Empty;

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

    public string? MonedaCodigo { get; set; }
    public decimal TipoCambio { get; set; }

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

    public bool Activo { get; set; }

    public string? EstadoCobro { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int DiasVencidos { get; set; }

    public List<FacturaVentaDetalleDto> Detalles { get; set; } = new();
}

public class FacturaVentaResumenDto
{
    public long Id { get; set; }
    public long VentaId { get; set; }
    public long? ClientePerfilFiscalId { get; set; }

    public string NumeroFactura { get; set; } = string.Empty;
    public string? BillUuid { get; set; }
    public string? Cuf { get; set; }
    public DateTime FechaEmision { get; set; }

    public string? ActivityCode { get; set; }
    public string? IdentityDocTypeCode { get; set; }
    public string EstadoFactura { get; set; } = string.Empty;
    public string EstadoSiat { get; set; } = string.Empty;

    public string NitFactura { get; set; } = string.Empty;
    public string RazonSocialFactura { get; set; } = string.Empty;
    public string? BeneficiaryName { get; set; }

    public string? MonedaCodigo { get; set; }
    public decimal Total { get; set; }
    public decimal? AdditionalDiscount { get; set; }
    public bool Revertido { get; set; }
    public bool Activo { get; set; }

    public string? EstadoCobro { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int DiasVencidos { get; set; }
}

public class FacturaVentaDetalleDto
{
    public long Id { get; set; }
    public long FacturaVentaId { get; set; }
    public long? VentaDetalleId { get; set; }

    public string TipoItemVenta { get; set; } = string.Empty;

    public string? ItemCode { get; set; }
    public string? CodigoProducto { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? DetalleAdicional { get; set; }

    public decimal Cantidad { get; set; }
    public string? UnidadMedida { get; set; }

    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoMonto { get; set; }
    public decimal ImpuestoMonto { get; set; }
    public decimal TotalLinea { get; set; }
}

public class GenerarFacturaVentaRequestDto
{
    public long VentaId { get; set; }
    public string TipoDocumentoFactura { get; set; } = "Factura";
    public string? Observaciones { get; set; }

    public string? NitFactura { get; set; }
    public string? Complemento { get; set; }
    public string? RazonSocialFactura { get; set; }
    public string? EmailFactura { get; set; }
    public string? TelefonoFactura { get; set; }
}

public class AnularFacturaVentaRequestDto
{
    public string MotivoAnulacion { get; set; } = string.Empty;
}
