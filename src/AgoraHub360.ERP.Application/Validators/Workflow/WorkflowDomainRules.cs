namespace AgoraHub360.ERP.Application.Validators.Workflow;

using AgoraHub360.ERP.Domain.Entities.Workflow;

/// <summary>
/// Reglas de dominio estáticas para el módulo Workflow.
/// Centralizan las decisiones de negocio sobre transiciones de estado
/// y permisos de operación sobre tareas.
/// </summary>
public static class WorkflowDomainRules
{
    // ?? Estados válidos ???????????????????????????????????????????????????????
    public const string Pendiente   = "PENDIENTE";
    public const string EnProceso   = "EN_PROCESO";
    public const string Completado  = "COMPLETADO";
    public const string Bloqueado   = "BLOQUEADO";

    /// <summary>
    /// Grafo de transiciones válidas.
    /// Key   = estado actual.
    /// Value = estados destino permitidos.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string[]> Transiciones =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [Pendiente]  = new[] { EnProceso,  Pendiente },
            [EnProceso]  = new[] { Completado, Bloqueado, Pendiente },
            [Bloqueado]  = new[] { Pendiente },
            [Completado] = Array.Empty<string>(),   // estado terminal — no se puede mover
        };

    // ?? Operaciones permitidas ????????????????????????????????????????????????

    /// <summary>
    /// La tarea puede marcarse como completada si:
    /// • su estado actual NO es ya "COMPLETADO", y
    /// • está activa (no fue eliminada lógicamente).
    /// </summary>
    public static bool PuedeCompletarse(Tarea t)
        => t.Estado != Completado && t.Activo;

    /// <summary>
    /// La tarea puede reordenarse dentro del workflow solo si
    /// aún no ha comenzado (estado PENDIENTE).
    /// Tareas en proceso, bloqueadas o completadas tienen posición fija.
    /// </summary>
    public static bool PuedeReordenarse(Tarea t)
        => t.Estado == Pendiente;

    /// <summary>
    /// Verifica si la transición de <paramref name="estadoActual"/> a
    /// <paramref name="estadoNuevo"/> es válida según el grafo de dominio.
    /// La transición a PENDIENTE (reset) está siempre permitida desde
    /// cualquier estado excepto COMPLETADO.
    /// </summary>
    /// <param name="estadoActual">Estado de origen (case-insensitive).</param>
    /// <param name="estadoNuevo">Estado de destino (case-insensitive).</param>
    public static bool TransicionEstadoValida(string estadoActual, string estadoNuevo)
    {
        if (!Transiciones.TryGetValue(estadoActual, out var permitidos))
            return false;   // estado actual desconocido

        return permitidos.Contains(estadoNuevo, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Retorna el array de estados destino válidos desde <paramref name="estadoActual"/>.
    /// Útil para poblar un select en la UI solo con opciones legítimas.
    /// </summary>
    public static IEnumerable<string> TransicionesDisponibles(string estadoActual)
        => Transiciones.TryGetValue(estadoActual, out var permitidos)
            ? permitidos
            : Enumerable.Empty<string>();
}
