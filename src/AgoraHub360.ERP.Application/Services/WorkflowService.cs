namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Application.Validators.Workflow;
using AgoraHub360.ERP.Domain.Entities.Workflow;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Workflow;

/// <summary>
/// Implementación del servicio de Workflow.
/// Orquesta creación, avance y consulta de tareas sobre documentos del ERP.
/// El commit de cada operación de escritura es responsabilidad de este servicio
/// a través de <see cref="IUnitOfWork"/>.
/// </summary>
public sealed class WorkflowService : IWorkflowService
{
    private readonly IWorkflowRepository _repo;
    private readonly IUnitOfWork         _uow;
    private readonly ICurrentUserService _currentUser;

    public WorkflowService(
        IWorkflowRepository repo,
        IUnitOfWork         uow,
        ICurrentUserService currentUser)
    {
        _repo        = repo;
        _uow         = uow;
        _currentUser = currentUser;
    }

    // ?? Consultas ?????????????????????????????????????????????????????????????

    /// <inheritdoc/>
    public async Task<Result<List<TareaDto>>> GetByEntityAsync(
        string entityType, int entityId, int empresaId, CancellationToken ct = default)
    {
        var tareas = await _repo.GetByEntityAsync(entityType, entityId, empresaId, ct);
        return Result<List<TareaDto>>.Success(tareas.Select(MapToDto).ToList());
    }

    /// <inheritdoc/>
    public async Task<Result<TareaResumenDto>> GetResumenAsync(
        string entityType, int entityId, int empresaId, CancellationToken ct = default)
    {
        var tareas = await _repo.GetByEntityAsync(entityType, entityId, empresaId, ct);

        if (tareas.Count == 0)
            return Result<TareaResumenDto>.Failure("No existen tareas para el documento indicado.");

        var total       = tareas.Count;
        var completadas = tareas.Count(t => t.Estado == WorkflowDomainRules.Completado);
        var pendientes  = tareas.Count(t => t.Estado == WorkflowDomainRules.Pendiente);
        var bloqueadas  = tareas.Count(t => t.Estado == WorkflowDomainRules.Bloqueado);

        var proximaVencimiento = tareas
            .Where(t => t.Estado  != WorkflowDomainRules.Completado && t.FechaPlan.HasValue)
            .OrderBy(t => t.FechaPlan)
            .Select(t => t.FechaPlan)
            .FirstOrDefault();

        var resumen = new TareaResumenDto
        {
            EntityType               = entityType,
            EntityId                 = entityId,
            TotalTareas              = total,
            Completadas              = completadas,
            Pendientes               = pendientes,
            Bloqueadas               = bloqueadas,
            ProximaFechaVencimiento  = proximaVencimiento,
        };

        return Result<TareaResumenDto>.Success(resumen);
    }

    // ?? Escritura ?????????????????????????????????????????????????????????????

