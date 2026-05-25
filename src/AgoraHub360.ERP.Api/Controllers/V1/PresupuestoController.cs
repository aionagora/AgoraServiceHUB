namespace AgoraHub360.ERP.Api.Controllers.V1;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Presupuestos;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/presupuestos")]
[Authorize]
public class PresupuestoController : ControllerBase
{
    private readonly IPresupuestoService _service;

    public PresupuestoController(IPresupuestoService service)
    {
        _service = service;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<PresupuestoContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<PresupuestoContableDto>.Ok(result.Value!));
    }

    [HttpGet("comparativo")]
    public async Task<IActionResult> GetComparativo(
        [FromQuery] int gestion,
        [FromQuery] int? mes,
        [FromQuery] int? cuentaId,
        [FromQuery] int? centroCostoId,
        CancellationToken ct)
    {
        if (gestion <= 0) return BadRequest(ApiResponse<IEnumerable<PresupuestoVsRealDto>>.Fail("Gestión inválida."));

        var result = await _service.GetComparativoAsync(gestion, mes, cuentaId, centroCostoId, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IEnumerable<PresupuestoVsRealDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IEnumerable<PresupuestoVsRealDto>>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePresupuestoDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<PresupuestoContableDto>.Fail(result.Error!));
        
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.PresupuestoContableId }, ApiResponse<PresupuestoContableDto>.Ok(result.Value));
    }

    [HttpPost("{id:int}/aprobar")]
    public async Task<IActionResult> Aprobar(int id, CancellationToken ct)
    {
        var result = await _service.AprobarAsync(id, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<string>.Fail(result.Error!));

        return Ok(ApiResponse<string>.Ok("Presupuesto aprobado correctamente."));
    }

    [HttpPost("importar")]
    public async Task<IActionResult> Importar([FromForm] IFormFile file, [FromForm] int gestion, CancellationToken ct)
    {
        if (file == null || file.Length == 0) return BadRequest(ApiResponse<CargaMasivaResultDto>.Fail("Archivo no proporcionado."));
        if (gestion <= 0) return BadRequest(ApiResponse<CargaMasivaResultDto>.Fail("Gestión inválida."));

        using var stream = file.OpenReadStream();
        
        try
        {
            var result = await _service.ImportarDesdeExcelAsync(gestion, stream, ct);
            if (!result.IsSuccess) return BadRequest(ApiResponse<CargaMasivaResultDto>.Fail(result.Error!));
            return Ok(ApiResponse<CargaMasivaResultDto>.Ok(result.Value!));
        }
        catch (System.NotImplementedException ex)
        {
            return StatusCode(501, ApiResponse<CargaMasivaResultDto>.Fail(ex.Message));
        }
    }
}
