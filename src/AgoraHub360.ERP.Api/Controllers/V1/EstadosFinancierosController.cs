namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contabilidad/estados-financieros")]
[Authorize]
public class EstadosFinancierosController : ControllerBase
{
    private readonly IEstadoFinancieroService _service;
    private readonly IExportService _exportService;
    private readonly IEmpresaService _empresaService;
    private readonly ILogger<EstadosFinancierosController> _logger;

    public EstadosFinancierosController(IEstadoFinancieroService service, IExportService exportService, IEmpresaService empresaService, ILogger<EstadosFinancierosController> logger)
    {
        _service = service;
        _exportService = exportService;
        _empresaService = empresaService;
        _logger = logger;
    }


    // ????????????????????????????????????????????
    // BALANCE GENERAL
    // ????????????????????????????????????????????

    [HttpGet("balance-general")]
    public async Task<IActionResult> GetBalanceGeneral([FromQuery] DateTime fechaCorte, CancellationToken ct)
    {
        var r = await _service.GetBalanceGeneralAsync(fechaCorte, ct);
        return r.IsSuccess
            ? Ok(ApiResponse<BalanceGeneralDto>.Ok(r.Value!))
            : BadRequest(ApiResponse<BalanceGeneralDto>.Fail(r.Error!));
    }

    [HttpGet("balance-general/export")]
    public async Task<IActionResult> ExportBalanceGeneralExcel([FromQuery] DateTime fechaCorte, [FromQuery] string format = "xlsx", CancellationToken ct = default)
    {
        format = (format ?? "xlsx").Trim().ToLowerInvariant();
        if (format != "xlsx" && format != "pdf")
            return BadRequest(ApiResponse<string>.Fail("El formato permitido es xlsx o pdf."));

        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (!int.TryParse(claim, out int empresaId)) return Unauthorized();

        _logger.LogInformation("Exportación de {Reporte} por usuario {Usuario} empresa {EmpresaId} formato {Formato} fecha de corte {FechaCorte}",
            "Balance General", User.Identity?.Name ?? "Desconocido", empresaId, format, fechaCorte.ToString("yyyy-MM-dd"));

        var r = await _service.GetBalanceGeneralAsync(fechaCorte, ct);
        if (!r.IsSuccess) return BadRequest(ApiResponse<string>.Fail(r.Error!));
        var data = r.Value!;

        var empresa = await _empresaService.GetByIdAsync(empresaId, ct);
        var nombreEmpresa = empresa.IsSuccess ? empresa.Value!.Nombre : "Empresa";

        byte[] bytes;
        string mime;
        string filename;

        if (format == "pdf")
        {
            bytes = _exportService.ExportarBalanceGeneralPdf(data, nombreEmpresa);
            mime = "application/pdf";
            filename = $"Balance_General_{data.FechaCorte:yyyyMMdd}.pdf";
        }
        else
        {
            bytes = _exportService.ExportarBalanceGeneralExcel(data, nombreEmpresa);
            mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            filename = $"Balance_General_{data.FechaCorte:yyyyMMdd}.xlsx";
        }

        return File(bytes, mime, filename);
    }

    // ????????????????????????????????????????????
    // ESTADO DE RESULTADOS
    // ????????????????????????????????????????????

