namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;
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
    private readonly IAsientoImportService _importService;
    private readonly ICurrentUserService _currentUser;

    public AsientosContablesController(
        IAsientoContableService service,
        IComprobanteDocumentoService docService,
        IAsientoExportService exportService,
        IAsientoImportService importService,
        ICurrentUserService currentUser)
    {
        _service       = service;
        _docService    = docService;
        _exportService = exportService;
        _importService = importService;
        _currentUser   = currentUser;
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

    [HttpGet("exportar")]
    public async Task<IActionResult> ExportarAsync(
        [FromQuery] string formato = "excel",
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] string? estado = null,
        [FromQuery] int? tipoComprobanteId = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _service.GetAllAsync(desde, hasta, estado, tipoComprobanteId, search, ct);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<string>.Fail(result.Error!));

            var asientos = result.Value!;

            return formato.ToLower() switch
            {
                "excel" => File(
                    await _exportService.ExportarExcelAsync(asientos, ct),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"asientos_{DateTime.Today:yyyyMMdd}.xlsx"),

                "csv" => File(
                    System.Text.Encoding.UTF8.GetBytes(await _exportService.GenerarCsvAsync(asientos)),
                    "text/csv",
                    $"asientos_{DateTime.Today:yyyyMMdd}.csv"),

                "json" => File(
                    System.Text.Encoding.UTF8.GetBytes(await _exportService.GenerarJsonAsync(asientos)),
                    "application/json",
                    $"asientos_{DateTime.Today:yyyyMMdd}.json"),

                "xml" => File(
                    System.Text.Encoding.UTF8.GetBytes(await _exportService.GenerarXmlAsync(asientos)),
                    "application/xml",
                    $"asientos_{DateTime.Today:yyyyMMdd}.xml"),

                _ => BadRequest("Formato no soportado. Use: excel, csv, json, xml")
            };
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error al generar exportación: {ex.Message}"));
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

    // ── Importación masiva ──────────────────────────────────────────────

    /// <summary>
    /// Importa asientos contables desde un archivo Excel.
    /// Con <paramref name="soloValidar"/> = true solo valida sin persistir.
    /// </summary>
    [HttpPost("importar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportarAsientosAsync(
        IFormFile archivo,
        [FromQuery] bool soloValidar = false,
        [FromQuery] bool contabilizarInmediatamente = false,
        CancellationToken ct = default)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(ApiResponse<string>.Fail("Debe adjuntar un archivo."));

        if (archivo.Length > 5 * 1024 * 1024)
            return BadRequest(ApiResponse<string>.Fail("El archivo no puede superar 5 MB."));

        var empresaId = _currentUser.EmpresaId;
        if (empresaId is null or 0)
            return BadRequest(ApiResponse<string>.Fail("No se pudo determinar la empresa del usuario."));

        await using var stream = archivo.OpenReadStream();

        if (soloValidar)
        {
            var validacion = await _importService.ValidarArchivoAsync(stream, empresaId.Value, ct);
            return Ok(ApiResponse<ImportValidacionDto>.Ok(validacion));
        }

        var resultado = await _importService.ImportarAsync(stream, empresaId.Value, contabilizarInmediatamente, ct);
        return Ok(ApiResponse<ImportResultDto>.Ok(resultado));
    }

    /// <summary>
    /// Descarga una plantilla Excel vacía con los encabezados esperados para la importación masiva.
    /// </summary>
    [HttpGet("plantilla-importacion")]
    public IActionResult DescargarPlantillaImportacion()
    {
        var archivo = _importService.GenerarPlantillaImportacion();
        return File(
            archivo,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Plantilla_Importacion_Asientos.xlsx");
    }
}
