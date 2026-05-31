namespace AgoraHub360.ERP.Persistence.Interceptors;

using System.Text.Json;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
/// Interceptor que establece automáticamente los campos de auditoría
/// (FechaCreacion, CreadoPor, FechaModificacion, ModificadoPor) en cada SaveChanges
/// y genera registros en la tabla AuditLog.
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    // Almacén temporal de entradas pendientes (Added cuyo PK no se conoce hasta después del save)
    private List<AuditEntry> _pendingAuditEntries = new();

    public AuditableEntityInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        _pendingAuditEntries = CaptureAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        WritePendingAuditLogs(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        _pendingAuditEntries = CaptureAuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        await WritePendingAuditLogsAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    // ──── Campos de auditoría automáticos ────

    private void ApplyAudit(DbContext? context)
    {
        if (context is null) return;

        var now = DateTime.Now;
        var user = _currentUserService.UserName ?? "system";
        var empresaId = _currentUserService.EmpresaId;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.FechaCreacion = now;
                    entry.Entity.CreadoPor = user;

                    // Auto-set EmpresaId en entidades tenant-aware nuevas
                    if (entry.Entity is TenantEntity tenantEntity
                        && tenantEntity.EmpresaId == 0
                        && empresaId.HasValue)
                    {
                        tenantEntity.EmpresaId = empresaId.Value;
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.FechaModificacion = now;
                    entry.Entity.ModificadoPor = user;
                    // No permitir sobreescribir campos de creación
                    entry.Property(nameof(AuditableEntity.FechaCreacion)).IsModified = false;
                    entry.Property(nameof(AuditableEntity.CreadoPor)).IsModified = false;
                    break;
            }
        }
    }

    // ──── Captura de AuditLog ────

    private List<AuditEntry> CaptureAuditEntries(DbContext? context)
    {
        if (context is null) return new();

        var entries = new List<AuditEntry>();
        var now = DateTime.Now;
        var user = _currentUserService.UserName ?? "system";
        var empresaId = _currentUserService.EmpresaId;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry
            {
                Entry = entry,
                Entidad = entry.Entity.GetType().Name,
                Accion = entry.State switch
                {
                    EntityState.Added => "Insert",
                    EntityState.Modified => "Update",
                    EntityState.Deleted => "Delete",
                    _ => "Unknown"
                },
                Usuario = user,
                EmpresaId = empresaId,
                FechaHora = now
            };

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                // Capturar PK (puede ser temporal en Added)
                if (property.Metadata.IsPrimaryKey())
                {
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                    }
                    else
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    }
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NuevosValores[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.AnterioresValores[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified when property.IsModified:
                        auditEntry.CamposModificadosList.Add(propertyName);
                        auditEntry.AnterioresValores[propertyName] = property.OriginalValue;
                        auditEntry.NuevosValores[propertyName] = property.CurrentValue;
                        break;
                }
            }

            entries.Add(auditEntry);
        }

        return entries;
    }

    private void WritePendingAuditLogs(DbContext? context)
    {
        if (context is null || _pendingAuditEntries.Count == 0) return;

        var auditLogs = BuildAuditLogs();
        if (auditLogs.Count == 0) return;

        context.Set<AuditLog>().AddRange(auditLogs);
        context.SaveChanges();
    }

    private async Task WritePendingAuditLogsAsync(DbContext? context, CancellationToken ct)
    {
        if (context is null || _pendingAuditEntries.Count == 0) return;

        var auditLogs = BuildAuditLogs();
        if (auditLogs.Count == 0) return;

        context.Set<AuditLog>().AddRange(auditLogs);
        await context.SaveChangesAsync(ct);
    }

    private List<AuditLog> BuildAuditLogs()
    {
        var logs = new List<AuditLog>();
        var jsonOptions = new JsonSerializerOptions { WriteIndented = false };

        foreach (var entry in _pendingAuditEntries)
        {
            // Resolver PKs temporales (generados por DB) tras el save
            foreach (var prop in entry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    entry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                else
                {
                    entry.NuevosValores[prop.Metadata.Name] = prop.CurrentValue;
                }
            }

            var entidadId = entry.KeyValues.Count == 1
                ? entry.KeyValues.Values.First()?.ToString() ?? ""
                : JsonSerializer.Serialize(entry.KeyValues, jsonOptions);

            logs.Add(new AuditLog
            {
                Entidad = entry.Entidad,
                EntidadId = entidadId,
                Accion = entry.Accion,
                ValoresAnteriores = entry.AnterioresValores.Count > 0
                    ? JsonSerializer.Serialize(entry.AnterioresValores, jsonOptions)
                    : null,
                ValoresNuevos = entry.NuevosValores.Count > 0
                    ? JsonSerializer.Serialize(entry.NuevosValores, jsonOptions)
                    : null,
                CamposModificados = entry.CamposModificadosList.Count > 0
                    ? string.Join(", ", entry.CamposModificadosList)
                    : null,
                EmpresaId = entry.EmpresaId,
                Usuario = entry.Usuario,
                FechaHora = entry.FechaHora
            });
        }

        _pendingAuditEntries.Clear();
        return logs;
    }

    // ──── Clase auxiliar para captura temporal ────

    private class AuditEntry
    {
        public EntityEntry Entry { get; set; } = null!;
        public string Entidad { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string? Usuario { get; set; }
        public int? EmpresaId { get; set; }
        public DateTime FechaHora { get; set; }
        public Dictionary<string, object?> KeyValues { get; } = new();
        public Dictionary<string, object?> AnterioresValores { get; } = new();
        public Dictionary<string, object?> NuevosValores { get; } = new();
        public List<string> CamposModificadosList { get; } = new();
        public List<PropertyEntry> TemporaryProperties { get; } = new();
    }
}