    [HttpGet("estado-resultados")]
    public async Task<IActionResult> GetEstadoResultados([FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
    {
        var r = await _service.GetEstadoResultadosAsync(desde, hasta, ct);
        return r.IsSuccess
            ? Ok(ApiResponse<EstadoResultadosDto>.Ok(r.Value!))
            : BadRequest(ApiResponse<EstadoResultadosDto>.Fail(r.Error!));
    }

    [HttpGet("estado-resultados/excel")]
    public async Task<IActionResult> ExportEstadoResultadosExcel([FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        int.TryParse(claim ?? "0", out int empresaId);

        _logger.LogInformation("Exportación de {Reporte} por usuario {Usuario} empresa {EmpresaId} formato {Formato} desde {Desde} hasta {Hasta}",
            "Estado de Resultados", User.Identity?.Name ?? "Desconocido", empresaId, "xlsx", desde.ToString("yyyy-MM-dd"), hasta.ToString("yyyy-MM-dd"));

        var r = await _service.GetEstadoResultadosAsync(desde, hasta, ct);
        if (!r.IsSuccess) return BadRequest(ApiResponse<string>.Fail(r.Error!));
        var data = r.Value!;

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Estado de Resultados");

        ws.Cell(1, 1).Value = data.Empresa;
        ws.Range(1, 1, 1, 3).Merge();
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(2, 1).Value = "ESTADO DE RESULTADOS";
        ws.Range(2, 1, 2, 3).Merge();
        ws.Cell(2, 1).Style.Font.Bold = true;
        ws.Cell(2, 1).Style.Font.FontSize = 12;

        ws.Cell(3, 1).Value = $"Del {data.FechaDesde:dd/MM/yyyy} al {data.FechaHasta:dd/MM/yyyy} — Gestión {data.Gestion}";
        ws.Range(3, 1, 3, 3).Merge();

        int row = 5;
        SetHeader(ws, ref row, "Código", "Cuenta", "Monto");

        // INGRESOS
        row++;
        ws.Cell(row, 1).Value = "INGRESOS";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;
        WriteTreeToExcel(ws, data.Ingresos, ref row);
        WriteTotalRow(ws, ref row, "TOTAL INGRESOS", data.TotalIngresos);

        // COSTOS
        row++;
        ws.Cell(row, 1).Value = "COSTOS";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;
        WriteTreeToExcel(ws, data.Costos, ref row);
        WriteTotalRow(ws, ref row, "TOTAL COSTOS", data.TotalCostos);

        // UTILIDAD BRUTA
        row++;
        ws.Cell(row, 2).Value = "UTILIDAD BRUTA";
        ws.Cell(row, 2).Style.Font.Bold = true;
        ws.Cell(row, 2).Style.Font.FontSize = 11;
        ws.Cell(row, 3).Value = data.UtilidadBruta;
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 3).Style.Font.FontColor = data.UtilidadBruta >= 0 ? XLColor.DarkGreen : XLColor.Red;
        row += 2;

        // GASTOS
        ws.Cell(row, 1).Value = "GASTOS";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;
        WriteTreeToExcel(ws, data.Gastos, ref row);
        WriteTotalRow(ws, ref row, "TOTAL GASTOS", data.TotalGastos);

        // UTILIDAD NETA
        row += 2;
        ws.Cell(row, 2).Value = "UTILIDAD (PÉRDIDA) NETA";
        ws.Cell(row, 2).Style.Font.Bold = true;
        ws.Cell(row, 2).Style.Font.FontSize = 12;
        ws.Cell(row, 3).Value = data.UtilidadNeta;
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 3).Style.Font.FontSize = 12;
        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 3).Style.Font.FontColor = data.UtilidadNeta >= 0 ? XLColor.DarkGreen : XLColor.Red;
        ws.Cell(row, 3).Style.Border.TopBorder = XLBorderStyleValues.Double;
        ws.Cell(row, 3).Style.Border.BottomBorder = XLBorderStyleValues.Double;

        ws.Column(1).Width = 18;
        ws.Column(2).Width = 45;
        ws.Column(3).Width = 20;

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Estado_Resultados_{data.FechaDesde:yyyyMMdd}_{data.FechaHasta:yyyyMMdd}.xlsx");
    }

    // ????????????????????????????????????????????
    // SUMAS Y SALDOS
    // ????????????????????????????????????????????

