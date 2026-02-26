namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.VER;

public interface IVersioningService
{
    Task<Result<IReadOnlyList<EntityVersionDto>>> GetHistoryAsync(
        string entityName, string entityId, CancellationToken ct = default);

    Task<Result<EntityVersionDto>> GetVersionAsync(
        string entityName, string entityId, int versionNo, CancellationToken ct = default);

    Task SaveSnapshotAsync<T>(
        string entityName, string entityId, T entity,
        byte changeType, int? empresaId = null, CancellationToken ct = default);
}
