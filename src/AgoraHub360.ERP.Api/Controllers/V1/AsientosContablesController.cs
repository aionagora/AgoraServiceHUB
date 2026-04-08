namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contabilidad/asientos")]
[Authorize]
public class AsientosContablesController : ControllerBase
{
    private readonly IAsientoContableService _service;
    private readonly IComprobanteDocumentoService _docService;

    public AsientosContablesController(
        IAsientoContableService service,
        IComprobanteDocumentoService docService)
    {
        _service    = service;
        _docService = docService;
        // Configurar EPPlus para uso no comercial
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
            // Obtener los datos filtrados
            var result = await _service.GetAllAsync(desde, hasta, estado, tipoComprobanteId, search, ct);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<string>.Fail(result.Error!));

            var asientos = result.Value!;

            // Crear el archivo Excel
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Comprobantes Contables");

            // Configurar encabezados
            worksheet.Cells[1, 1].Value = "Tipo";
            worksheet.Cells[1, 2].Value = "Número";
            worksheet.Cells[1, 3].Value = "Fecha";
            worksheet.Cells[1, 4].Value = "Gestión";
            worksheet.Cells[1, 5].Value = "Concepto";
            worksheet.Cells[1, 6].Value = "Glosa";
            worksheet.Cells[1, 7].Value = "Tipo Registro";
            worksheet.Cells[1, 8].Value = "T/C Moneda";
            worksheet.Cells[1, 9].Value = "Valor T/C";
            worksheet.Cells[1, 10].Value = "Tipo Pago";
            worksheet.Cells[1, 11].Value = "Nro Documento";
            worksheet.Cells[1, 12].Value = "Total Debe";
            worksheet.Cells[1, 13].Value = "Total Haber";
            worksheet.Cells[1, 14].Value = "Estado";
            worksheet.Cells[1, 15].Value = "Registrado Por";

