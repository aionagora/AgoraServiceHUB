namespace AgoraHub360.ERP.Shared.DTOs.Workflow;

/// <summary>
/// Tarea de workflow asociada a una entidad del sistema.
/// </summary>
public class TareaDto
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Completada { get; set; }
    public DateTime? FechaCompletado { get; set; }
    public string? CompletadoPor { get; set; }
    public int Orden { get; set; }
    public bool Obligatoria { get; set; }
    public DateTime FechaCreacion { get; set; }
}

/// <summary>
/// DTO para reordenar tareas.
/// </summary>
public class ReorderTareasDto
{
    /// <summary>
    /// Lista de IDs de tareas en el nuevo orden deseado.
    /// </summary>
    public List<int> TareaIds { get; set; } = new();
}
