namespace AgoraHub360.ERP.Domain.Entities.VER;

/// <summary>
/// Snapshot versionado de cualquier entidad del sistema.
/// Permite recuperar el estado completo en cualquier versión anterior.
/// ChangeType: 1=Create, 2=Update, 3=Delete
/// </summary>
public class EntityVersion
{
    public long VersionId { get; set; }
    public string EntityName { get; set; } = string.Empty;

    /// <summary>PK de la entidad serializada como string (o JSON si PK compuesta).</summary>
    public string EntityId { get; set; } = string.Empty;

    public int VersionNo { get; set; }

    /// <summary>1=Create, 2=Update, 3=Delete</summary>
    public byte ChangeType { get; set; }

    /// <summary>Estado completo serializado de la entidad.</summary>
    public string SnapshotJson { get; set; } = "{}";

    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public int? EmpresaId { get; set; }
}
