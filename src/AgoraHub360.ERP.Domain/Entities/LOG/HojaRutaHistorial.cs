namespace AgoraHub360.ERP.Domain.Entities.LOG;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Historial de cambios de estado de una Hoja de Ruta.
/// Cada transición queda registrada como auditoría.
/// </summary>
public class HojaRutaHistorial : AuditableEntity
{
    public long HojaRutaHistorialId { get; set; }
    public long HojaRutaId { get; set; }

    public string EstadoAnterior { get; set; } = string.Empty;
    public string SubEstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
    public string SubEstadoNuevo { get; set; } = string.Empty;

    public DateTime FechaCambio { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string? Observaciones { get; set; }

    // Navegación
    public HojaRuta? HojaRuta { get; set; }
}