            // Estilo de encabezados
            using (var range = worksheet.Cells[1, 1, 1, 15])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
                range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }

            // Llenar datos
            int row = 2;
            foreach (var asiento in asientos)
            {
                worksheet.Cells[row, 1].Value = asiento.TipoComprobanteCodigo;
                worksheet.Cells[row, 2].Value = asiento.Numero;
                worksheet.Cells[row, 3].Value = asiento.Fecha.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 4].Value = asiento.Gestion;
                worksheet.Cells[row, 5].Value = asiento.Concepto ?? "";
                worksheet.Cells[row, 6].Value = asiento.Glosa;
                worksheet.Cells[row, 7].Value = asiento.TipoRegistro;
                worksheet.Cells[row, 8].Value = asiento.TipoCambioMoneda ?? "";
                worksheet.Cells[row, 9].Value = asiento.ValorTipoCambio?.ToString("N2") ?? "";
                worksheet.Cells[row, 10].Value = asiento.TipoPagoNombre ?? "";
                worksheet.Cells[row, 11].Value = asiento.NumeroDocumentoPago ?? "";
                worksheet.Cells[row, 12].Value = asiento.TotalDebe;
                worksheet.Cells[row, 13].Value = asiento.TotalHaber;
                worksheet.Cells[row, 14].Value = asiento.Estado;
                worksheet.Cells[row, 15].Value = asiento.RegistradoPorNombre ?? "";

                // Formato de moneda para columnas de monto
                worksheet.Cells[row, 12].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 13].Style.Numberformat.Format = "#,##0.00";

                row++;
            }

            // Auto ajustar columnas
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Agregar totales
            if (asientos.Any())
            {
                worksheet.Cells[row, 11].Value = "TOTALES:";
                worksheet.Cells[row, 11].Style.Font.Bold = true;
                worksheet.Cells[row, 12].Formula = $"SUM(L2:L{row - 1})";
                worksheet.Cells[row, 13].Formula = $"SUM(M2:M{row - 1})";
                worksheet.Cells[row, 12].Style.Font.Bold = true;
                worksheet.Cells[row, 13].Style.Font.Bold = true;
                worksheet.Cells[row, 12].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 13].Style.Numberformat.Format = "#,##0.00";

                // Borde superior para la fila de totales
                using (var range = worksheet.Cells[row, 11, row, 13])
                {
                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;
                }
            }

            // Generar el archivo
            var fileBytes = package.GetAsByteArray();
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

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add($"Comprobante {a.Numero}");

            // ?? Cabecera ??
            ws.Cells[1, 1].Value = "COMPROBANTE CONTABLE";
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Size = 14;

            ws.Cells[3, 1].Value = "Tipo:"; ws.Cells[3, 2].Value = $"{a.TipoComprobanteCodigo} - {a.TipoComprobanteNombre}";
            ws.Cells[4, 1].Value = "Número:"; ws.Cells[4, 2].Value = a.Numero;
            ws.Cells[5, 1].Value = "Fecha:"; ws.Cells[5, 2].Value = a.Fecha.ToString("dd/MM/yyyy");
            ws.Cells[6, 1].Value = "Gestión:"; ws.Cells[6, 2].Value = a.Gestion;
            ws.Cells[7, 1].Value = "Estado:"; ws.Cells[7, 2].Value = a.Estado;
            ws.Cells[8, 1].Value = "Concepto:"; ws.Cells[8, 2].Value = a.Concepto ?? "";
            ws.Cells[9, 1].Value = "Glosa:"; ws.Cells[9, 2].Value = a.Glosa;
            ws.Cells[10, 1].Value = "Registrado por:"; ws.Cells[10, 2].Value = a.RegistradoPorNombre ?? "";

            if (a.TipoCambioMoneda != null)
            {
                ws.Cells[11, 1].Value = "Tipo Cambio:";
                ws.Cells[11, 2].Value = $"{a.TipoCambioMoneda} {a.ValorTipoCambio:N2}";
            }
            if (!string.IsNullOrEmpty(a.TipoPagoCodigo) && a.TipoPagoCodigo != "S/D")
            {
                ws.Cells[12, 1].Value = "Documento Pago:";
                ws.Cells[12, 2].Value = $"{a.TipoPagoNombre} {a.NumeroDocumentoPago}";
            }

            using (var rng = ws.Cells[3, 1, 12, 1])
            {
                rng.Style.Font.Bold = true;
            }

            // ?? Detalle ??
            int headerRow = 14;
            ws.Cells[headerRow, 1].Value = "#";
            ws.Cells[headerRow, 2].Value = "Código";
            ws.Cells[headerRow, 3].Value = "Nombre Cuenta";
            ws.Cells[headerRow, 4].Value = "Glosa Detalle";
            ws.Cells[headerRow, 5].Value = "Centro Costo";
            ws.Cells[headerRow, 6].Value = "Debe";
            ws.Cells[headerRow, 7].Value = "Haber";

            using (var rng = ws.Cells[headerRow, 1, headerRow, 7])
            {
                rng.Style.Font.Bold = true;
                rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
                rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
            }

            int row = headerRow + 1;
            foreach (var l in a.Lineas)
            {
                ws.Cells[row, 1].Value = l.NumeroLinea;
                ws.Cells[row, 2].Value = l.CuentaCodigo;
                ws.Cells[row, 3].Value = l.CuentaNombre;
                ws.Cells[row, 4].Value = l.Glosa ?? "";
                ws.Cells[row, 5].Value = l.CentroCostoCodigo ?? "";
                ws.Cells[row, 6].Value = l.Debe;
                ws.Cells[row, 7].Value = l.Haber;
                ws.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";
                row++;
            }

            // Totales
            ws.Cells[row, 5].Value = "TOTALES:";
            ws.Cells[row, 5].Style.Font.Bold = true;
            ws.Cells[row, 6].Value = a.TotalDebe;
            ws.Cells[row, 7].Value = a.TotalHaber;
            ws.Cells[row, 6].Style.Font.Bold = true;
            ws.Cells[row, 7].Style.Font.Bold = true;
            ws.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";
            using (var rng = ws.Cells[row, 5, row, 7])
            {
                rng.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            var fileBytes = package.GetAsByteArray();
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

            var asientos = result.Value!;

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Detalle Plano");

            // Encabezados — una fila por línea de detalle
            var headers = new[]
            {
                "Tipo","Numero","Fecha","Gestion","Concepto","Glosa","TipoRegistro",
                "Estado","TipoCambioMoneda","ValorTipoCambio","TipoPago","NroDocPago",
                "TotalDebe","TotalHaber","RegistradoPor",
                "LineaNro","CuentaCodigo","CuentaNombre","LineaDebe","LineaHaber",
                "LineaGlosa","CentroCosto"
            };
            for (int c = 0; c < headers.Length; c++)
                ws.Cells[1, c + 1].Value = headers[c];

            using (var rng = ws.Cells[1, 1, 1, headers.Length])
            {
                rng.Style.Font.Bold = true;
                rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(31, 73, 125));
                rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
            }

            int row = 2;
            var altRow = false;
            foreach (var a in asientos)
            {
                var lineas = a.Lineas.Count > 0 ? a.Lineas : new List<AsientoContableLineaDto> { new() };
                foreach (var l in lineas)
                {
                    ws.Cells[row, 1].Value  = a.TipoComprobanteCodigo;
                    ws.Cells[row, 2].Value  = a.Numero;
                    ws.Cells[row, 3].Value  = a.Fecha.ToString("dd/MM/yyyy");
                    ws.Cells[row, 4].Value  = a.Gestion;
                    ws.Cells[row, 5].Value  = a.Concepto ?? "";
                    ws.Cells[row, 6].Value  = a.Glosa;
                    ws.Cells[row, 7].Value  = a.TipoRegistro;
                    ws.Cells[row, 8].Value  = a.Estado;
                    ws.Cells[row, 9].Value  = a.TipoCambioMoneda ?? "";
                    ws.Cells[row, 10].Value = a.ValorTipoCambio?.ToString("N2") ?? "";
                    ws.Cells[row, 11].Value = a.TipoPagoNombre ?? "";
                    ws.Cells[row, 12].Value = a.NumeroDocumentoPago ?? "";
                    ws.Cells[row, 13].Value = a.TotalDebe;
                    ws.Cells[row, 14].Value = a.TotalHaber;
                    ws.Cells[row, 15].Value = a.RegistradoPorNombre ?? "";
                    ws.Cells[row, 16].Value = l.NumeroLinea > 0 ? l.NumeroLinea : (object)"";
                    ws.Cells[row, 17].Value = l.CuentaCodigo;
                    ws.Cells[row, 18].Value = l.CuentaNombre;
                    ws.Cells[row, 19].Value = l.Debe > 0 ? l.Debe : (object)"";
                    ws.Cells[row, 20].Value = l.Haber > 0 ? l.Haber : (object)"";
                    ws.Cells[row, 21].Value = l.Glosa ?? "";
                    ws.Cells[row, 22].Value = l.CentroCostoCodigo ?? "";

                    ws.Cells[row, 13].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[row, 14].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[row, 19].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[row, 20].Style.Numberformat.Format = "#,##0.00";

                    // Filas alternadas para mejor legibilidad
                    if (altRow)
                    {
                        using var rng = ws.Cells[row, 1, row, headers.Length];
                        rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(242, 242, 242));
                    }
                    row++;
                }
                altRow = !altRow;
            }

            if (row > 2)
                ws.Cells[ws.Dimension.Address].AutoFitColumns();

            var fileBytes = package.GetAsByteArray();
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
