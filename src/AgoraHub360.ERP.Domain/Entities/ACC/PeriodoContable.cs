namespace AgoraHub360.ERP.Domain.Entities.ACC;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;

/// <summary>
/// Período contable (mensual).
/// Controla en qué meses se pueden registrar asientos y cuáles están cerrados.
/// </summary>
public class PeriodoContable : TenantEntity
{
    public int PeriodoContableId { get; set; }

    /// <summary>Año fiscal (ej: 2025).</summary>
    public int Anio { get; set; }

    /// <summary>Mes (1-12).</summary>
    public int Mes { get; set; }

    /// <summary>Nombre del período (ej: "Enero 2025").</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Estado: Abierto, Cerrado.</summary>
    public string Estado { get; set; } = "Abierto";

    /// <summary>Fecha en que se cerró el período (null si abierto).</summary>
    public DateTime? FechaCierre { get; set; }

    /// <summary>FK al usuario que cerro el periodo.</summary>
    public int? CerradoPorId { get; set; }
    public Usuario? CerradoPor { get; set; }

    /// <summary>Nombre capturado al cierre — firma historica inmutable para reportes.</summary>
    public string? CerradoPorNombre { get; set; }
}
