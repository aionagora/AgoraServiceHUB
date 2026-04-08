namespace AgoraHub360.ERP.Shared.DTOs.Workflow;

// ?????????????????????????????????????????????????????????????????????????????
// MÓDULO WORKFLOW — DTOs
// Todos los DTOs del módulo wf consolidados en este archivo.
// ?????????????????????????????????????????????????????????????????????????????

// ?? DTO 1: TareaDto — Lectura / Respuesta ?????????????????????????????????????

/// <summary>
/// DTO de lectura completo para una tarea del workflow.
/// Usado en respuestas de API y binding de componentes Blazor.
/// </summary>
public class TareaDto
{
    public int         Id           { get; set; }
    public string      EntityType   { get; set; } = string.Empty;
    public int         EntityId     { get; set; }
    public int         EmpresaId    { get; set; }

    public int         Orden        { get; set; }
    public string      Codigo       { get; set; } = string.Empty;
    public string      Descripcion  { get; set; } = string.Empty;

    public DateOnly?   FechaPlan    { get; set; }
    public DateOnly?   FechaReal    { get; set; }

    public string      Estado       { get; set; } = string.Empty;
    public string?     Responsable  { get; set; }
    public bool        Completado   { get; set; }

    public string?     Observaciones { get; set; }
    public string?     MetadataJson  { get; set; }

    public DateTime    FechaCreacion { get; set; }
    public string?     CreadoPor     { get; set; }
}

// ?? DTO 2: TareaCreateDto — Creación manual ???????????????????????????????????

/// <summary>
/// DTO para creación manual de una tarea en un documento existente.
/// EntityType y EntityId identifican el documento padre.
/// </summary>
public class TareaCreateDto
{
    /// <summary>Tipo de documento padre. Ej: "OrdenPedido", "OrdenCompra".</summary>
    public string  EntityType   { get; set; } = string.Empty;

    /// <summary>PK del documento padre.</summary>
    public int     EntityId     { get; set; }

    /// <summary>Posición en la secuencia del workflow (1-based).</summary>
    public int     Orden        { get; set; }

    /// <summary>Código funcional del hito. Ej: ETD, ETA, DUI.</summary>
    public string  Codigo       { get; set; } = string.Empty;

    /// <summary>Descripción legible del hito.</summary>
    public string  Descripcion  { get; set; } = string.Empty;

    public DateOnly?  FechaPlan    { get; set; }
    public string?    Responsable  { get; set; }
    public string?    Observaciones { get; set; }
    public string?    MetadataJson  { get; set; }
}

// ?? DTO 3: TareaUpdateDto — Actualización parcial inline ?????????????????????

/// <summary>
/// DTO para actualización parcial de una tarea (edición inline en grilla).
/// Solo se actualizan los campos enviados; null = sin cambio.
/// </summary>
public class TareaUpdateDto
{
    public string?   Descripcion   { get; set; }
    public DateOnly? FechaPlan     { get; set; }
    public DateOnly? FechaReal     { get; set; }
    public string?   Estado        { get; set; }
    public string?   Responsable   { get; set; }
    public string?   Observaciones { get; set; }
    public string?   MetadataJson  { get; set; }
}

// ?? DTO 4: CompletarTareaDto — Completar un hito ?????????????????????????????

/// <summary>
/// DTO para marcar una tarea como completada.
/// Si FechaReal es null, el servicio usará DateOnly.FromDateTime(DateTime.Today).
/// </summary>
public class CompletarTareaDto
{
    /// <summary>Id de la tarea a completar. Obligatorio.</summary>
    public int       TareaId       { get; set; }

    /// <summary>Fecha real de ejecución. Null = fecha de hoy.</summary>
    public DateOnly? FechaReal     { get; set; }

    /// <summary>Observaciones registradas al completar el hito.</summary>
    public string?   Observaciones { get; set; }

    /// <summary>
    /// Metadata específica capturada al completar.
    /// Ej: número de BL, número de DUI, referencia de pago.
    /// </summary>
    public string?   MetadataJson  { get; set; }
}

// ?? DTO 5: GenerarHitosDto — Generar hitos desde plantilla ???????????????????

/// <summary>
/// DTO para instanciar automáticamente las tareas de un documento
/// a partir de la plantilla global (o empresa) que corresponda.
/// </summary>
public class GenerarHitosDto
{
    /// <summary>Tipo de documento. Ej: "OrdenPedido", "OrdenCompra".</summary>
    public string  EntityType { get; set; } = string.Empty;

