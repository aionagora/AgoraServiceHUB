namespace AgoraHub360.ERP.Domain.Entities.ACC;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Tipo de pago / Documento (Cheque, Efectivo, QR, Transferencia, S/D, etc.).
/// </summary>
public class TipoPago : TenantEntity
{
    public int TipoPagoId { get; set; }

    /// <summary>Código corto (ej: "CHQ", "EFE", "QR", "TRF", "S/D").</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Nombre del tipo de pago (ej: "Cheque", "Efectivo").</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Indica si requiere número de referencia (ej: nro cheque).</summary>
    public bool RequiereReferencia { get; set; }

    /// <summary>Orden de visualización.</summary>
    public int Orden { get; set; }
}
