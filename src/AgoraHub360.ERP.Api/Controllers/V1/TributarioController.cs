namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Tributario;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tributario")]
[Authorize]
public class TributarioController : ControllerBase
{
    private readonly IImpuestoService _impuestoService;
    private readonly ILogger<TributarioController> _logger;

    public TributarioController(
        IImpuestoService impuestoService,
        ILogger<TributarioController> logger)
    {
        _impuestoService = impuestoService;
        _logger = logger;
    }

    [HttpGet("iva")]
    [ProducesResponseType(typeof(ApiResponse<RegistroImpuestoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RegistroImpuestoDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalcularIVA([FromQuery] int periodoId, CancellationToken ct)
    {
        if (periodoId <= 0) return BadRequest(ApiResponse<RegistroImpuestoDto>.Fail("ID de período inválido."));

        var result = await _impuestoService.CalcularIVAAsync(periodoId, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<RegistroImpuestoDto>.Fail(result.Error! ?? "Error al calcular IVA."));

        return Ok(ApiResponse<RegistroImpuestoDto>.Ok(result.Value!));
    }

    [HttpGet("it")]
    [ProducesResponseType(typeof(ApiResponse<RegistroImpuestoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RegistroImpuestoDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalcularIT([FromQuery] int periodoId, CancellationToken ct)
    {
        if (periodoId <= 0) return BadRequest(ApiResponse<RegistroImpuestoDto>.Fail("ID de período inválido."));

        var result = await _impuestoService.CalcularITAsync(periodoId, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<RegistroImpuestoDto>.Fail(result.Error! ?? "Error al calcular IT."));

        return Ok(ApiResponse<RegistroImpuestoDto>.Ok(result.Value!));
    }

    [HttpGet("iue")]
    [ProducesResponseType(typeof(ApiResponse<RegistroImpuestoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RegistroImpuestoDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalcularIUE([FromQuery] int gestion, CancellationToken ct)
    {
        if (gestion <= 0) return BadRequest(ApiResponse<RegistroImpuestoDto>.Fail("Gestión inválida."));

        var result = await _impuestoService.CalcularIUEAsync(gestion, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<RegistroImpuestoDto>.Fail(result.Error! ?? "Error al calcular IUE."));

        return Ok(ApiResponse<RegistroImpuestoDto>.Ok(result.Value!));
    }

    [HttpGet("formulario-200")]
    [ProducesResponseType(typeof(ApiResponse<FormularioSINDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FormularioSINDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<FormularioSINDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFormulario200([FromQuery] int periodoId, CancellationToken ct)
    {
        if (periodoId <= 0) return BadRequest(ApiResponse<FormularioSINDto>.Fail("ID de período inválido."));

        var result = await _impuestoService.GetDatosFormulario200Async(periodoId, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<FormularioSINDto>.Fail(result.Error! ?? "Formulario no encontrado."));

        return Ok(ApiResponse<FormularioSINDto>.Ok(result.Value!));
    }

    [HttpGet("formulario-400")]
    [ProducesResponseType(typeof(ApiResponse<FormularioSINDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FormularioSINDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<FormularioSINDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFormulario400([FromQuery] int periodoId, CancellationToken ct)
    {
        if (periodoId <= 0) return BadRequest(ApiResponse<FormularioSINDto>.Fail("ID de período inválido."));

        var result = await _impuestoService.GetDatosFormulario400Async(periodoId, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<FormularioSINDto>.Fail(result.Error! ?? "Formulario no encontrado."));

        return Ok(ApiResponse<FormularioSINDto>.Ok(result.Value!));
    }

    [HttpGet("formulario-500")]
    [ProducesResponseType(typeof(ApiResponse<FormularioSINDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FormularioSINDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<FormularioSINDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFormulario500([FromQuery] int gestion, CancellationToken ct)
    {
        if (gestion <= 0) return BadRequest(ApiResponse<FormularioSINDto>.Fail("Gestión inválida."));

        var result = await _impuestoService.GetDatosFormulario500Async(gestion, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<FormularioSINDto>.Fail(result.Error! ?? "Formulario no encontrado."));

        return Ok(ApiResponse<FormularioSINDto>.Ok(result.Value!));
    }

    [HttpPost("{id:long}/declarar")]
    // [Authorize(Roles = "Contador,Admin,Admin Contable")] // Se puede habilitar si los roles coinciden con el tenant local
    [ProducesResponseType(typeof(ApiResponse<RegistroImpuestoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RegistroImpuestoDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeclararImpuesto(long id, [FromBody] DeclararImpuestoRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.NumeroCertificado))
            return BadRequest(ApiResponse<RegistroImpuestoDto>.Fail("El número de certificado es obligatorio."));

        var result = await _impuestoService.MarcarDeclaradoAsync(id, request.NumeroCertificado, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<RegistroImpuestoDto>.Fail(result.Error! ?? "Error al declarar impuesto."));

        return Ok(ApiResponse<RegistroImpuestoDto>.Ok(result.Value!));
    }
}