    /// <summary>PK del documento al que se le generarán los hitos.</summary>
    public int     EntityId   { get; set; }

    /// <summary>Empresa del documento (para buscar plantilla personalizada o global).</summary>
    public int     EmpresaId  { get; set; }

    /// <summary>
    /// Sub-tipo de operación para seleccionar la plantilla correcta.
    /// Ej: "IMPORTACION", "TRASPASO_INTERNO". Null = plantilla genérica del EntityType.
    /// </summary>
    public string? SubTipo    { get; set; }
}

// ?? DTO 6: ReordenarTareasDto — Reordenar secuencia de hitos ?????????????????

/// <summary>
/// DTO para actualizar el orden de un conjunto de tareas de un documento.
/// Se envía la lista completa de id + nuevo orden tras drag-and-drop en la UI.
/// </summary>
public class ReordenarTareasDto
{
    public List<TareaOrdenItem> Items { get; set; } = new();
}

/// <summary>Par (TareaId, NuevoOrden) para la operación de reordenamiento.</summary>
public class TareaOrdenItem
{
    /// <summary>Id de la tarea a reordenar.</summary>
    public int TareaId     { get; set; }

    /// <summary>Nueva posición en la secuencia (1-based).</summary>
    public int NuevoOrden  { get; set; }
}

// ?? DTO 7: TareaResumenDto — Resumen para dashboard ??????????????????????????

/// <summary>
/// DTO de resumen del estado de workflow para un documento específico.
/// Usado en tarjetas de dashboard, indicadores KPI y barras de progreso.
/// </summary>
public class TareaResumenDto
{
    public string   EntityType  { get; set; } = string.Empty;
    public int      EntityId    { get; set; }

    public int      TotalTareas  { get; set; }
    public int      Completadas  { get; set; }
    public int      Pendientes   { get; set; }
    public int      Bloqueadas   { get; set; }

    /// <summary>Fecha planificada más próxima entre las tareas no completadas.</summary>
    public DateOnly? ProximaFechaVencimiento { get; set; }

    /// <summary>
    /// Porcentaje de avance calculado: (Completadas / TotalTareas) * 100.
    /// Retorna 0 si TotalTareas es 0 para evitar división por cero.
    /// </summary>
    public decimal PorcentajeAvance =>
        TotalTareas == 0 ? 0m : Math.Round((decimal)Completadas / TotalTareas * 100, 1);
}

// ?? DTO 8: PlantillaTareaDto — Lectura de plantillas ?????????????????????????

/// <summary>
/// DTO de lectura para una plantilla de tarea del workflow.
/// Usado en el endpoint GET /api/wf/plantillas para mostrar
/// las plantillas disponibles por EntityType y SubTipo.
/// </summary>
public class PlantillaTareaDto
{
    public int      Id              { get; set; }
    public string   EntityType      { get; set; } = string.Empty;
    public string?  SubTipo         { get; set; }
    public int      Orden           { get; set; }
    public string   Codigo          { get; set; } = string.Empty;
    public string   Descripcion     { get; set; } = string.Empty;
    public bool     EsAutomatico    { get; set; }
    public string?  RolResponsable  { get; set; }
    public int?     EmpresaId       { get; set; }
}

// ?? DTO 9: PlantillaTareaCreateDto ????????????????????????????????????????????

/// <summary>
/// DTO para crear una plantilla de tarea personalizada de empresa.
/// Las plantillas globales (EmpresaId = null) se gestionan solo por seed.
/// </summary>
public class PlantillaTareaCreateDto
{
    public string  EntityType     { get; set; } = string.Empty;
    public string? SubTipo        { get; set; }
    public int     Orden          { get; set; }
    public string  Codigo         { get; set; } = string.Empty;
    public string  Descripcion    { get; set; } = string.Empty;
    public bool    EsAutomatico   { get; set; } = false;
    public string? RolResponsable { get; set; }
}

// ?? DTO 10: PlantillaTareaUpdateDto ??????????????????????????????????????????

/// <summary>
/// DTO para actualizar una plantilla existente de empresa.
/// Todos los campos son opcionales; null = sin cambio.
/// </summary>
public class PlantillaTareaUpdateDto
{
    public int?    Orden          { get; set; }
    public string? Descripcion    { get; set; }
    public bool?   EsAutomatico   { get; set; }
    public string? RolResponsable { get; set; }
}
