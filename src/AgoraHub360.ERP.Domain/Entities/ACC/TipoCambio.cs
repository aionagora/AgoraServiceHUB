namespace AgoraHub360.ERP.Domain.Entities.ACC;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Tipo de cambio diario (Dólar, UFV, Euro, etc.).
/// </summary>
public class TipoCambio : TenantEntity
{
    public int TipoCambioId { get; set; }

    /// <summary>Moneda (ej: "USD", "UFV", "EUR").</summary>
    public string Moneda { get; set; } = string.Empty;

    /// <summary>Nombre descriptivo (ej: "Dólar Americano").</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Símbolo (ej: "$", "Bs").</summary>
    public string Simbolo { get; set; } = string.Empty;

    /// <summary>Tipo de cambio de compra.</summary>
    public decimal TasaCompra { get; set; }

    /// <summary>Tipo de cambio de venta.</summary>
    public decimal TasaVenta { get; set; }

    /// <summary>Fecha de vigencia del tipo de cambio.</summary>
    public DateTime FechaVigencia { get; set; }
}
