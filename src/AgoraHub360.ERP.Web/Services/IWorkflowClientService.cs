namespace AgoraHub360.ERP.Web.Services;

using AgoraHub360.ERP.Shared.DTOs.Workflow;

/// <summary>
/// Abstracción del cliente HTTP del módulo Workflow para Blazor WASM.
/// Permite inyectar un mock en tests de componentes sin depender de HttpClient.
/// </summary>
public interface IWorkflowClientService
{
    /// <summary>Tareas activas de un documento, ordenadas por Orden.</summary>
    Task<List<TareaDto>> GetByEntityAsync(string entityType, int entityId);

    /// <summary>Resumen de progreso: conteos por estado, % avance y próxima fecha.</summary>
    Task<TareaResumenDto?> GetResumenAsync(string entityType, int entityId);

    /// <summary>Crea una tarea individual para un documento.</summary>
    Task<TareaDto?> CreateAsync(TareaCreateDto dto);

    /// <summary>Completa un hito y registra fecha real y metadata.</summary>
    Task<TareaDto?> CompletarAsync(CompletarTareaDto dto);

    /// <summary>Genera los hitos de un documento desde la plantilla configurada.</summary>
    Task GenerarHitosAsync(GenerarHitosDto dto);

    /// <summary>Actualización parcial (patch) de una tarea.</summary>
    Task<TareaDto?> UpdateAsync(int id, TareaUpdateDto dto);

    /// <summary>Reordena las tareas tras drag-and-drop en la UI.</summary>
    Task ReordenarAsync(ReordenarTareasDto dto);

    /// <summary>Plantillas disponibles para un EntityType, con fallback a globales.</summary>
    Task<List<PlantillaTareaDto>> GetPlantillasAsync(string entityType, string? subTipo = null);

    // ?? ABM de Plantillas ?????????????????????????????????????????????????????

    /// <summary>Todas las plantillas visibles (empresa + globales) para el ABM.</summary>
    Task<List<PlantillaTareaDto>> GetAllPlantillasAsync();

    /// <summary>Crea una plantilla personalizada de empresa.</summary>
    Task<PlantillaTareaDto?> CreatePlantillaAsync(PlantillaTareaCreateDto dto);

    /// <summary>Actualiza una plantilla de empresa.</summary>
    Task<PlantillaTareaDto?> UpdatePlantillaAsync(int id, PlantillaTareaUpdateDto dto);

    /// <summary>Desactiva lógicamente una plantilla de empresa.</summary>
    Task DeletePlantillaAsync(int id);
}