    /// <inheritdoc/>
    public async Task<Result<TareaDto>> CompletarTareaAsync(
        CompletarTareaDto dto, int empresaId, CancellationToken ct = default)
    {
        // 1. Obtener tarea
        var tarea = await _repo.GetByIdAsync(dto.TareaId, empresaId, ct);
        if (tarea is null)
            return Result<TareaDto>.Failure($"Tarea {dto.TareaId} no encontrada.");

        // 2. Validar regla de dominio
        if (!WorkflowDomainRules.PuedeCompletarse(tarea))
            return Result<TareaDto>.Failure(
                $"La tarea '{tarea.Codigo}' no puede completarse (estado actual: {tarea.Estado}).");

        // 3-8. Aplicar cambios
        tarea.Completado       = true;
        tarea.FechaReal        = dto.FechaReal ?? DateOnly.FromDateTime(DateTime.Today);
        tarea.Estado           = WorkflowDomainRules.Completado;
        tarea.Observaciones    = dto.Observaciones ?? tarea.Observaciones;
        if (dto.MetadataJson is not null)
            tarea.MetadataJson = dto.MetadataJson;
        tarea.ModificadoPor    = _currentUser.UserName;
        tarea.FechaModificacion = DateTime.UtcNow;

        // 9. Actualizar y guardar
        await _repo.UpdateAsync(tarea, ct);
        await _uow.SaveChangesAsync();

        // 10. Retornar DTO mapeado
        return Result<TareaDto>.Success(MapToDto(tarea));
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> GenerarHitosInicialesAsync(
        GenerarHitosDto dto, CancellationToken ct = default)
    {
        // 1. Verificar que no existan hitos previos para este documento
        var yaExisten = await _repo.ExisteEntityAsync(dto.EntityType, dto.EntityId, dto.EmpresaId, ct);
        if (yaExisten)
            return Result<bool>.Failure("Hitos ya generados para este documento.");

        // 2. Obtener plantillas (empresa ? fallback global)
        var plantillas = await _repo.GetPlantillasAsync(dto.EntityType, dto.SubTipo, dto.EmpresaId, ct);
        if (plantillas.Count == 0)
            return Result<bool>.Failure(
                $"Plantilla no configurada para EntityType='{dto.EntityType}', SubTipo='{dto.SubTipo ?? "—"}'.");

        // 3. Construir las tareas desde las plantillas
        var ahora = DateTime.UtcNow;
        var tareas = plantillas.Select(p => new Tarea
        {
            EntityType      = dto.EntityType,
            EntityId        = dto.EntityId,
            EmpresaId       = dto.EmpresaId,
            Orden           = p.Orden,
            Codigo          = p.Codigo,
            Descripcion     = p.Descripcion,
            Responsable     = p.RolResponsable,
            // Hitos automáticos se completan de inmediato
            Estado          = p.EsAutomatico ? WorkflowDomainRules.Completado : WorkflowDomainRules.Pendiente,
            Completado      = p.EsAutomatico,
            FechaReal       = p.EsAutomatico ? DateOnly.FromDateTime(DateTime.Today) : null,
            Activo          = true,
            FechaCreacion   = ahora,
            CreadoPor       = _currentUser.UserName,
        }).ToList();

        // 4. Persistir
        await _repo.AddRangeAsync(tareas, ct);
        await _uow.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    /// <inheritdoc/>
    public async Task<Result<TareaDto>> UpdateTareaAsync(
        int id, TareaUpdateDto dto, int empresaId, CancellationToken ct = default)
    {
        var tarea = await _repo.GetByIdAsync(id, empresaId, ct);
        if (tarea is null)
            return Result<TareaDto>.Failure($"Tarea {id} no encontrada.");

        // Validar transición de estado si se solicita cambio
        if (dto.Estado is not null &&
            !WorkflowDomainRules.TransicionEstadoValida(tarea.Estado, dto.Estado))
        {
            return Result<TareaDto>.Failure(
                $"Transición de estado inválida: '{tarea.Estado}' ? '{dto.Estado}'.");
        }

        // Aplicar solo campos enviados (patch semántico)
        if (dto.Descripcion   is not null) tarea.Descripcion   = dto.Descripcion;
        if (dto.FechaPlan     is not null) tarea.FechaPlan     = dto.FechaPlan;
        if (dto.FechaReal     is not null) tarea.FechaReal     = dto.FechaReal;
        if (dto.Responsable   is not null) tarea.Responsable   = dto.Responsable;
        if (dto.Observaciones is not null) tarea.Observaciones = dto.Observaciones;
        if (dto.MetadataJson  is not null) tarea.MetadataJson  = dto.MetadataJson;

        if (dto.Estado is not null)
        {
            tarea.Estado     = dto.Estado;
            tarea.Completado = dto.Estado == WorkflowDomainRules.Completado;
        }

        tarea.ModificadoPor     = _currentUser.UserName;
        tarea.FechaModificacion = DateTime.UtcNow;

        await _repo.UpdateAsync(tarea, ct);
        await _uow.SaveChangesAsync();

        return Result<TareaDto>.Success(MapToDto(tarea));
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> ReordenarTareasAsync(
        int empresaId, ReordenarTareasDto dto, CancellationToken ct = default)
    {
        if (dto.Items.Count == 0)
            return Result<bool>.Failure("La lista de items de reordenamiento está vacía.");

        var ahora = DateTime.UtcNow;
        var errores = new List<string>();

        foreach (var item in dto.Items)
        {
            var tarea = await _repo.GetByIdAsync(item.TareaId, empresaId, ct);
            if (tarea is null)
            {
                errores.Add($"Tarea {item.TareaId} no encontrada.");
                continue;
            }

            if (!WorkflowDomainRules.PuedeReordenarse(tarea))
            {
                errores.Add(
                    $"La tarea '{tarea.Codigo}' no puede reordenarse (estado: {tarea.Estado}).");
                continue;
            }

            tarea.Orden             = item.NuevoOrden;
            tarea.ModificadoPor     = _currentUser.UserName;
            tarea.FechaModificacion = ahora;
            await _repo.UpdateAsync(tarea, ct);
        }

        if (errores.Count > 0)
            return Result<bool>.Failure(string.Join(" | ", errores));

        await _uow.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> DesactivarTareasAsync(
        string entityType, int entityId, int empresaId, CancellationToken ct = default)
    {
        await _repo.DesactivarPorEntityAsync(entityType, entityId, empresaId, ct);
        await _uow.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    /// <inheritdoc/>
    public async Task<Result<TareaDto>> CreateTareaAsync(
        TareaCreateDto dto, int empresaId, CancellationToken ct = default)
    {
        var yaExiste = await _repo.GetByCodigoAsync(dto.EntityType, dto.EntityId, dto.Codigo, empresaId, ct);
        if (yaExiste is not null)
            return Result<TareaDto>.Failure(
                $"Ya existe una tarea con código '{dto.Codigo}' para este documento.");

        var tarea = new Tarea
        {
            EntityType    = dto.EntityType,
            EntityId      = dto.EntityId,
            EmpresaId     = empresaId,
            Orden         = dto.Orden,
            Codigo        = dto.Codigo,
            Descripcion   = dto.Descripcion,
            FechaPlan     = dto.FechaPlan,
            Responsable   = dto.Responsable,
            Observaciones = dto.Observaciones,
            MetadataJson  = dto.MetadataJson,
            Estado        = WorkflowDomainRules.Pendiente,
            Completado    = false,
            Activo        = true,
            FechaCreacion = DateTime.UtcNow,
            CreadoPor     = _currentUser.UserName,
        };

        await _repo.AddAsync(tarea, ct);
        await _uow.SaveChangesAsync();

        return Result<TareaDto>.Success(MapToDto(tarea));
    }

    /// <inheritdoc/>
    public async Task<Result<List<PlantillaTareaDto>>> GetPlantillasAsync(
        string entityType, string? subTipo, int empresaId, CancellationToken ct = default)
    {
        var plantillas = await _repo.GetPlantillasAsync(entityType, subTipo, empresaId, ct);
        var dtos = plantillas.Select(p => new PlantillaTareaDto
        {
            Id             = p.Id,
            EntityType     = p.EntityType,
            SubTipo        = p.SubTipo,
            Orden          = p.Orden,
            Codigo         = p.Codigo,
            Descripcion    = p.Descripcion,
            EsAutomatico   = p.EsAutomatico,
            RolResponsable = p.RolResponsable,
            EmpresaId      = p.EmpresaId,
        }).ToList();

        return Result<List<PlantillaTareaDto>>.Success(dtos);
    }

    // ?? Mapeo manual ??????????????????????????????????????????????????????????

    private static TareaDto MapToDto(Tarea t) => new()
    {
        Id            = t.Id,
        EntityType    = t.EntityType,
        EntityId      = t.EntityId,
        EmpresaId     = t.EmpresaId,
        Orden         = t.Orden,
        Codigo        = t.Codigo,
        Descripcion   = t.Descripcion,
        FechaPlan     = t.FechaPlan,
        FechaReal     = t.FechaReal,
        Estado        = t.Estado,
        Responsable   = t.Responsable,
        Completado    = t.Completado,
        Observaciones = t.Observaciones,
        MetadataJson  = t.MetadataJson,
        FechaCreacion = t.FechaCreacion,
        CreadoPor     = t.CreadoPor,
    };

    private static PlantillaTareaDto MapPlantillaToDto(PlantillaTarea p) => new()
    {
        Id             = p.Id,
        EntityType     = p.EntityType,
        SubTipo        = p.SubTipo,
        Orden          = p.Orden,
        Codigo         = p.Codigo,
        Descripcion    = p.Descripcion,
        EsAutomatico   = p.EsAutomatico,
        RolResponsable = p.RolResponsable,
        EmpresaId      = p.EmpresaId,
    };

    // ?? ABM Plantillas ????????????????????????????????????????????????????????

    /// <inheritdoc/>
    public async Task<Result<List<PlantillaTareaDto>>> GetAllPlantillasAsync(
        int empresaId, CancellationToken ct = default)
    {
        var lista = await _repo.GetAllPlantillasAsync(empresaId, ct);
        return Result<List<PlantillaTareaDto>>.Success(lista.Select(MapPlantillaToDto).ToList());
    }

    /// <inheritdoc/>
    public async Task<Result<PlantillaTareaDto>> CreatePlantillaAsync(
        PlantillaTareaCreateDto dto, int empresaId, CancellationToken ct = default)
    {
        var plantilla = new PlantillaTarea
        {
            EntityType     = dto.EntityType,
            SubTipo        = string.IsNullOrWhiteSpace(dto.SubTipo) ? null : dto.SubTipo,
            Orden          = dto.Orden,
            Codigo         = dto.Codigo.ToUpperInvariant(),
            Descripcion    = dto.Descripcion,
            EsAutomatico   = dto.EsAutomatico,
            RolResponsable = string.IsNullOrWhiteSpace(dto.RolResponsable) ? null : dto.RolResponsable.ToUpperInvariant(),
            EmpresaId      = empresaId,   // siempre empresa — no globales
            Activo         = true,
            FechaCreacion  = DateTime.UtcNow,
            CreadoPor      = _currentUser.UserName,
        };

        await _repo.AddPlantillaAsync(plantilla, ct);
        await _uow.SaveChangesAsync();

        return Result<PlantillaTareaDto>.Success(MapPlantillaToDto(plantilla));
    }

    /// <inheritdoc/>
    public async Task<Result<PlantillaTareaDto>> UpdatePlantillaAsync(
        int id, PlantillaTareaUpdateDto dto, int empresaId, CancellationToken ct = default)
    {
        var plantilla = await _repo.GetPlantillaByIdAsync(id, empresaId, ct);
        if (plantilla is null)
            return Result<PlantillaTareaDto>.Failure($"Plantilla {id} no encontrada.");

        // Las plantillas globales (EmpresaId == null) no se pueden modificar desde la UI
        if (plantilla.EmpresaId is null)
            return Result<PlantillaTareaDto>.Failure(
                "Las plantillas globales del sistema no pueden modificarse. Cree una plantilla de empresa.");

        if (dto.Orden          is not null) plantilla.Orden          = dto.Orden.Value;
        if (dto.Descripcion    is not null) plantilla.Descripcion    = dto.Descripcion;
        if (dto.EsAutomatico   is not null) plantilla.EsAutomatico   = dto.EsAutomatico.Value;
        if (dto.RolResponsable is not null)
            plantilla.RolResponsable = string.IsNullOrWhiteSpace(dto.RolResponsable)
                ? null : dto.RolResponsable.ToUpperInvariant();

        plantilla.ModificadoPor     = _currentUser.UserName;
        plantilla.FechaModificacion = DateTime.UtcNow;

        await _repo.UpdatePlantillaAsync(plantilla, ct);
        await _uow.SaveChangesAsync();

        return Result<PlantillaTareaDto>.Success(MapPlantillaToDto(plantilla));
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> DeletePlantillaAsync(
        int id, int empresaId, CancellationToken ct = default)
    {
        var plantilla = await _repo.GetPlantillaByIdAsync(id, empresaId, ct);
        if (plantilla is null)
            return Result<bool>.Failure($"Plantilla {id} no encontrada.");

        if (plantilla.EmpresaId is null)
            return Result<bool>.Failure(
                "Las plantillas globales del sistema no pueden eliminarse.");

        plantilla.Activo            = false;
        plantilla.ModificadoPor     = _currentUser.UserName;
        plantilla.FechaModificacion = DateTime.UtcNow;

        await _repo.UpdatePlantillaAsync(plantilla, ct);
        await _uow.SaveChangesAsync();

        return Result<bool>.Success(true);
    }
}
