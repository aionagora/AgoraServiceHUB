namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contabilidad")]
[Authorize]
public class ContabilidadController : ControllerBase
{
    private readonly ICierreContableService _cierreService;

    public ContabilidadController(ICierreContableService cierreService)
    {
        _cierreService = cierreService;
    }

    [HttpPost("cierre-contable")]
    public async Task<IActionResult> EjecutarCierre([FromBody] EjecutarCierreDto dto, CancellationToken ct)
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (!int.TryParse(claim, out int empresaId)) return Unauthorized();

        try
        {
            var result = await _cierreService.EjecutarCierreAsync(empresaId, dto, ct);
            return Ok(ApiResponse<CierreContableDto>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CierreContableDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpGet("cierre-contable/{gestion}")]
    public async Task<IActionResult> ObtenerCierre(int gestion, CancellationToken ct)
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (!int.TryParse(claim, out int empresaId)) return Unauthorized();

        var result = await _cierreService.ObtenerCierreAsync(empresaId, gestion, ct);
        return result is not null
            ? Ok(ApiResponse<CierreContableDto>.Ok(result))
            : Ok(ApiResponse<CierreContableDto>.Ok(null));
    }

    [HttpPost("cierre-contable/apertura/{gestionNueva}")]
    public async Task<IActionResult> EjecutarApertura(int gestionNueva, CancellationToken ct)
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (!int.TryParse(claim, out int empresaId)) return Unauthorized();

        try
        {
            await _cierreService.EjecutarAperturaAsync(empresaId, gestionNueva, ct);
            return Ok(ApiResponse<bool>.Ok(true));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }
}
