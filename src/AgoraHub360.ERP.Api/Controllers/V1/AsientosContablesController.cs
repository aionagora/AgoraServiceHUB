namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contabilidad/asientos")]
[Authorize]
public class AsientosContablesController : ControllerBase
{
    private readonly IAsientoContableService _service;
    private readonly IComprobanteDocumentoService _docService;
    private readonly IAsientoExportService _exportService;

    public AsientosContablesController(
        IAsientoContableService service,
        IComprobanteDocumentoService docService,
        IAsientoExportService exportService)
    {
        _service       = service;
        _docService    = docService;
        _exportService = exportService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
        [FromQuery] string? estado, [FromQuery] int? tipoComprobanteId,
        [FromQuery] string? search, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(desde, hasta, estado, tipoComprobanteId, search, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<IReadOnlyList<AsientoContableDto>>.Fail(result.Error!));
        return Ok(ApiResponse<IReadOnlyList<AsientoContableDto>>.Ok(result.Value!));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess) return NotFound(ApiResponse<AsientoContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<AsientoContableDto>.Ok(result.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAsientoContableDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<AsientoContableDto>.Fail(result.Error!));
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.AsientoContableId },
            ApiResponse<AsientoContableDto>.Ok(result.Value!, "Comprobante creado."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateAsientoContableDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<AsientoContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<AsientoContableDto>.Ok(result.Value!, "Comprobante actualizado."));
    }

    [HttpPost("{id:long}/copiar")]
    public async Task<IActionResult> Copiar(long id, CancellationToken ct)
    {
        var result = await _service.CopiarAsync(id, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<AsientoContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<AsientoContableDto>.Ok(result.Value!, "Comprobante copiado."));
    }

    [HttpPost("{id:long}/contabilizar")]
    public async Task<IActionResult> Contabilizar(long id, CancellationToken ct)
    {
        var result = await _service.ContabilizarAsync(id, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<AsientoContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<AsientoContableDto>.Ok(result.Value!, "Comprobante contabilizado."));
    }

    [HttpPost("{id:long}/anular")]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
    {
        var result = await _service.AnularAsync(id, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<AsientoContableDto>.Fail(result.Error!));
        return Ok(ApiResponse<AsientoContableDto>.Ok(result.Value!, "Comprobante anulado."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        if (!result.IsSuccess) return BadRequest(ApiResponse<bool>.Fail(result.Error!));
        return Ok(ApiResponse<bool>.Ok(true, "Comprobante eliminado."));
    }

    // ?? Catálogos ??
    [HttpGet("tipos-comprobante")]
    public async Task<IActionResult> GetTiposComprobante(CancellationToken ct)
    {
        var r = await _service.GetTiposComprobanteAsync(ct);
        return r.IsSuccess ? Ok(ApiResponse<IReadOnlyList<TipoComprobanteDto>>.Ok(r.Value!)) : BadRequest(ApiResponse<IReadOnlyList<TipoComprobanteDto>>.Fail(r.Error!));
    }

    [HttpGet("tipos-cambio")]
    public async Task<IActionResult> GetTiposCambio(CancellationToken ct)
    {
        var r = await _service.GetTiposCambioAsync(ct);
        return r.IsSuccess ? Ok(ApiResponse<IReadOnlyList<TipoCambioDto>>.Ok(r.Value!)) : BadRequest(ApiResponse<IReadOnlyList<TipoCambioDto>>.Fail(r.Error!));
    }

    [HttpGet("tipos-pago")]
    public async Task<IActionResult> GetTiposPago(CancellationToken ct)
    {
        var r = await _service.GetTiposPagoAsync(ct);
        return r.IsSuccess ? Ok(ApiResponse<IReadOnlyList<TipoPagoDto>>.Ok(r.Value!)) : BadRequest(ApiResponse<IReadOnlyList<TipoPagoDto>>.Fail(r.Error!));
    }

    [HttpPost("seed-catalogos")]
    public async Task<IActionResult> SeedCatalogos(CancellationToken ct)
    {
        var r = await _service.SeedCatalogosAsync(ct);
        return r.IsSuccess ? Ok(ApiResponse<int>.Ok(r.Value!, $"{r.Value} catálogos generados.")) : BadRequest(ApiResponse<int>.Fail(r.Error!));
    }

    /// <summary>
    /// Exporta los comprobantes contables filtrados a un archivo Excel
    /// </summary>
    [HttpGet("exportar-excel")]
    public async Task<IActionResult> ExportarExcel(
        [FromQuery] DateTime? desde, 
        [FromQuery] DateTime? hasta,
        [FromQuery] string? estado, 
        [FromQuery] int? tipoComprobanteId,
        [FromQuery] string? search, 
        CancellationToken ct)
    {
        try
        {
            var result = await _service.GetAllAsync(desde, hasta, estado, tipoComprobanteId, search, ct);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<string>.Fail(result.Error!));

            var fileBytes = _exportService.ExportarExcel(result.Value!);
            var fileName = $"Comprobantes_Contables_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(fileBytes, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error al generar Excel: {ex.Message}"));
        }
    }

    /// <summary>
    /// Exporta un comprobante individual a Excel con cabecera + detalle de líneas
    /// </summary>
    [HttpGet("{id:long}/exportar-excel")]
    public async Task<IActionResult> ExportarExcelIndividual(long id, CancellationToken ct)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, ct);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<string>.Fail(result.Error!));

            var a = result.Value!;
            var fileBytes = _exportService.ExportarExcelIndividual(a);
            var numSafe = a.Numero.Replace("/", "-").Replace("\\", "-");

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Comprobante_{numSafe}.xlsx");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error al generar Excel: {ex.Message}"));
        }
    }

    /// <summary>
    /// Exporta listado detallado plano: una fila por línea de comprobante.
    /// Formato pensado para migración a otros sistemas contables.
    /// </summary>
    [HttpGet("exportar-excel-plano")]
    public async Task<IActionResult> ExportarExcelPlano(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? estado,
        [FromQuery] int? tipoComprobanteId,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        try
        {
            var result = await _service.GetAllAsync(desde, hasta, estado, tipoComprobanteId, search, ct);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<string>.Fail(result.Error!));

            var fileBytes = _exportService.ExportarExcelPlano(result.Value!);
            var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Comprobantes_Detalle_Plano_{ts}.xlsx");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error al generar Excel: {ex.Message}"));
        }
    }

    // ?? Documentos adjuntos ???????????????????????????????????????????????????

    /// <summary>Lista los documentos adjuntos a un comprobante.</summary>
    [HttpGet("{comprobanteId:long}/documentos")]
    public async Task<IActionResult> GetDocumentos(long comprobanteId, CancellationToken ct)
    {
        var result = await _docService.GetByComprobanteAsync(comprobanteId, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<IReadOnlyList<ComprobanteDocumentoDto>>.Fail(result.Error!));

        return Ok(ApiResponse<IReadOnlyList<ComprobanteDocumentoDto>>.Ok(result.Value!));
    }

    /// <summary>Adjunta un documento existente a un comprobante.</summary>
    [HttpPost("{comprobanteId:long}/documentos")]
    public async Task<IActionResult> AdjuntarDocumento(
        long comprobanteId, [FromBody] AdjuntarDocumentoDto dto, CancellationToken ct)
    {
        var result = await _docService.AdjuntarAsync(comprobanteId, dto, ct);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ComprobanteDocumentoDto>.Fail(result.Error!));

        return CreatedAtAction(
            nameof(GetDocumentos),
            new { comprobanteId },
            ApiResponse<ComprobanteDocumentoDto>.Ok(result.Value!, "Documento adjuntado."));
    }

    /// <summary>Elimina el vínculo entre un comprobante y un documento adjunto.</summary>
    [HttpDelete("{comprobanteId:long}/documentos/{docId:int}")]
    public async Task<IActionResult> RemoverDocumento(long comprobanteId, int docId, CancellationToken ct)
    {
        var result = await _docService.RemoverAsync(comprobanteId, docId, ct);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<bool>.Fail(result.Error!));

        return Ok(ApiResponse<bool>.Ok(true, "Documento removido del comprobante."));
    }
}
