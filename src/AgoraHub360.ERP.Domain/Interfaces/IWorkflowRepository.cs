namespace AgoraHub360.ERP.Domain.Interfaces;

using AgoraHub360.ERP.Domain.Entities.Workflow;

/// <summary>
/// Repositorio especializado para el módulo Workflow.
/// Expone operaciones de acceso a datos optimizadas para
/// <see cref="Tarea"/> y <see cref="PlantillaTarea"/>.
/// Todas las queries respetan el scope de empresa (EmpresaId).
/// </summary>
public interface IWorkflowRepository
{
    // ?? Consultas de Tarea ????????????????????????????????????????????????????

    /// <summary>
    /// Devuelve todas las tareas activas de un documento, ordenadas por Orden.
    /// Usa el índice IX_Tareas_Entity ? Index Seek O(log n).
    /// </summary>
    Task<List<Tarea>> GetByEntityAsync(string entityType, int entityId, int empresaId,
        CancellationToken ct = default);

    /// <summary>
    /// Devuelve una tarea por su Id dentro del scope de empresa.
    /// Retorna null si no existe o no pertenece a la empresa.
    /// </summary>
    Task<Tarea?> GetByIdAsync(int id, int empresaId,
        CancellationToken ct = default);

    /// <summary>
    /// Devuelve la tarea con el código indicado para un documento específico.
    /// El código es único por (EntityType, EntityId, Codigo, EmpresaId).
    /// </summary>
    Task<Tarea?> GetByCodigoAsync(string entityType, int entityId, string codigo, int empresaId,
        CancellationToken ct = default);

    /// <summary>
    /// Comprueba si ya existen tareas para el documento indicado.
    /// Útil para evitar doble generación de hitos.
    /// </summary>
    Task<bool> ExisteEntityAsync(string entityType, int entityId, int empresaId,
        CancellationToken ct = default);

    // ?? Consultas de PlantillaTarea ???????????????????????????????????????????

    /// <summary>
    /// Devuelve las plantillas para un EntityType + SubTipo, aplicando fallback:
    /// 1º busca plantillas propias de la empresa (<paramref name="empresaId"/>),
    /// si no hay ? devuelve las globales (EmpresaId = null).
    /// Solo se incluyen plantillas cuyo SubTipo coincida exactamente
    /// o cuyo SubTipo sea null (aplica a todos los sub-tipos).
    /// </summary>
    Task<List<PlantillaTarea>> GetPlantillasAsync(string entityType, string? subTipo, int? empresaId,
        CancellationToken ct = default);

    // ?? Resúmenes / Agregados ?????????????????????????????????????????????????

    /// <summary>
    /// Devuelve un diccionario [Estado ? cantidad] con el conteo de tareas
    /// activas de la empresa, agrupado por estado.
    /// Usado para KPIs y badges de dashboard.
    /// </summary>
    Task<Dictionary<string, int>> GetResumenEstadosPorEmpresaAsync(int empresaId,
        CancellationToken ct = default);

    // ?? Escritura ?????????????????????????????????????????????????????????????

    /// <summary>Agrega una tarea al contexto (sin SaveChanges).</summary>
    Task AddAsync(Tarea tarea, CancellationToken ct = default);

    /// <summary>Agrega un lote de tareas al contexto (sin SaveChanges).</summary>
    Task AddRangeAsync(List<Tarea> tareas, CancellationToken ct = default);

    /// <summary>Marca la tarea como modificada en el contexto (sin SaveChanges).</summary>
    Task UpdateAsync(Tarea tarea, CancellationToken ct = default);

    /// <summary>
    /// Desactiva lógicamente (Activo = false) todas las tareas de un documento.
    /// Operación usada al anular o eliminar el documento padre.
    /// No llama a SaveChanges.
    /// </summary>
    Task DesactivarPorEntityAsync(string entityType, int entityId, int empresaId,
        CancellationToken ct = default);

    // ?? ABM de PlantillaTarea (empresa) ???????????????????????????????????????

    /// <summary>Devuelve todas las plantillas activas de una empresa más las globales.</summary>
    Task<List<PlantillaTarea>> GetAllPlantillasAsync(int empresaId, CancellationToken ct = default);

    /// <summary>Devuelve una plantilla por Id, validando que pertenezca a la empresa.</summary>
    Task<PlantillaTarea?> GetPlantillaByIdAsync(int id, int empresaId, CancellationToken ct = default);

    /// <summary>Agrega una plantilla de empresa al contexto (sin SaveChanges).</summary>
    Task AddPlantillaAsync(PlantillaTarea plantilla, CancellationToken ct = default);

    /// <summary>Marca la plantilla como modificada en el contexto (sin SaveChanges).</summary>
    Task UpdatePlantillaAsync(PlantillaTarea plantilla, CancellationToken ct = default);
}
