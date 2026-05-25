namespace AgoraHub360.ERP.Domain.Entities.CMP;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Hito de tracking dentro de un Expediente de Importación.
/// Registra eventos clave: ETD, ATD, ETA, ATA, Aduana, Observación, Levante, etc.
/// </summary>
public class HitoExpediente : AuditableEntity
{
    public long HitoExpedienteId { get; set; }
    public long ExpedienteImportacionId { get; set; }

    /// <summary>Tipo de hito (ETD, ATD, ETA, ATA, Aduana, ObservacionAduana, Levante, Otro).</summary>
    public string TipoHito { get; set; } = string.Empty;

    public DateTime FechaHito { get; set; }

    public string? Descripcion { get; set; }

    /// <summary>Archivo adjunto o referencia documental del hito.</summary>
    public string? ReferenciaDocumento { get; set; }

    // Navegación
    public ExpedienteImportacion? ExpedienteImportacion { get; set; }
}
