namespace AgoraHub360.ERP.Shared.DTOs.VER;

public record EntityVersionDto(
    long VersionId,
    string EntityName,
    string EntityId,
    int VersionNo,
    byte ChangeType,
    string SnapshotJson,
    string? ChangedBy,
    DateTime ChangedAt,
    int? EmpresaId);
