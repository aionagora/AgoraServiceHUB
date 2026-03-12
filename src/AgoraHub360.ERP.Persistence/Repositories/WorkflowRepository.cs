namespace AgoraHub360.ERP.Persistence.Repositories;

using AgoraHub360.ERP.Domain.Entities.Workflow;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Implementación de <see cref="IWorkflowRepository"/> sobre EF Core.
/// Todas las queries de lectura usan AsNoTracking para máximo rendimiento.
/// Las escrituras no llaman SaveChanges — la responsabilidad es del servicio
/// a través de IUnitOfWork.
/// </summary>
public sealed class WorkflowRepository : IWorkflowRepository
{
    private readonly AgoraDbContext _context;

    public WorkflowRepository(AgoraDbContext context)
    {
        _context = context;
    }

    // ?? Consultas de Tarea ????????????????????????????????????????????????????

    /// <inheritdoc/>
    /// <remarks>
    /// Usa el índice IX_Tareas_Entity (EmpresaId, EntityType, EntityId, Orden)
    /// con INCLUDE(Estado, Completado, FechaReal) ? Index Seek O(log n),
    /// sin Key Lookup para las columnas más frecuentes.
    /// </remarks>
    public async Task<List<Tarea>> GetByEntityAsync(
        string entityType, int entityId, int empresaId, CancellationToken ct = default)
        => await _context.Tareas
            .Where(t => t.EmpresaId  == empresaId
                     && t.EntityType == entityType
                     && t.EntityId   == entityId
                     && t.Activo)
            .OrderBy(t => t.Orden)
            .AsNoTracking()
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<Tarea?> GetByIdAsync(int id, int empresaId, CancellationToken ct = default)
        => await _context.Tareas
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id        == id
                                   && t.EmpresaId == empresaId
                                   && t.Activo, ct);

    /// <inheritdoc/>
    /// <remarks>
    /// Usa el índice único IX_Tareas_Codigo_UQ (EmpresaId, EntityType, EntityId, Codigo).
    /// </remarks>
    public async Task<Tarea?> GetByCodigoAsync(
        string entityType, int entityId, string codigo, int empresaId, CancellationToken ct = default)
        => await _context.Tareas
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.EmpresaId  == empresaId
                                   && t.EntityType == entityType
                                   && t.EntityId   == entityId
                                   && t.Codigo     == codigo
                                   && t.Activo, ct);

    /// <inheritdoc/>
    public async Task<bool> ExisteEntityAsync(
        string entityType, int entityId, int empresaId, CancellationToken ct = default)
        => await _context.Tareas
            .AnyAsync(t => t.EmpresaId  == empresaId
                        && t.EntityType == entityType
                        && t.EntityId   == entityId
                        && t.Activo, ct);

    // ?? Consultas de PlantillaTarea ???????????????????????????????????????????

    /// <inheritdoc/>
    /// <remarks>
    /// Algoritmo de resolución de plantilla (empresa-first con fallback a global):
    /// 1. Busca plantillas con EmpresaId == empresaId.
    /// 2. Si no encuentra ninguna ? usa plantillas con EmpresaId == null.
    /// En ambos casos filtra por SubTipo exacto O SubTipo == null (genéricas).
    /// </remarks>
    public async Task<List<PlantillaTarea>> GetPlantillasAsync(
        string entityType, string? subTipo, int? empresaId, CancellationToken ct = default)
    {
        // Plantillas de la empresa
        if (empresaId.HasValue)
        {
            var propias = await _context.PlantillasTareas
                .Where(p => p.EntityType == entityType
                         && p.EmpresaId  == empresaId.Value
                         && (p.SubTipo   == null || p.SubTipo == subTipo)
                         && p.Activo)
                .OrderBy(p => p.Orden)
                .AsNoTracking()
                .ToListAsync(ct);

            if (propias.Count > 0)
                return propias;
        }

        // Fallback: plantillas globales (EmpresaId == null)
        return await _context.PlantillasTareas
            .Where(p => p.EntityType == entityType
                     && p.EmpresaId  == null
                     && (p.SubTipo   == null || p.SubTipo == subTipo)
                     && p.Activo)
            .OrderBy(p => p.Orden)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // ?? Resúmenes / Agregados ?????????????????????????????????????????????????

    /// <inheritdoc/>
    public async Task<Dictionary<string, int>> GetResumenEstadosPorEmpresaAsync(
        int empresaId, CancellationToken ct = default)
        => await _context.Tareas
            .Where(t => t.EmpresaId == empresaId && t.Activo)
            .GroupBy(t => t.Estado)
            .Select(g => new { Estado = g.Key, Count = g.Count() })
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Estado, x => x.Count, ct);

    // ?? Escritura ?????????????????????????????????????????????????????????????

    /// <inheritdoc/>
    public async Task AddAsync(Tarea tarea, CancellationToken ct = default)
        => await _context.Tareas.AddAsync(tarea, ct);

    /// <inheritdoc/>
    public async Task AddRangeAsync(List<Tarea> tareas, CancellationToken ct = default)
        => await _context.Tareas.AddRangeAsync(tareas, ct);

    /// <inheritdoc/>
    public Task UpdateAsync(Tarea tarea, CancellationToken ct = default)
    {
        _context.Tareas.Update(tarea);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Carga las entidades con tracking para aplicar la baja lógica en bloque.
    /// Más eficiente que ExecuteUpdate cuando el número de filas es pequeño
    /// (típicamente 5–15 hitos por documento).
    /// </remarks>
    public async Task DesactivarPorEntityAsync(
        string entityType, int entityId, int empresaId, CancellationToken ct = default)
    {
        var tareas = await _context.Tareas
            .Where(t => t.EmpresaId  == empresaId
                     && t.EntityType == entityType
                     && t.EntityId   == entityId
                     && t.Activo)
            .ToListAsync(ct);

        foreach (var t in tareas)
            t.Activo = false;

        // No SaveChanges — el servicio llama a IUnitOfWork.SaveChangesAsync()
    }

    // ?? ABM de PlantillaTarea (empresa) ???????????????????????????????????????

    /// <inheritdoc/>
    public async Task<List<PlantillaTarea>> GetAllPlantillasAsync(
        int empresaId, CancellationToken ct = default)
        => await _context.PlantillasTareas
            .Where(p => (p.EmpresaId == empresaId || p.EmpresaId == null) && p.Activo)
            .OrderBy(p => p.EntityType)
            .ThenBy(p => p.SubTipo)
            .ThenBy(p => p.Orden)
            .AsNoTracking()
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<PlantillaTarea?> GetPlantillaByIdAsync(
        int id, int empresaId, CancellationToken ct = default)
        => await _context.PlantillasTareas
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Id == id && (p.EmpresaId == empresaId || p.EmpresaId == null) && p.Activo, ct);

    /// <inheritdoc/>
    public async Task AddPlantillaAsync(PlantillaTarea plantilla, CancellationToken ct = default)
        => await _context.PlantillasTareas.AddAsync(plantilla, ct);

    /// <inheritdoc/>
    public Task UpdatePlantillaAsync(PlantillaTarea plantilla, CancellationToken ct = default)
    {
        _context.PlantillasTareas.Update(plantilla);
        return Task.CompletedTask;
    }
}
