namespace AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Entidad base con campos de auditoría automática.
/// Todas las entidades del sistema heredan de esta clase.
/// </summary>
public abstract class AuditableEntity
{
    public DateTime FechaCreacion { get; set; }
    public string? CreadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }
    public bool Activo { get; set; } = true;
}
