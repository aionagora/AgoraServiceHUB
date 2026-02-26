namespace AgoraHub360.ERP.Domain.Entities.CMP;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Gasto individual de importación (flete, seguro, arancel, agencia, etc.).
/// </summary>
public class GastoImportacion : AuditableEntity
{
    public long GastoImportacionId { get; set; }
    public long HojaImportacionId { get; set; }

    /// <summary>Tipo de gasto (ej: Flete, Seguro, Arancel, Agencia, Almacenaje, Otros).</summary>
    public string TipoGasto { get; set; } = string.Empty;

    /// <summary>Descripción detallada.</summary>
    public string? Descripcion { get; set; }

    /// <summary>Monto del gasto.</summary>
    public decimal Monto { get; set; }

    /// <summary>Moneda del gasto (puede diferir de la OC).</summary>
    public string MonedaId { get; set; } = "BOB";

    /// <summary>Tasa de cambio a moneda base de la empresa.</summary>
    public decimal TasaCambio { get; set; } = 1;

    /// <summary>Monto convertido a moneda base: Monto * TasaCambio.</summary>
    public decimal MontoBase { get; set; }

    /// <summary>Referencia de factura/documento del gasto.</summary>
    public string? Referencia { get; set; }

    // Navegación
    public HojaImportacion? HojaImportacion { get; set; }
}
