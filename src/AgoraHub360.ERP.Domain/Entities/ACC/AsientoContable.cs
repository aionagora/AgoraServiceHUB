namespace AgoraHub360.ERP.Domain.Entities.ACC;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.DOC;

/// <summary>
/// Comprobante contable (journal entry).
/// Cabecera del comprobante con líneas Debe/Haber que deben cuadrar en partida doble.
/// </summary>
public class AsientoContable : TenantEntity
{
    public long AsientoContableId { get; set; }

    // ?? Tipo de comprobante ??
    /// <summary>FK al tipo de comprobante (Ingreso, Egreso, Traspaso). Null para comprobantes migrados.</summary>
    public int? TipoComprobanteId { get; set; }
    public TipoComprobante? TipoComprobante { get; set; }

    /// <summary>Número de comprobante (se reinicia por período y tipo). Ej: CI-001, CE-001.</summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>Fecha contable del comprobante.</summary>
    public DateTime Fecha { get; set; }

    /// <summary>Gestión / Año fiscal.</summary>
    public int Gestion { get; set; }

    // ?? Tipo de registro ??
    /// <summary>Tipo de registro: Manual, Automático, Ajuste.</summary>
    public string TipoRegistro { get; set; } = "Manual";

    // ?? Estado ??
    /// <summary>Estado: Borrador, Contabilizado, Anulado.</summary>
    public string Estado { get; set; } = "Borrador";

    // ?? Concepto y Glosa ??
    /// <summary>Concepto / Etiqueta: "Por concepto de...".</summary>
    public string? Concepto { get; set; }

    /// <summary>Glosa general del comprobante.</summary>
    public string Glosa { get; set; } = string.Empty;

    // ?? Tipo de cambio ??
    /// <summary>FK al tipo de cambio vigente (Dólar, UFV).</summary>
    public int? TipoCambioId { get; set; }
    public TipoCambio? TipoCambio { get; set; }

    /// <summary>Valor del tipo de cambio al momento del registro.</summary>
    public decimal? ValorTipoCambio { get; set; }

    // ?? Tipo de pago / Documento ??
    /// <summary>FK al tipo de pago (Cheque, Efectivo, QR, S/D).</summary>
    public int? TipoPagoId { get; set; }
    public TipoPago? TipoPago { get; set; }

    /// <summary>Número de documento de pago (nro cheque, referencia QR, etc.).</summary>
    public string? NumeroDocumentoPago { get; set; }

    // ?? Usuario que registra ??
    /// <summary>FK al usuario que registro el comprobante.</summary>
    public int? RegistradoPorId { get; set; }
    public Usuario? RegistradoPor { get; set; }

    /// <summary>Nombre capturado al registrar — firma historica inmutable para impresiones.</summary>
    public string? RegistradoPorNombre { get; set; }

    // ?? Origen (para asientos automáticos) ??
    /// <summary>Tipo de documento origen (ej: Recepción, Importación, Venta, Ajuste).</summary>
    public string? OrigenTipo { get; set; }

    /// <summary>Id del documento origen (ej: RecepcionCompraId, HojaImportacionId).</summary>
    public long? OrigenId { get; set; }

    /// <summary>Referencia al documento origen (ej: REC-000001).</summary>
    public string? OrigenReferencia { get; set; }

    // ?? Totales ??
    /// <summary>Total Debe (calculado).</summary>
    public decimal TotalDebe { get; set; }

    /// <summary>Total Haber (calculado).</summary>
    public decimal TotalHaber { get; set; }

    // Navegación
    public ICollection<AsientoContableLinea> Lineas { get; set; } = new List<AsientoContableLinea>();
    public ICollection<ComprobanteDocumento> Documentos { get; set; } = new List<ComprobanteDocumento>();
}
