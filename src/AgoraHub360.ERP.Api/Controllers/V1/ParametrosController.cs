namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Parametro;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ParametrosController : ControllerBase
{
    private readonly IParametroSistemaService _service;

    public ParametrosController(IParametroSistemaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<ParametroSistemaDto>>.Ok(result.Value!));
    }

    [HttpGet("categoria/{categoria}")]
    public async Task<IActionResult> GetByCategoria(string categoria, CancellationToken ct)
    {
        var result = await _service.GetByCategoriaAsync(categoria, ct);
        return Ok(ApiResponse<IReadOnlyList<ParametroSistemaDto>>.Ok(result.Value!));
    }

    [HttpGet("clave/{clave}")]
    public async Task<IActionResult> GetByClave(string clave, CancellationToken ct)
    {
        var result = await _service.GetByClaveAsync(clave, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<ParametroSistemaDto>.Fail(result.Error!));

        return Ok(ApiResponse<ParametroSistemaDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] UpsertParametroDto dto, CancellationToken ct)
    {
        var result = await _service.UpsertAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ParametroSistemaDto>.Fail(result.Error!));

        return Ok(ApiResponse<ParametroSistemaDto>.Ok(result.Value!, "Parámetro guardado exitosamente."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Parámetro eliminado exitosamente."));
    }
}
