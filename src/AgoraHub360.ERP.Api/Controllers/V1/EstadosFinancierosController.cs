namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contabilidad/estados-financieros")]
[Authorize]
public class EstadosFinancierosController : ControllerBase
{
    private readonly IEstadoFinancieroService _service;
    private readonly IExportService _exportService;
    private readonly IEmpresaService _empresaService;

    public EstadosFinancierosController(IEstadoFinancieroService service, IExportService exportService, IEmpresaService empresaService)
    {
        _service = service;
        _exportService = exportService;
        _empresaService = empresaService;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (!int.TryParse(claim, out int empresaId)) return Unauthorized();

        var r = await _service.GetBalanceGeneralAsync(fechaCorte, ct);
        if (!r.IsSuccess) return BadRequest(ApiResponse<string>.Fail(r.Error!));
        var data = r.Value!;

        var empresa = await _empresaService.GetByIdAsync(empresaId, ct);
        var nombreEmpresa = empresa.IsSuccess ? empresa.Value!.Nombre : "Empresa";

        byte[] bytes;
        string mime;
        string filename;

        if (format.ToLower() == "pdf")
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
        var r = await _service.GetEstadoResultadosAsync(desde, hasta, ct);
        if (!r.IsSuccess) return BadRequest(ApiResponse<string>.Fail(r.Error!));
        var data = r.Value!;

        using var pkg = new ExcelPackage();
        var ws = pkg.Workbook.Worksheets.Add("Estado de Resultados");

        ws.Cells[1, 1].Value = data.Empresa;
        ws.Cells[1, 1, 1, 3].Merge = true;
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.Font.Size = 14;

        ws.Cells[2, 1].Value = "ESTADO DE RESULTADOS";
        ws.Cells[2, 1, 2, 3].Merge = true;
        ws.Cells[2, 1].Style.Font.Bold = true;
        ws.Cells[2, 1].Style.Font.Size = 12;

        ws.Cells[3, 1].Value = $"Del {data.FechaDesde:dd/MM/yyyy} al {data.FechaHasta:dd/MM/yyyy} — Gestión {data.Gestion}";
        ws.Cells[3, 1, 3, 3].Merge = true;

        int row = 5;
        SetHeader(ws, ref row, "Código", "Cuenta", "Monto");

        // INGRESOS
        row++;
        ws.Cells[row, 1].Value = "INGRESOS";
        ws.Cells[row, 1].Style.Font.Bold = true;
        row++;
        WriteTreeToExcel(ws, data.Ingresos, ref row);
        WriteTotalRow(ws, ref row, "TOTAL INGRESOS", data.TotalIngresos);

        // COSTOS
        row++;
        ws.Cells[row, 1].Value = "COSTOS";
        ws.Cells[row, 1].Style.Font.Bold = true;
        row++;
        WriteTreeToExcel(ws, data.Costos, ref row);
        WriteTotalRow(ws, ref row, "TOTAL COSTOS", data.TotalCostos);

        // UTILIDAD BRUTA
        row++;
        ws.Cells[row, 2].Value = "UTILIDAD BRUTA";
        ws.Cells[row, 2].Style.Font.Bold = true;
        ws.Cells[row, 2].Style.Font.Size = 11;
        ws.Cells[row, 3].Value = data.UtilidadBruta;
        ws.Cells[row, 3].Style.Font.Bold = true;
        ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
        ws.Cells[row, 3].Style.Font.Color.SetColor(data.UtilidadBruta >= 0 ? Color.DarkGreen : Color.Red);
        row += 2;

        // GASTOS
        ws.Cells[row, 1].Value = "GASTOS";
        ws.Cells[row, 1].Style.Font.Bold = true;
        row++;
        WriteTreeToExcel(ws, data.Gastos, ref row);
        WriteTotalRow(ws, ref row, "TOTAL GASTOS", data.TotalGastos);

        // UTILIDAD NETA
        row += 2;
        ws.Cells[row, 2].Value = "UTILIDAD (PÉRDIDA) NETA";
        ws.Cells[row, 2].Style.Font.Bold = true;
        ws.Cells[row, 2].Style.Font.Size = 12;
        ws.Cells[row, 3].Value = data.UtilidadNeta;
        ws.Cells[row, 3].Style.Font.Bold = true;
        ws.Cells[row, 3].Style.Font.Size = 12;
        ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
        ws.Cells[row, 3].Style.Font.Color.SetColor(data.UtilidadNeta >= 0 ? Color.DarkGreen : Color.Red);
        ws.Cells[row, 3].Style.Border.Top.Style = ExcelBorderStyle.Double;
        ws.Cells[row, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Double;

        ws.Column(1).Width = 18;
        ws.Column(2).Width = 45;
        ws.Column(3).Width = 20;

        return File(pkg.GetAsByteArray(),
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
        if (format.ToLower() == "pdf") return BadRequest(ApiResponse<string>.Fail("Formato PDF no implementado para Sumas y Saldos."));

        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (!int.TryParse(claim, out int empresaId)) return Unauthorized();

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
        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (!int.TryParse(claim, out int empresaId)) return Unauthorized();

        var r = await _service.GetLibroDiarioAsync(desde, hasta, estado, ct);
        if (!r.IsSuccess) return BadRequest(ApiResponse<string>.Fail(r.Error!));
        var data = r.Value!;

        var empresaInfo = await _empresaService.GetByIdAsync(empresaId, ct);
        var nombreEmpresa = empresaInfo.IsSuccess ? empresaInfo.Value!.Nombre : "Empresa";

        byte[] bytes;
        string mime;
        string filename;

        if (format.ToLower() == "pdf")
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
    public async Task<IActionResult> ExportFlujoDEfectivoExcel([FromQuery] DateTime desde, [FromQuery] DateTime hasta, [FromQuery] string format = "xlsx", CancellationToken ct = default)
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (!int.TryParse(claim, out int empresaId)) return Unauthorized();

        var dto = await _service.GetFlujoDEfectivoAsync(empresaId, desde, hasta, ct);

        var empresaInfo = await _empresaService.GetByIdAsync(empresaId, ct);
        var nombreEmpresa = empresaInfo.IsSuccess ? empresaInfo.Value!.Nombre : "Empresa";

        byte[] bytes;
        string mime;
        string filename;

        if (format.ToLower() == "pdf")
        {
            bytes = _exportService.ExportarFlujoDEfectivoPdf(dto, nombreEmpresa);
            mime = "application/pdf";
            filename = $"Flujo_Efectivo_{dto.Desde:yyyyMMdd}_{dto.Hasta:yyyyMMdd}.pdf";
        }
        else
        {
            bytes = _exportService.ExportarFlujoDEfectivoExcel(dto, nombreEmpresa);
            mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            filename = $"Flujo_Efectivo_{dto.Desde:yyyyMMdd}_{dto.Hasta:yyyyMMdd}.xlsx";
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
    public async Task<IActionResult> ExportLibroMayorExcel([FromQuery] int cuentaContableId, [FromQuery] DateTime desde, [FromQuery] DateTime hasta, [FromQuery] string format = "xlsx", CancellationToken ct = default)
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value;
        if (!int.TryParse(claim, out int empresaId)) return Unauthorized();

        try
        {
            var dto = await _service.GetLibroMayorAsync(empresaId, cuentaContableId, desde, hasta, ct);

            var empresaInfo = await _empresaService.GetByIdAsync(empresaId, ct);
            var nombreEmpresa = empresaInfo.IsSuccess ? empresaInfo.Value!.Nombre : "Empresa";

            byte[] bytes;
            string mime;
            string filename;

            if (format.ToLower() == "pdf")
            {
                bytes = _exportService.ExportarLibroMayorPdf(dto, nombreEmpresa);
                mime = "application/pdf";
                filename = $"Libro_Mayor_{dto.CodigoCuenta}_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.pdf";
            }
            else
            {
                bytes = _exportService.ExportarLibroMayorExcel(dto, nombreEmpresa);
                mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                filename = $"Libro_Mayor_{dto.CodigoCuenta}_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.xlsx";
            }

            return File(bytes, mime, filename);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }
    // ????????????????????????????????????????????

    private static void SetHeader(ExcelWorksheet ws, ref int row, params string[] cols)
    {
        for (int c = 0; c < cols.Length; c++)
        {
            ws.Cells[row, c + 1].Value = cols[c];
            ws.Cells[row, c + 1].Style.Font.Bold = true;
            ws.Cells[row, c + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[row, c + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
            ws.Cells[row, c + 1].Style.Font.Color.SetColor(Color.White);
            ws.Cells[row, c + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }
        row++;
    }

    private static void WriteTreeToExcel(ExcelWorksheet ws, List<BalanceGrupoDto> items, ref int row)
    {
        foreach (var item in items)
        {
            WriteNodeToExcel(ws, item, ref row);
        }
    }

    private static void WriteNodeToExcel(ExcelWorksheet ws, BalanceGrupoDto node, ref int row)
    {
        ws.Cells[row, 1].Value = node.Codigo;
        ws.Cells[row, 2].Value = node.Nombre;
        ws.Cells[row, 2].Style.Indent = Math.Max(0, node.Nivel - 1);
        ws.Cells[row, 3].Value = node.Saldo;
        ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
        if (node.EsAgrupador)
        {
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 2].Style.Font.Bold = true;
            ws.Cells[row, 3].Style.Font.Bold = true;
        }
        row++;
        foreach (var child in node.SubCuentas)
            WriteNodeToExcel(ws, child, ref row);
    }

    private static void WriteTotalRow(ExcelWorksheet ws, ref int row, string label, decimal amount)
    {
        ws.Cells[row, 2].Value = label;
        ws.Cells[row, 2].Style.Font.Bold = true;
        ws.Cells[row, 3].Value = amount;
        ws.Cells[row, 3].Style.Font.Bold = true;
        ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
        ws.Cells[row, 3].Style.Border.Top.Style = ExcelBorderStyle.Double;
        row++;
    }
}
