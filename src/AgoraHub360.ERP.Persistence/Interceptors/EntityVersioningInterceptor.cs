namespace AgoraHub360.ERP.Persistence.Interceptors;

using System.Text.Json;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Entities.PRC;
using AgoraHub360.ERP.Domain.Entities.VER;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
/// Interceptor que genera snapshots JSON en la tabla ver.EntityVersions
/// cada vez que cambian entidades clave de MDM: Product, CompanyProduct,
/// AttributeDefinition y PriceList.
/// Sigue el mismo patrón que AuditableEntityInterceptor.
/// </summary>
public class EntityVersioningInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private List<VersionCapture> _pending = new();

    // Tipos a versionar (solo las entidades de negocio críticas)
    private static readonly HashSet<Type> TrackedTypes = new()
    {
        typeof(Product),
        typeof(CompanyProduct),
        typeof(AttributeDefinition),
        typeof(PriceList)
    };

    // ChangeType bytes: 1=Crear, 2=Actualizar, 3=Eliminar
    private const byte Created = 1;
    private const byte Updated = 2;
    private const byte Deleted = 3;

    public EntityVersioningInterceptor(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    // ── Sync ──────────────────────────────────────────────────────────────────

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        _pending = Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        WriteVersions(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    // ── Async ─────────────────────────────────────────────────────────────────

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        _pending = Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        await WriteVersionsAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    // ── Captura antes del Save ────────────────────────────────────────────────

    private List<VersionCapture> Capture(DbContext? context)
    {
        if (context is null) return new();

        var captures = new List<VersionCapture>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged) continue;
            if (!TrackedTypes.Contains(entry.Entity.GetType())) continue;

            captures.Add(new VersionCapture
            {
                Entry = entry,
                EntityType = entry.Entity.GetType().Name,
                ChangeType = entry.State switch
                {
                    EntityState.Added => Created,
                    EntityState.Modified => Updated,
                    EntityState.Deleted => Deleted,
                    _ => Updated
                },
                // Snapshot del estado actual (para Added/Modified) o anterior (para Deleted)
                EntitySnapshot = entry.State == EntityState.Deleted
                    ? SerializeOriginal(entry)
                    : SerializeCurrent(entry),
                ChangedBy = _currentUser.UserName,
                ChangedAt = DateTime.UtcNow,
                EmpresaId = _currentUser.EmpresaId
            });
        }

        return captures;
    }

    // ── Escritura después del Save (PKs ya resueltas) ─────────────────────────

    private void WriteVersions(DbContext? context)
    {
        if (context is null || _pending.Count == 0) return;

        var versions = BuildVersions(context);
        if (versions.Count == 0) return;

        context.Set<EntityVersion>().AddRange(versions);
        context.SaveChanges();
        _pending.Clear();
    }

    private async Task WriteVersionsAsync(DbContext? context, CancellationToken ct)
    {
        if (context is null || _pending.Count == 0) return;

        var versions = BuildVersions(context);
        if (versions.Count == 0) return;

        context.Set<EntityVersion>().AddRange(versions);
        await context.SaveChangesAsync(ct);
        _pending.Clear();
    }

    private List<EntityVersion> BuildVersions(DbContext context)
    {
        var result = new List<EntityVersion>();

        foreach (var capture in _pending)
        {
            var entityId = ResolveEntityId(capture.Entry);
            if (string.IsNullOrEmpty(entityId)) continue;

            // Calcular el siguiente número de versión
            var existingCount = context.Set<EntityVersion>()
                .Where(v => v.EntityName == capture.EntityType && v.EntityId == entityId)
                .Count();

            result.Add(new EntityVersion
            {
                EntityName = capture.EntityType,
                EntityId = entityId,
                VersionNo = existingCount + 1,
                ChangeType = capture.ChangeType,
                SnapshotJson = capture.EntitySnapshot,
                ChangedBy = capture.ChangedBy,
                ChangedAt = capture.ChangedAt,
                EmpresaId = capture.EmpresaId
            });
        }

        return result;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string ResolveEntityId(EntityEntry entry)
    {
        var keyValues = entry.Properties
            .Where(p => p.Metadata.IsPrimaryKey())
            .Select(p => p.CurrentValue?.ToString() ?? "")
            .ToList();

        return keyValues.Count == 1
            ? keyValues[0]
            : string.Join("|", keyValues);
    }

    private static string SerializeOriginal(EntityEntry entry)
    {
        var dict = entry.Properties
            .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
        return JsonSerializer.Serialize(dict);
    }

    private static string SerializeCurrent(EntityEntry entry)
    {
        var dict = entry.Properties
            .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
        return JsonSerializer.Serialize(dict);
    }

    // ── Clase auxiliar ────────────────────────────────────────────────────────

    private class VersionCapture
    {
        public EntityEntry Entry { get; set; } = null!;
        public string EntityType { get; set; } = "";
        public byte ChangeType { get; set; }
        public string EntitySnapshot { get; set; } = "";
        public string? ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public int? EmpresaId { get; set; }
    }
}
