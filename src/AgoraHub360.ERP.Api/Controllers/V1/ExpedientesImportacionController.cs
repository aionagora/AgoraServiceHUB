namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Compras;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compras/expedientes")]
[Authorize]
public class ExpedientesImportacionController : ControllerBase
{
    private readonly IExpedienteImportacionService _service;

    public ExpedientesImportacionController(IExpedienteImportacionService service)
    {
        _service = service;
    }

    /// <summary>Lista expedientes con filtros opcionales.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? estado,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(estado, desde, hasta, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<ExpedienteImportacionDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<ExpedienteImportacionDto>>.Ok(result.Value!));
    }

    /// <summary>Obtiene un expediente por Id con hitos y OCs vinculadas.</summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<ExpedienteImportacionDto>.Fail(result.Error!));
        return Ok(ApiResponse<ExpedienteImportacionDto>.Ok(result.Value!));
    }

    /// <summary>Crea un expediente vinculando 1..N OCs.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpedienteImportacionDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ExpedienteImportacionDto>.Fail(result.Error!));
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.ExpedienteImportacionId },
            ApiResponse<ExpedienteImportacionDto>.Ok(result.Value!, "Expediente de importación creado."));
    }

    /// <summary>Actualiza datos de embarque (forwarder, BL/AWB, ETD, ETA, etc.).</summary>
    [HttpPut("{id:long}/embarque")]
    public async Task<IActionResult> UpdateEmbarque(long id, [FromBody] UpdateExpedienteEmbarqueDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateEmbarqueAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ExpedienteImportacionDto>.Fail(result.Error!));
        return Ok(ApiResponse<ExpedienteImportacionDto>.Ok(result.Value!, "Datos de embarque actualizados."));
    }

    /// <summary>Confirma la salida/despacho: Borrador ? EnTransito.</summary>
    [HttpPost("{id:long}/confirmar-salida")]
    public async Task<IActionResult> ConfirmarSalida(long id, [FromQuery] DateTime fechaSalida, CancellationToken ct)
    {
        var result = await _service.ConfirmarSalidaAsync(id, fechaSalida, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ExpedienteImportacionDto>.Fail(result.Error!));
        return Ok(ApiResponse<ExpedienteImportacionDto>.Ok(result.Value!, "Salida confirmada. Expediente en tránsito."));
    }

    /// <summary>Registra el arribo: EnTransito ? Arribado.</summary>
    [HttpPost("{id:long}/arribo")]
    public async Task<IActionResult> RegistrarArribo(long id, [FromBody] RegistrarArriboDto dto, CancellationToken ct)
    {
        var result = await _service.RegistrarArriboAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ExpedienteImportacionDto>.Fail(result.Error!));
        return Ok(ApiResponse<ExpedienteImportacionDto>.Ok(result.Value!, "Arribo registrado."));
    }

    /// <summary>Inicia despacho aduanero: Arribado ? EnAduana.</summary>
    [HttpPost("{id:long}/despacho-aduanero")]
    public async Task<IActionResult> IniciarDespachoAduanero(long id, [FromBody] RegistrarDespachoAduaneroDto dto, CancellationToken ct)
    {
        var result = await _service.IniciarDespachoAduaneroAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ExpedienteImportacionDto>.Fail(result.Error!));
        return Ok(ApiResponse<ExpedienteImportacionDto>.Ok(result.Value!, "Despacho aduanero iniciado."));
    }

    /// <summary>Gate: registra observación/aforo aduanero ? ObservacionAduana.</summary>
    [HttpPost("{id:long}/observacion-aduana")]
    public async Task<IActionResult> RegistrarObservacion(long id, [FromBody] RegistrarObservacionAduanaDto dto, CancellationToken ct)
    {
        var result = await _service.RegistrarObservacionAduanaAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ExpedienteImportacionDto>.Fail(result.Error!));
        return Ok(ApiResponse<ExpedienteImportacionDto>.Ok(result.Value!, "Observación aduanera registrada."));
    }

    /// <summary>Subsana observación y vuelve a EnAduana.</summary>
    [HttpPost("{id:long}/subsanar-observacion")]
    public async Task<IActionResult> SubsanarObservacion(long id, [FromQuery] string? observaciones, CancellationToken ct)
    {
        var result = await _service.SubsanarObservacionAsync(id, observaciones, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ExpedienteImportacionDto>.Fail(result.Error!));
        return Ok(ApiResponse<ExpedienteImportacionDto>.Ok(result.Value!, "Observación subsanada. Expediente vuelve a despacho aduanero."));
    }

    /// <summary>Registra el levante/liberación aduanera ? Liberado.</summary>
    [HttpPost("{id:long}/levante")]
    public async Task<IActionResult> RegistrarLevante(long id, [FromBody] RegistrarLevanteDto dto, CancellationToken ct)
    {
        var result = await _service.RegistrarLevanteAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ExpedienteImportacionDto>.Fail(result.Error!));
        return Ok(ApiResponse<ExpedienteImportacionDto>.Ok(result.Value!, "Levante registrado. Mercadería liberada."));
    }

    /// <summary>Agrega un hito de tracking manual al expediente.</summary>
    [HttpPost("{id:long}/hitos")]
    public async Task<IActionResult> AddHito(long id, [FromBody] AddHitoExpedienteDto dto, CancellationToken ct)
    {
        var result = await _service.AddHitoAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ExpedienteImportacionDto>.Fail(result.Error!));
        return Ok(ApiResponse<ExpedienteImportacionDto>.Ok(result.Value!, "Hito registrado."));
    }

    /// <summary>Cierra el expediente cuando todas las OCs están cerradas.</summary>
    [HttpPost("{id:long}/cerrar")]
    public async Task<IActionResult> Cerrar(long id, CancellationToken ct)
    {
        var result = await _service.CerrarAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ExpedienteImportacionDto>.Fail(result.Error!));
        return Ok(ApiResponse<ExpedienteImportacionDto>.Ok(result.Value!, "Expediente de importación cerrado."));
    }

    /// <summary>Elimina (soft-delete) un expediente en Borrador.</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Expediente eliminado."));
    }
}
