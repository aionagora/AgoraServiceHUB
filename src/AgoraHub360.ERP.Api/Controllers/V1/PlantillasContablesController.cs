namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contabilidad/plantillas")]
[Authorize]
public class PlantillasContablesController : ControllerBase
{
    private readonly IPlantillaContableService _service;

    public PlantillasContablesController(IPlantillaContableService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? tipoDocumento, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(tipoDocumento, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IReadOnlyList<PlantillaContableDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<PlantillaContableDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<PlantillaContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<PlantillaContableDto>.Ok(result.Value!));
    }

    [HttpGet("tipo/{tipoDocumento}")]
    public async Task<IActionResult> GetByTipo(string tipoDocumento, CancellationToken ct)
    {
        var result = await _service.GetByTipoDocumentoAsync(tipoDocumento, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<PlantillaContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<PlantillaContableDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlantillaContableDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<PlantillaContableDto>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById),
            new { id = result.Value!.PlantillaContableId },
            ApiResponse<PlantillaContableDto>.Ok(result.Value!, "Plantilla creada."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePlantillaContableDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<PlantillaContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<PlantillaContableDto>.Ok(result.Value!, "Plantilla actualizada."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Plantilla eliminada."));
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed(CancellationToken ct)
    {
        var result = await _service.SeedPlantillasAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<int>.Fail(result.Error!));
        return Ok(ApiResponse<int>.Ok(result.Value!, $"{result.Value} plantillas generadas."));
    }
}
