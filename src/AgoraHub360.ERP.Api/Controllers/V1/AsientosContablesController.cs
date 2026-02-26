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

    public AsientosContablesController(IAsientoContableService service)
    {
        _service = service;
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
                worksheet.Cells[row, 15].Value = asiento.RegistradoPor ?? "";

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
}
