namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Workflow;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Endpoints REST del módulo Workflow.
/// Gestiona tareas de hitos sobre cualquier documento del ERP
/// (OrdenPedido, OrdenCompra, OrdenVenta, Embarque).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/wf")]
[Authorize]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowService    _service;
    private readonly ICurrentUserService _currentUser;

    public WorkflowController(
        IWorkflowService    service,
        ICurrentUserService currentUser)
    {
        _service     = service;
        _currentUser = currentUser;
    }

    // ?? Helpers ???????????????????????????????????????????????????????????????

    /// <summary>
    /// Obtiene el EmpresaId del usuario autenticado.
    /// Retorna un BadRequest si el claim no está presente.
    /// </summary>
    private bool TryGetEmpresaId(out int empresaId)
    {
        empresaId = _currentUser.EmpresaId ?? 0;
        return empresaId > 0;
    }

    // ?? GET /api/wf/tareas ????????????????????????????????????????????????????

    /// <summary>
    /// Lista todas las tareas activas de un documento, ordenadas por Orden.
    /// </summary>
    [HttpGet("tareas")]
    public async Task<IActionResult> GetByEntity(
        [FromQuery] string entityType,
        [FromQuery] int    entityId,
        CancellationToken  ct)
    {
        if (!TryGetEmpresaId(out var empresaId))
            return BadRequest(ApiResponse<List<TareaDto>>.Fail("EmpresaId no encontrado en el token."));

        var result = await _service.GetByEntityAsync(entityType, entityId, empresaId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<List<TareaDto>>.Fail(result.Error!));

        return Ok(ApiResponse<List<TareaDto>>.Ok(result.Value!));
    }

    // ?? GET /api/wf/tareas/{id}/resumen ???????????????????????????????????????

    /// <summary>
    /// Resumen de progreso del workflow: conteos por estado, % avance y próxima fecha.
    /// </summary>
    [HttpGet("tareas/{id:int}/resumen")]
    public async Task<IActionResult> GetResumen(
        int               id,
        [FromQuery] string entityType,
        [FromQuery] int    entityId,
        CancellationToken  ct)
    {
        if (!TryGetEmpresaId(out var empresaId))
            return BadRequest(ApiResponse<TareaResumenDto>.Fail("EmpresaId no encontrado en el token."));

        var result = await _service.GetResumenAsync(entityType, entityId, empresaId, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<TareaResumenDto>.Fail(result.Error!));

        return Ok(ApiResponse<TareaResumenDto>.Ok(result.Value!));
    }

    // ?? POST /api/wf/tareas ???????????????????????????????????????????????????

    /// <summary>
    /// Crea una tarea individual para un documento (creación manual).
    /// </summary>
    [HttpPost("tareas")]
    public async Task<IActionResult> CreateTarea(
        [FromBody]  TareaCreateDto             dto,
        [FromServices] IValidator<TareaCreateDto> validator,
        CancellationToken                      ct)
    {
        if (!TryGetEmpresaId(out var empresaId))
            return BadRequest(ApiResponse<TareaDto>.Fail("EmpresaId no encontrado en el token."));

        var validation = await validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<TareaDto>.Fail("Datos de tarea inválidos.", errors));
        }

        var result = await _service.CreateTareaAsync(dto, empresaId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<TareaDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetByEntity),
            new { entityType = dto.EntityType, entityId = dto.EntityId },
            ApiResponse<TareaDto>.Ok(result.Value!, "Tarea creada."));
    }

    // ?? POST /api/wf/tareas/generar-iniciales ?????????????????????????????????

    /// <summary>
    /// Instancia automáticamente los hitos de un documento desde la plantilla configurada.
    /// </summary>
    [HttpPost("tareas/generar-iniciales")]
    public async Task<IActionResult> GenerarIniciales(
        [FromBody]  GenerarHitosDto                dto,
        [FromServices] IValidator<GenerarHitosDto> validator,
        CancellationToken                          ct)
    {
        var validation = await validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<bool>.Fail("Datos inválidos para generar hitos.", errors));
        }

        var result = await _service.GenerarHitosInicialesAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Hitos generados correctamente."));
    }

    // ?? PATCH /api/wf/tareas/{id} ?????????????????????????????????????????????

    /// <summary>
    /// Actualiza parcialmente una tarea (edición inline en grilla).
    /// </summary>
    [HttpPatch("tareas/{id:int}")]
    public async Task<IActionResult> UpdateTarea(
        int                id,
        [FromBody] TareaUpdateDto dto,
        CancellationToken  ct)
    {
        if (!TryGetEmpresaId(out var empresaId))
            return BadRequest(ApiResponse<TareaDto>.Fail("EmpresaId no encontrado en el token."));

        var result = await _service.UpdateTareaAsync(id, dto, empresaId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<TareaDto>.Fail(result.Error!));

        return Ok(ApiResponse<TareaDto>.Ok(result.Value!, "Tarea actualizada."));
    }

    // ?? PATCH /api/wf/tareas/{id}/completar ???????????????????????????????????

    /// <summary>
    /// Marca un hito como completado y registra fecha real y metadata.
    /// </summary>
    [HttpPatch("tareas/{id:int}/completar")]
    public async Task<IActionResult> CompletarTarea(
        int                      id,
        [FromBody] CompletarTareaDto dto,
        [FromServices] IValidator<CompletarTareaDto> validator,
        CancellationToken        ct)
    {
        if (!TryGetEmpresaId(out var empresaId))
            return BadRequest(ApiResponse<TareaDto>.Fail("EmpresaId no encontrado en el token."));

        // Asegura que el id de ruta coincida con el del body
        dto.TareaId = id;

        var validation = await validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<TareaDto>.Fail("Datos inválidos para completar tarea.", errors));
        }

        var result = await _service.CompletarTareaAsync(dto, empresaId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<TareaDto>.Fail(result.Error!));

        return Ok(ApiResponse<TareaDto>.Ok(result.Value!, "Tarea completada."));
    }

    // ?? PATCH /api/wf/tareas/reorder ?????????????????????????????????????????

    /// <summary>
    /// Actualiza el orden de un conjunto de tareas tras drag-and-drop en la UI.
    /// </summary>
    [HttpPatch("tareas/reorder")]
    public async Task<IActionResult> ReordenarTareas(
        [FromBody] ReordenarTareasDto dto,
        CancellationToken             ct)
    {
        if (!TryGetEmpresaId(out var empresaId))
            return BadRequest(ApiResponse<bool>.Fail("EmpresaId no encontrado en el token."));

        var result = await _service.ReordenarTareasAsync(empresaId, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return NoContent();
    }

    // ?? DELETE /api/wf/tareas/entity ?????????????????????????????????????????

    /// <summary>
    /// Desactiva lógicamente todas las tareas de un documento.
    /// Requiere rol SUPERVISOR o GERENCIA.
    /// </summary>
    [HttpDelete("tareas/entity")]
    [Authorize(Roles = "SUPERVISOR,GERENCIA")]
    public async Task<IActionResult> DesactivarPorEntity(
        [FromQuery] string entityType,
        [FromQuery] int    entityId,
        CancellationToken  ct)
    {
        if (!TryGetEmpresaId(out var empresaId))
            return BadRequest(ApiResponse<bool>.Fail("EmpresaId no encontrado en el token."));

        var result = await _service.DesactivarTareasAsync(entityType, entityId, empresaId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return NoContent();
    }

    // ?? GET /api/wf/plantillas ????????????????????????????????????????????????

    /// <summary>
    /// Devuelve las plantillas de hitos disponibles para un tipo de documento.
    /// Aplica fallback: plantillas de empresa ? plantillas globales.
    /// </summary>
    [HttpGet("plantillas")]
    public async Task<IActionResult> GetPlantillas(
        [FromQuery] string  entityType,
        [FromQuery] string? subTipo,
        CancellationToken   ct)
    {
        if (!TryGetEmpresaId(out var empresaId))
            return BadRequest(ApiResponse<List<PlantillaTareaDto>>.Fail("EmpresaId no encontrado en el token."));

        var result = await _service.GetPlantillasAsync(entityType, subTipo, empresaId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<List<PlantillaTareaDto>>.Fail(result.Error!));

        return Ok(ApiResponse<List<PlantillaTareaDto>>.Ok(result.Value!));
    }

    // ?? GET /api/wf/plantillas/all ????????????????????????????????????????????

    /// <summary>Todas las plantillas visibles (empresa + globales) para el ABM.</summary>
    [HttpGet("plantillas/all")]
    public async Task<IActionResult> GetAllPlantillas(CancellationToken ct)
    {
        if (!TryGetEmpresaId(out var empresaId))
            return BadRequest(ApiResponse<List<PlantillaTareaDto>>.Fail("EmpresaId no encontrado en el token."));

        var result = await _service.GetAllPlantillasAsync(empresaId, ct);
        return Ok(ApiResponse<List<PlantillaTareaDto>>.Ok(result.Value!));
    }

    // ?? POST /api/wf/plantillas ???????????????????????????????????????????????

    /// <summary>Crea una plantilla personalizada para la empresa.</summary>
    [HttpPost("plantillas")]
    public async Task<IActionResult> CreatePlantilla(
        [FromBody] PlantillaTareaCreateDto dto,
        CancellationToken ct)
    {
        if (!TryGetEmpresaId(out var empresaId))
            return BadRequest(ApiResponse<PlantillaTareaDto>.Fail("EmpresaId no encontrado en el token."));

        if (string.IsNullOrWhiteSpace(dto.EntityType) || string.IsNullOrWhiteSpace(dto.Codigo)
            || string.IsNullOrWhiteSpace(dto.Descripcion) || dto.Orden <= 0)
            return BadRequest(ApiResponse<PlantillaTareaDto>.Fail("EntityType, Codigo, Descripcion y Orden son obligatorios."));

        var result = await _service.CreatePlantillaAsync(dto, empresaId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<PlantillaTareaDto>.Fail(result.Error!));

        return Ok(ApiResponse<PlantillaTareaDto>.Ok(result.Value!, "Plantilla creada."));
    }

    // ?? PATCH /api/wf/plantillas/{id} ?????????????????????????????????????????

    /// <summary>Actualiza una plantilla de empresa.</summary>
    [HttpPatch("plantillas/{id:int}")]
    public async Task<IActionResult> UpdatePlantilla(
        int id,
        [FromBody] PlantillaTareaUpdateDto dto,
        CancellationToken ct)
    {
        if (!TryGetEmpresaId(out var empresaId))
            return BadRequest(ApiResponse<PlantillaTareaDto>.Fail("EmpresaId no encontrado en el token."));

        var result = await _service.UpdatePlantillaAsync(id, dto, empresaId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<PlantillaTareaDto>.Fail(result.Error!));

        return Ok(ApiResponse<PlantillaTareaDto>.Ok(result.Value!, "Plantilla actualizada."));
    }

    // ?? DELETE /api/wf/plantillas/{id} ????????????????????????????????????????

    /// <summary>Desactiva lógicamente una plantilla de empresa.</summary>
    [HttpDelete("plantillas/{id:int}")]
    public async Task<IActionResult> DeletePlantilla(int id, CancellationToken ct)
    {
        if (!TryGetEmpresaId(out var empresaId))
            return BadRequest(ApiResponse<bool>.Fail("EmpresaId no encontrado en el token."));

        var result = await _service.DeletePlantillaAsync(id, empresaId, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));

        return NoContent();
    }
}
