namespace AgoraHub360.ERP.Persistence.Interceptors;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
/// Interceptor que establece automáticamente los campos de auditoría
/// (FechaCreacion, CreadoPor, FechaModificacion, ModificadoPor) en cada SaveChanges.
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditableEntityInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null) return;

        var now = DateTime.UtcNow;
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
}
