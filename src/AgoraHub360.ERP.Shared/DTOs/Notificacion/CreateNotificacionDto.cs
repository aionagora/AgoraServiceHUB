namespace AgoraHub360.ERP.Shared.DTOs.Notificacion;

/// <summary>DTO para enviar una notificación interna del sistema.</summary>
public class CreateNotificacionDto
{
    public int EmpresaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>Tipo funcional de la notificación, ej: "CONTABILIDAD_PERIODO_PROXIMO".</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Prioridad: "Alta", "Media", "Baja".</summary>
    public string Prioridad { get; set; } = "Media";

    /// <summary>ID de la entidad relacionada (como string para generalidad).</summary>
    public string? ReferenciaId { get; set; }

    /// <summary>Tipo de la entidad relacionada, ej: "PeriodoContable".</summary>
    public string? ReferenciaTipo { get; set; }
}
