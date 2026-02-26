namespace AgoraHub360.ERP.Application.Services;

using System.Text.Json;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.VER;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.VER;

public class VersioningService : IVersioningService
{
    private readonly IRepository<EntityVersion> _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public VersioningService(
        IRepository<EntityVersion> repo,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repo = repo;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<EntityVersionDto>>> GetHistoryAsync(
        string entityName, string entityId, CancellationToken ct = default)
    {
        var versions = await _repo.FindAsync(
            v => v.EntityName == entityName && v.EntityId == entityId, ct);
        return Result<IReadOnlyList<EntityVersionDto>>.Success(
            versions.OrderBy(v => v.VersionNo).Select(Map).ToList().AsReadOnly());
    }

    public async Task<Result<EntityVersionDto>> GetVersionAsync(
        string entityName, string entityId, int versionNo, CancellationToken ct = default)
    {
        var version = (await _repo.FindAsync(
            v => v.EntityName == entityName && v.EntityId == entityId && v.VersionNo == versionNo, ct))
            .FirstOrDefault();
        if (version is null)
            return Result<EntityVersionDto>.Failure($"Versión {versionNo} no encontrada para {entityName}/{entityId}.");
        return Result<EntityVersionDto>.Success(Map(version));
    }

    public async Task SaveSnapshotAsync<T>(
        string entityName, string entityId, T entity,
        byte changeType, int? empresaId = null, CancellationToken ct = default)
    {
        var existing = await _repo.FindAsync(
            v => v.EntityName == entityName && v.EntityId == entityId, ct);
        var nextVersion = existing.Any() ? existing.Max(v => v.VersionNo) + 1 : 1;

        var snapshot = new EntityVersion
        {
            EntityName = entityName,
            EntityId = entityId,
            VersionNo = nextVersion,
            ChangeType = changeType,
            SnapshotJson = JsonSerializer.Serialize(entity),
            ChangedBy = _currentUser.UserName,
            ChangedAt = DateTime.UtcNow,
            EmpresaId = empresaId ?? _currentUser.EmpresaId
        };

        await _repo.AddAsync(snapshot, ct);
        await _uow.SaveChangesAsync(ct);
    }

    private static EntityVersionDto Map(EntityVersion v) => new(
        v.VersionId, v.EntityName, v.EntityId, v.VersionNo,
        v.ChangeType, v.SnapshotJson, v.ChangedBy, v.ChangedAt, v.EmpresaId);
}
