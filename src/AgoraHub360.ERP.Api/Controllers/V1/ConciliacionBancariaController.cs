namespace AgoraHub360.ERP.Api.Controllers.V1;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Bancario;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/conciliacion-bancaria")]
[Authorize]
public class ConciliacionBancariaController : ControllerBase
{
    private readonly IConciliacionBancariaService _service;

    public ConciliacionBancariaController(IConciliacionBancariaService service)
    {
        _service = service;
    }

    [HttpPost("importar")]
    [ProducesResponseType(typeof(ApiResponse<ImportExtractoResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ImportExtractoResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Importar(
        [FromForm] ImportarExtractoFormRequestDto request,
        CancellationToken ct)
    {
        if (request.Archivo == null || request.Archivo.Length == 0)
            return BadRequest(ApiResponse<ImportExtractoResultDto>.Fail("Archivo no proporcionado o vacío."));

        if (request.CuentaId <= 0)
            return BadRequest(ApiResponse<ImportExtractoResultDto>.Fail("Cuenta contable inválida."));

        if (request.PeriodoId <= 0)
            return BadRequest(ApiResponse<ImportExtractoResultDto>.Fail("Período contable inválido."));

        var formatoLimpio = request.Formato?.ToLower().Trim();
        if (formatoLimpio != "csv" && formatoLimpio != "excel" && formatoLimpio != "xlsx")
            return BadRequest(ApiResponse<ImportExtractoResultDto>.Fail("Formato no soportado. Use 'csv' o 'excel'."));

        using var stream = request.Archivo.OpenReadStream();

        var result = await _service.ImportarExtractoAsync(request.CuentaId, request.PeriodoId, stream, formatoLimpio, ct);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ImportExtractoResultDto>.Fail(result.Error!));

        return Ok(ApiResponse<ImportExtractoResultDto>.Ok(result.Value!));
    }

    [HttpGet("sugerencias")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SugerenciaConciliacionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SugerenciaConciliacionDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSugerencias(
        [FromQuery] int cuentaId,
        [FromQuery] int periodoId,
        CancellationToken ct)
    {
        if (cuentaId <= 0)
            return BadRequest(ApiResponse<IEnumerable<SugerenciaConciliacionDto>>.Fail("Cuenta contable inválida."));

        if (periodoId <= 0)
            return BadRequest(ApiResponse<IEnumerable<SugerenciaConciliacionDto>>.Fail("Período contable inválido."));

        var result = await _service.GetSugerenciasAsync(cuentaId, periodoId, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<IEnumerable<SugerenciaConciliacionDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IEnumerable<SugerenciaConciliacionDto>>.Ok(result.Value!));
    }

    [HttpPost("conciliar")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Conciliar(
        [FromBody] ConciliarMovimientoRequestDto request,
        CancellationToken ct)
    {
        if (request == null || request.ExtractoId <= 0 || request.AsientoContableLineaId <= 0)
            return BadRequest(ApiResponse<string>.Fail("Solicitud inválida. Se requiere ExtractoId y AsientoContableLineaId válidos."));

        var result = await _service.ConciliarAsync(request.ExtractoId, request.AsientoContableLineaId, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<string>.Fail(result.Error!));

        return Ok(ApiResponse<string>.Ok("Movimiento conciliado con éxito."));
    }

    [HttpPost("desconciliar/{extractoId:long}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Desconciliar(
        long extractoId,
        CancellationToken ct)
    {
        if (extractoId <= 0)
            return BadRequest(ApiResponse<string>.Fail("ExtractoId inválido."));

        var result = await _service.DesconciliarAsync(extractoId, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<string>.Fail(result.Error!));

        return Ok(ApiResponse<string>.Ok("Movimiento desconciliado con éxito."));
    }

    [HttpGet("resumen")]
    [ProducesResponseType(typeof(ApiResponse<ResumenConciliacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ResumenConciliacionDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetResumen(
        [FromQuery] int cuentaId,
        [FromQuery] int periodoId,
        CancellationToken ct)
    {
        if (cuentaId <= 0)
            return BadRequest(ApiResponse<ResumenConciliacionDto>.Fail("Cuenta contable inválida."));

        if (periodoId <= 0)
            return BadRequest(ApiResponse<ResumenConciliacionDto>.Fail("Período contable inválido."));

        var result = await _service.GetResumenAsync(cuentaId, periodoId, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ResumenConciliacionDto>.Fail(result.Error!));

        return Ok(ApiResponse<ResumenConciliacionDto>.Ok(result.Value!));
    }

    [HttpPost("aprobar/{conciliacionId:int}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Aprobar(
        int conciliacionId,
        CancellationToken ct)
    {
        if (conciliacionId <= 0)
            return BadRequest(ApiResponse<string>.Fail("ConciliacionId inválido."));

        var result = await _service.AprobarAsync(conciliacionId, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<string>.Fail(result.Error!));

        return Ok(ApiResponse<string>.Ok("Conciliación aprobada con éxito."));
    }

    public class ImportarExtractoFormRequestDto
    {
        public IFormFile? Archivo { get; set; }
        public int CuentaId { get; set; }
        public int PeriodoId { get; set; }
        public string? Formato { get; set; }
    }
}
