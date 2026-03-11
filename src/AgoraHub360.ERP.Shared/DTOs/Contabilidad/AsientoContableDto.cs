namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad;

/// <summary>DTO de lectura para Comprobante Contable.</summary>
public class AsientoContableDto
{
    public long AsientoContableId { get; set; }
    public int EmpresaId { get; set; }

    // Tipo comprobante
    public int? TipoComprobanteId { get; set; }
    public string TipoComprobanteCodigo { get; set; } = string.Empty;
    public string TipoComprobanteNombre { get; set; } = string.Empty;

    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int Gestion { get; set; }
    public string TipoRegistro { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Concepto { get; set; }
    public string Glosa { get; set; } = string.Empty;

    // Tipo de cambio
    public int? TipoCambioId { get; set; }
    public string? TipoCambioMoneda { get; set; }
    public decimal? ValorTipoCambio { get; set; }

    // Tipo de pago / Documento
    public int? TipoPagoId { get; set; }
    public string? TipoPagoCodigo { get; set; }
    public string? TipoPagoNombre { get; set; }
    public string? NumeroDocumentoPago { get; set; }

    // Usuario
    public int? RegistradoPorId { get; set; }
    public string? RegistradoPorNombre { get; set; }

    // Origen
    public string? OrigenTipo { get; set; }
    public long? OrigenId { get; set; }
    public string? OrigenReferencia { get; set; }

    // Totales
    public decimal TotalDebe { get; set; }
    public decimal TotalHaber { get; set; }
    public bool Cuadrado { get; set; }

    public List<AsientoContableLineaDto> Lineas { get; set; } = new();
}

/// <summary>DTO de lectura para línea de comprobante.</summary>
public class AsientoContableLineaDto
{
    public long AsientoContableLineaId { get; set; }
    public int NumeroLinea { get; set; }
    public int CuentaContableId { get; set; }
    public string CuentaCodigo { get; set; } = string.Empty;
    public string CuentaNombre { get; set; } = string.Empty;
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string? Glosa { get; set; }
    public string? Referencia { get; set; }

    // Centro de costo analítico (nullable — compatible con datos existentes)
    public int? CentroCostoId { get; set; }
    public string? CentroCostoCodigo { get; set; }
    public string? CentroCostoNombre { get; set; }
}

/// <summary>DTO para crear un comprobante contable.</summary>
public class CreateAsientoContableDto
{
    public int TipoComprobanteId { get; set; }
    public DateTime Fecha { get; set; }
    public string? Concepto { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public int? TipoCambioId { get; set; }
    public int? TipoPagoId { get; set; }
    public string? NumeroDocumentoPago { get; set; }
    public string? OrigenTipo { get; set; }
    public long? OrigenId { get; set; }
    public string? OrigenReferencia { get; set; }
    public List<CreateAsientoLineaDto> Lineas { get; set; } = new();
}

/// <summary>DTO para actualizar un comprobante en borrador.</summary>
public class UpdateAsientoContableDto
{
    public int TipoComprobanteId { get; set; }
    public DateTime Fecha { get; set; }
    public string? Concepto { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public int? TipoCambioId { get; set; }
    public int? TipoPagoId { get; set; }
    public string? NumeroDocumentoPago { get; set; }
    public List<CreateAsientoLineaDto> Lineas { get; set; } = new();
}

/// <summary>DTO para crear una línea de comprobante.</summary>
public class CreateAsientoLineaDto
{
    public int CuentaContableId { get; set; }
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
    public string? Glosa { get; set; }
    public string? Referencia { get; set; }

    /// <summary>Centro de costo analítico opcional. Null = sin centro de costo.</summary>
    public int? CentroCostoId { get; set; }
}

// ?? DTOs para tablas de soporte ??

public class TipoComprobanteDto
{
    public int TipoComprobanteId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Prefijo { get; set; } = string.Empty;
    public int Orden { get; set; }
}

public class TipoCambioDto
{
    public int TipoCambioId { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Simbolo { get; set; } = string.Empty;
    public decimal TasaCompra { get; set; }
    public decimal TasaVenta { get; set; }
    public DateTime FechaVigencia { get; set; }
}

public class TipoPagoDto
{
    public int TipoPagoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool RequiereReferencia { get; set; }
    public int Orden { get; set; }
}