    [HttpGet("sumas-saldos")]
    public async Task<IActionResult> GetSumasYSaldos([FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
    {
        var r = await _service.GetSumasYSaldosAsync(desde, hasta, ct);
        return r.IsSuccess
            ? Ok(ApiResponse<SumasYSaldosDto>.Ok(r.Value!))
            : BadRequest(ApiResponse<SumasYSaldosDto>.Fail(r.Error!));
    }

    [HttpGet("sumas-saldos/export")]
    public async Task<IActionResult> ExportSumasYSaldosExcel([FromQuery] DateTime desde, [FromQuery] DateTime hasta, [FromQuery] string format = "xlsx", CancellationToken ct = default)
    {
        format = (format ?? "xlsx").Trim().ToLowerInvariant();
        if (format != "xlsx")
            return BadRequest(ApiResponse<string>.Fail("El reporte Sumas y Saldos solo permite exportación en formato xlsx."));

        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (!int.TryParse(claim, out int empresaId)) return Unauthorized();

        _logger.LogInformation("Exportación de {Reporte} por usuario {Usuario} empresa {EmpresaId} formato {Formato} desde {Desde} hasta {Hasta}",
            "Sumas y Saldos", User.Identity?.Name ?? "Desconocido", empresaId, format, desde.ToString("yyyy-MM-dd"), hasta.ToString("yyyy-MM-dd"));

        var r = await _service.GetSumasYSaldosAsync(desde, hasta, ct);
        if (!r.IsSuccess) return BadRequest(ApiResponse<string>.Fail(r.Error!));
        var data = r.Value!;

        var empresa = await _empresaService.GetByIdAsync(empresaId, ct);
        var nombreEmpresa = empresa.IsSuccess ? empresa.Value!.Nombre : "Empresa";

        var bytes = _exportService.ExportarSumasYSaldosExcel(data, nombreEmpresa);

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Sumas_Saldos_{data.FechaDesde:yyyyMMdd}_{data.FechaHasta:yyyyMMdd}.xlsx");
    }

    // ????????????????????????????????????????????
    // LIBRO DIARIO
    // ????????????????????????????????????????????

    [HttpGet("libro-diario")]
    public async Task<IActionResult> GetLibroDiario([FromQuery] DateTime desde, [FromQuery] DateTime hasta, [FromQuery] string? estado, CancellationToken ct)
    {
        var r = await _service.GetLibroDiarioAsync(desde, hasta, estado, ct);
        return r.IsSuccess
            ? Ok(ApiResponse<LibroDiarioDto>.Ok(r.Value!))
            : BadRequest(ApiResponse<LibroDiarioDto>.Fail(r.Error!));
    }

    [HttpGet("libro-diario/export")]
    public async Task<IActionResult> ExportLibroDiarioExcel([FromQuery] DateTime desde, [FromQuery] DateTime hasta, [FromQuery] string? estado, [FromQuery] string format = "xlsx", CancellationToken ct = default)
    {
        format = (format ?? "xlsx").Trim().ToLowerInvariant();
        if (format != "xlsx" && format != "pdf")
            return BadRequest(ApiResponse<string>.Fail("El formato permitido es xlsx o pdf."));

        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (!int.TryParse(claim, out int empresaId)) return Unauthorized();

        _logger.LogInformation("Exportación de {Reporte} por usuario {Usuario} empresa {EmpresaId} formato {Formato} desde {Desde} hasta {Hasta}",
            "Libro Diario", User.Identity?.Name ?? "Desconocido", empresaId, format, desde.ToString("yyyy-MM-dd"), hasta.ToString("yyyy-MM-dd"));

        var r = await _service.GetLibroDiarioAsync(desde, hasta, estado, ct);
        if (!r.IsSuccess) return BadRequest(ApiResponse<string>.Fail(r.Error!));
        var data = r.Value!;

        var empresaInfo = await _empresaService.GetByIdAsync(empresaId, ct);
        var nombreEmpresa = empresaInfo.IsSuccess ? empresaInfo.Value!.Nombre : "Empresa";

        byte[] bytes;
        string mime;
        string filename;

        if (format == "pdf")
        {
            bytes = _exportService.ExportarLibroDiarioPdf(data, nombreEmpresa);
            mime = "application/pdf";
            filename = $"Libro_Diario_{data.FechaDesde:yyyyMMdd}_{data.FechaHasta:yyyyMMdd}.pdf";
        }
        else
        {
            bytes = _exportService.ExportarLibroDiarioExcel(data, nombreEmpresa);
            mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            filename = $"Libro_Diario_{data.FechaDesde:yyyyMMdd}_{data.FechaHasta:yyyyMMdd}.xlsx";
        }

        return File(bytes, mime, filename);
    }

    // ????????????????????????????????????????????
    // FLUJO DE EFECTIVO
    // ????????????????????????????????????????????

    [HttpGet("flujo-efectivo")]
    [ProducesResponseType(typeof(ApiResponse<FlujoDEfectivoDto>), 200)]
    public async Task<IActionResult> GetFlujoDEfectivo([FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (!int.TryParse(claim, out int empresaId)) return Unauthorized();

        var dto = await _service.GetFlujoDEfectivoAsync(empresaId, desde, hasta, ct);
        return Ok(ApiResponse<FlujoDEfectivoDto>.Ok(dto));
    }

    [HttpGet("flujo-efectivo/export")]
    public async Task<IActionResult> ExportFlujoEfectivo([FromQuery] DateTime desde, [FromQuery] DateTime hasta, [FromQuery] string format = "xlsx", CancellationToken ct = default)
    {
        format = (format ?? "xlsx").Trim().ToLowerInvariant();
        if (format != "xlsx" && format != "pdf")
            return BadRequest(ApiResponse<string>.Fail("El formato permitido es xlsx o pdf."));

        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (!int.TryParse(claim, out int empresaId)) return Unauthorized();

        _logger.LogInformation("Exportación de {Reporte} por usuario {Usuario} empresa {EmpresaId} formato {Formato} desde {Desde} hasta {Hasta}",
            "Flujo de Efectivo", User.Identity?.Name ?? "Desconocido", empresaId, format, desde.ToString("yyyy-MM-dd"), hasta.ToString("yyyy-MM-dd"));

        var dto = await _service.GetFlujoDEfectivoAsync(empresaId, desde, hasta, ct);

        var empresaInfo = await _empresaService.GetByIdAsync(empresaId, ct);
        var nombreEmpresa = empresaInfo.IsSuccess ? empresaInfo.Value!.Nombre : "Empresa";

        byte[] bytes;
        string mime;
        string filename;

        if (format == "pdf")
        {
            bytes = _exportService.ExportarFlujoDEfectivoPdf(dto, nombreEmpresa);
            mime = "application/pdf";
            filename = $"FlujoEfectivo_{desde:yyyyMMdd}.pdf";
        }
        else
        {
            bytes = _exportService.ExportarFlujoDEfectivoExcel(dto, nombreEmpresa);
            mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            filename = $"FlujoEfectivo_{desde:yyyyMMdd}.xlsx";
        }

        return File(bytes, mime, filename);
    }

    // ????????????????????????????????????????????
    // LIBRO MAYOR
    // ????????????????????????????????????????????

    [HttpGet("libro-mayor")]
    [ProducesResponseType(typeof(ApiResponse<LibroMayorDto>), 200)]
    public async Task<IActionResult> GetLibroMayor([FromQuery] int cuentaContableId, [FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (!int.TryParse(claim, out int empresaId)) return Unauthorized();

        try
        {
            var dto = await _service.GetLibroMayorAsync(empresaId, cuentaContableId, desde, hasta, ct);
            return Ok(ApiResponse<LibroMayorDto>.Ok(dto));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<LibroMayorDto>.Fail(ex.Message));
        }
    }

    [HttpGet("libro-mayor/export")]
    public async Task<IActionResult> ExportLibroMayor([FromQuery] int cuentaContableId, [FromQuery] DateTime desde, [FromQuery] DateTime hasta, [FromQuery] string format = "xlsx", CancellationToken ct = default)
    {
        format = (format ?? "xlsx").Trim().ToLowerInvariant();
        if (format != "xlsx" && format != "pdf")
            return BadRequest(ApiResponse<string>.Fail("El formato permitido es xlsx o pdf."));

        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (!int.TryParse(claim, out int empresaId)) return Unauthorized();

        _logger.LogInformation("Exportación de {Reporte} por usuario {Usuario} empresa {EmpresaId} formato {Formato} desde {Desde} hasta {Hasta}",
            "Libro Mayor", User.Identity?.Name ?? "Desconocido", empresaId, format, desde.ToString("yyyy-MM-dd"), hasta.ToString("yyyy-MM-dd"));

        try
        {
            var dto = await _service.GetLibroMayorAsync(empresaId, cuentaContableId, desde, hasta, ct);

            var empresaInfo = await _empresaService.GetByIdAsync(empresaId, ct);
            var nombreEmpresa = empresaInfo.IsSuccess ? empresaInfo.Value!.Nombre : "Empresa";

            byte[] bytes;
            string mime;
            string filename;

            if (format == "pdf")
            {
                bytes = _exportService.ExportarLibroMayorPdf(dto, nombreEmpresa);
                mime = "application/pdf";
                filename = $"LibroMayor_{desde:yyyyMMdd}.pdf";
            }
            else
            {
                bytes = _exportService.ExportarLibroMayorExcel(dto, nombreEmpresa);
                mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                filename = $"LibroMayor_{desde:yyyyMMdd}.xlsx";
            }

            return File(bytes, mime, filename);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }
    // ????????????????????????????????????????????

    private static void SetHeader(IXLWorksheet ws, ref int row, params string[] cols)
    {
        for (int c = 0; c < cols.Length; c++)
        {
            var cell = ws.Cell(row, c + 1);
            cell.Value = cols[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(79, 129, 189);
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
        row++;
    }

    private static void WriteTreeToExcel(IXLWorksheet ws, List<BalanceGrupoDto> items, ref int row)
    {
        foreach (var item in items)
        {
            WriteNodeToExcel(ws, item, ref row);
        }
    }

    private static void WriteNodeToExcel(IXLWorksheet ws, BalanceGrupoDto node, ref int row)
    {
        ws.Cell(row, 1).Value = node.Codigo;
        ws.Cell(row, 2).Value = node.Nombre;
        ws.Cell(row, 2).Style.Alignment.Indent = Math.Max(0, node.Nivel - 1);
        ws.Cell(row, 3).Value = node.Saldo;
        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        if (node.EsAgrupador)
        {
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 3).Style.Font.Bold = true;
        }
        row++;
        foreach (var child in node.SubCuentas)
            WriteNodeToExcel(ws, child, ref row);
    }

    private static void WriteTotalRow(IXLWorksheet ws, ref int row, string label, decimal amount)
    {
        ws.Cell(row, 2).Value = label;
        ws.Cell(row, 2).Style.Font.Bold = true;
        ws.Cell(row, 3).Value = amount;
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 3).Style.Border.TopBorder = XLBorderStyleValues.Double;
        row++;
    }
}
