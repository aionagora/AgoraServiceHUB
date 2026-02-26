namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contabilidad/periodos")]
[Authorize]
public class PeriodosContablesController : ControllerBase
{
    private readonly IPeriodoContableService _service;

    public PeriodosContablesController(IPeriodoContableService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? anio, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(anio, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<PeriodoContableDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<PeriodoContableDto>>.Ok(result.Value!));
    }

    [HttpPost("generar")]
    public async Task<IActionResult> Generar([FromBody] GenerarPeriodosDto dto, CancellationToken ct)
    {
        var result = await _service.GenerarPeriodosAsync(dto.Anio, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<int>.Fail(result.Error!));
        return Ok(ApiResponse<int>.Ok(result.Value!, $"{result.Value} períodos generados para {dto.Anio}."));
    }

    [HttpPost("{id:int}/cerrar")]
    public async Task<IActionResult> Cerrar(int id, CancellationToken ct)
    {
        var result = await _service.CerrarAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<PeriodoContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<PeriodoContableDto>.Ok(result.Value!, $"Período {result.Value!.Nombre} cerrado."));
    }

    [HttpPost("{id:int}/reabrir")]
    public async Task<IActionResult> Reabrir(int id, CancellationToken ct)
    {
        var result = await _service.ReabrirAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<PeriodoContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<PeriodoContableDto>.Ok(result.Value!, $"Período {result.Value!.Nombre} reabierto."));
    }
}
