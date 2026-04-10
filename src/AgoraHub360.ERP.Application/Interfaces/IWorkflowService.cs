namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Workflow;

/// <summary>
/// Servicio de aplicación para el módulo Workflow.
/// Orquesta la creación, avance y consulta de tareas sobre documentos del ERP.
/// </summary>
public interface IWorkflowService
{
    /// <summary>Devuelve todas las tareas activas de un documento, ordenadas por Orden.</summary>
    Task<Result<List<TareaDto>>> GetByEntityAsync(
        string entityType, int entityId, int empresaId, CancellationToken ct = default);

    /// <summary>Crea manualmente una tarea individual para un documento.</summary>
    Task<Result<TareaDto>> CreateTareaAsync(
        TareaCreateDto dto, int empresaId, CancellationToken ct = default);

    /// <summary>Marca una tarea como completada y registra fecha real y metadata.</summary>
    Task<Result<TareaDto>> CompletarTareaAsync(
        CompletarTareaDto dto, int empresaId, CancellationToken ct = default);

    /// <summary>Genera el conjunto inicial de tareas desde la plantilla configurada.</summary>
    Task<Result<bool>> GenerarHitosInicialesAsync(
        GenerarHitosDto dto, CancellationToken ct = default);

    /// <summary>Actualización parcial (patch) de una tarea.</summary>
    Task<Result<TareaDto>> UpdateTareaAsync(
        int id, TareaUpdateDto dto, int empresaId, CancellationToken ct = default);

    /// <summary>Reordena tareas de un documento. Solo permite mover las PENDIENTES.</summary>
    Task<Result<bool>> ReordenarTareasAsync(
        int empresaId, ReordenarTareasDto dto, CancellationToken ct = default);

    /// <summary>Desactiva lógicamente todas las tareas de un documento.</summary>
    Task<Result<bool>> DesactivarTareasAsync(
        string entityType, int entityId, int empresaId, CancellationToken ct = default);

    /// <summary>Resumen de progreso: conteos por estado, porcentaje y próxima fecha.</summary>
    Task<Result<TareaResumenDto>> GetResumenAsync(
        string entityType, int entityId, int empresaId, CancellationToken ct = default);

    /// <summary>Plantillas disponibles para un EntityType, con fallback a globales.</summary>
    Task<Result<List<PlantillaTareaDto>>> GetPlantillasAsync(
        string entityType, string? subTipo, int empresaId, CancellationToken ct = default);

    // ?? ABM de Plantillas de empresa ??????????????????????????????????????????

    /// <summary>Todas las plantillas visibles para la empresa (propias + globales).</summary>
    Task<Result<List<PlantillaTareaDto>>> GetAllPlantillasAsync(
        int empresaId, CancellationToken ct = default);

    /// <summary>Crea una plantilla personalizada para la empresa.</summary>
    Task<Result<PlantillaTareaDto>> CreatePlantillaAsync(
        PlantillaTareaCreateDto dto, int empresaId, CancellationToken ct = default);

    /// <summary>Actualiza una plantilla de empresa (no permite modificar las globales).</summary>
    Task<Result<PlantillaTareaDto>> UpdatePlantillaAsync(
        int id, PlantillaTareaUpdateDto dto, int empresaId, CancellationToken ct = default);

    /// <summary>Desactiva lógicamente una plantilla de empresa.</summary>
    Task<Result<bool>> DeletePlantillaAsync(
        int id, int empresaId, CancellationToken ct = default);
}
