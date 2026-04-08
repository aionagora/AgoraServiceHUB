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

    public EstadosFinancierosController(IEstadoFinancieroService service)
    {
        _service = service;
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

    [HttpGet("balance-general/excel")]
    public async Task<IActionResult> ExportBalanceGeneralExcel([FromQuery] DateTime fechaCorte, CancellationToken ct)
    {
        var r = await _service.GetBalanceGeneralAsync(fechaCorte, ct);
        if (!r.IsSuccess) return BadRequest(ApiResponse<string>.Fail(r.Error!));
        var data = r.Value!;

        using var pkg = new ExcelPackage();
        var ws = pkg.Workbook.Worksheets.Add("Balance General");

        // Title
        ws.Cells[1, 1].Value = data.Empresa;
        ws.Cells[1, 1, 1, 3].Merge = true;
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.Font.Size = 14;

        ws.Cells[2, 1].Value = "BALANCE GENERAL";
        ws.Cells[2, 1, 2, 3].Merge = true;
        ws.Cells[2, 1].Style.Font.Bold = true;
        ws.Cells[2, 1].Style.Font.Size = 12;

        ws.Cells[3, 1].Value = $"Al {data.FechaCorte:dd/MM/yyyy} — Gestión {data.Gestion}";
        ws.Cells[3, 1, 3, 3].Merge = true;

        int row = 5;
        SetHeader(ws, ref row, "Código", "Cuenta", "Saldo");

        // ACTIVOS
        row++;
        ws.Cells[row, 1].Value = "ACTIVOS";
        ws.Cells[row, 1].Style.Font.Bold = true;
        ws.Cells[row, 1].Style.Font.Size = 11;
        row++;
        WriteTreeToExcel(ws, data.Activos, ref row);
        ws.Cells[row, 2].Value = "TOTAL ACTIVOS";
        ws.Cells[row, 2].Style.Font.Bold = true;
        ws.Cells[row, 3].Value = data.TotalActivos;
        ws.Cells[row, 3].Style.Font.Bold = true;
        ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
        ws.Cells[row, 3].Style.Border.Top.Style = ExcelBorderStyle.Double;
        row += 2;

        // PASIVOS
        ws.Cells[row, 1].Value = "PASIVOS";
        ws.Cells[row, 1].Style.Font.Bold = true;
        ws.Cells[row, 1].Style.Font.Size = 11;
        row++;
        WriteTreeToExcel(ws, data.Pasivos, ref row);
        ws.Cells[row, 2].Value = "TOTAL PASIVOS";
        ws.Cells[row, 2].Style.Font.Bold = true;
        ws.Cells[row, 3].Value = data.TotalPasivos;
        ws.Cells[row, 3].Style.Font.Bold = true;
        ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
        ws.Cells[row, 3].Style.Border.Top.Style = ExcelBorderStyle.Double;
        row += 2;

        // PATRIMONIO
        ws.Cells[row, 1].Value = "PATRIMONIO";
        ws.Cells[row, 1].Style.Font.Bold = true;
        ws.Cells[row, 1].Style.Font.Size = 11;
        row++;
        WriteTreeToExcel(ws, data.Patrimonio, ref row);
        ws.Cells[row, 2].Value = "TOTAL PATRIMONIO";
        ws.Cells[row, 2].Style.Font.Bold = true;
        ws.Cells[row, 3].Value = data.TotalPatrimonio;
        ws.Cells[row, 3].Style.Font.Bold = true;
        ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
        ws.Cells[row, 3].Style.Border.Top.Style = ExcelBorderStyle.Double;
        row += 2;

        // TOTAL PASIVO + PATRIMONIO
        ws.Cells[row, 2].Value = "TOTAL PASIVO + PATRIMONIO";
        ws.Cells[row, 2].Style.Font.Bold = true;
        ws.Cells[row, 3].Value = data.TotalPasivoPatrimonio;
        ws.Cells[row, 3].Style.Font.Bold = true;
        ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
        ws.Cells[row, 3].Style.Border.Top.Style = ExcelBorderStyle.Double;
        ws.Cells[row, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Double;

        ws.Column(1).Width = 18;
        ws.Column(2).Width = 45;
        ws.Column(3).Width = 20;

        return File(pkg.GetAsByteArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Balance_General_{data.FechaCorte:yyyyMMdd}.xlsx");
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

    [HttpGet("sumas-saldos/excel")]
    public async Task<IActionResult> ExportSumasYSaldosExcel([FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
    {
        var r = await _service.GetSumasYSaldosAsync(desde, hasta, ct);
        if (!r.IsSuccess) return BadRequest(ApiResponse<string>.Fail(r.Error!));
        var data = r.Value!;

        using var pkg = new ExcelPackage();
        var ws = pkg.Workbook.Worksheets.Add("Sumas y Saldos");

        ws.Cells[1, 1].Value = data.Empresa;
        ws.Cells[1, 1, 1, 6].Merge = true;
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.Font.Size = 14;

        ws.Cells[2, 1].Value = "BALANCE DE COMPROBACIÓN — SUMAS Y SALDOS";
        ws.Cells[2, 1, 2, 6].Merge = true;
        ws.Cells[2, 1].Style.Font.Bold = true;
        ws.Cells[2, 1].Style.Font.Size = 12;

        ws.Cells[3, 1].Value = $"Del {data.FechaDesde:dd/MM/yyyy} al {data.FechaHasta:dd/MM/yyyy} — Gestión {data.Gestion}";
        ws.Cells[3, 1, 3, 6].Merge = true;

        int row = 5;
        // Sub-headers
        ws.Cells[row, 3].Value = "SUMAS";
        ws.Cells[row, 3, row, 4].Merge = true;
        ws.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells[row, 3].Style.Font.Bold = true;
        ws.Cells[row, 5].Value = "SALDOS";
        ws.Cells[row, 5, row, 6].Merge = true;
        ws.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells[row, 5].Style.Font.Bold = true;
        row++;

        string[] headers = ["Código", "Cuenta", "Debe", "Haber", "Deudor", "Acreedor"];
        for (int c = 0; c < headers.Length; c++)
        {
            ws.Cells[row, c + 1].Value = headers[c];
            ws.Cells[row, c + 1].Style.Font.Bold = true;
            ws.Cells[row, c + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[row, c + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
            ws.Cells[row, c + 1].Style.Font.Color.SetColor(Color.White);
            ws.Cells[row, c + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }
        row++;

        foreach (var linea in data.Lineas)
        {
            ws.Cells[row, 1].Value = linea.Codigo;
            ws.Cells[row, 2].Value = linea.Nombre;
            ws.Cells[row, 3].Value = linea.SumaDebe;
            ws.Cells[row, 4].Value = linea.SumaHaber;
            ws.Cells[row, 5].Value = linea.SaldoDeudor;
            ws.Cells[row, 6].Value = linea.SaldoAcreedor;
            for (int c = 3; c <= 6; c++)
                ws.Cells[row, c].Style.Numberformat.Format = "#,##0.00";
            row++;
        }

        // Totals
        for (int c = 3; c <= 6; c++)
            ws.Cells[row, c].Style.Border.Top.Style = ExcelBorderStyle.Double;

        ws.Cells[row, 2].Value = "TOTALES";
        ws.Cells[row, 2].Style.Font.Bold = true;
        ws.Cells[row, 3].Value = data.TotalSumaDebe;
        ws.Cells[row, 4].Value = data.TotalSumaHaber;
        ws.Cells[row, 5].Value = data.TotalSaldoDeudor;
        ws.Cells[row, 6].Value = data.TotalSaldoAcreedor;
        for (int c = 3; c <= 6; c++)
        {
            ws.Cells[row, c].Style.Font.Bold = true;
            ws.Cells[row, c].Style.Numberformat.Format = "#,##0.00";
        }

        ws.Column(1).Width = 16;
        ws.Column(2).Width = 40;
        for (int c = 3; c <= 6; c++) ws.Column(c).Width = 16;

        return File(pkg.GetAsByteArray(),
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

    [HttpGet("libro-diario/excel")]
    public async Task<IActionResult> ExportLibroDiarioExcel([FromQuery] DateTime desde, [FromQuery] DateTime hasta, [FromQuery] string? estado, CancellationToken ct)
    {
        var r = await _service.GetLibroDiarioAsync(desde, hasta, estado, ct);
        if (!r.IsSuccess) return BadRequest(ApiResponse<string>.Fail(r.Error!));
        var data = r.Value!;

        using var pkg = new ExcelPackage();
        var ws = pkg.Workbook.Worksheets.Add("Libro Diario");

        ws.Cells[1, 1].Value = data.Empresa;
        ws.Cells[1, 1, 1, 6].Merge = true;
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.Font.Size = 14;

        ws.Cells[2, 1].Value = "LIBRO DIARIO";
        ws.Cells[2, 1, 2, 6].Merge = true;
        ws.Cells[2, 1].Style.Font.Bold = true;
        ws.Cells[2, 1].Style.Font.Size = 12;

        ws.Cells[3, 1].Value = $"Del {data.FechaDesde:dd/MM/yyyy} al {data.FechaHasta:dd/MM/yyyy} — Gestión {data.Gestion}";
        ws.Cells[3, 1, 3, 6].Merge = true;

        int row = 5;
        string[] headers = ["Fecha", "Nro. Comprobante", "Código", "Cuenta", "Debe", "Haber"];
        for (int c = 0; c < headers.Length; c++)
        {
            ws.Cells[row, c + 1].Value = headers[c];
            ws.Cells[row, c + 1].Style.Font.Bold = true;
            ws.Cells[row, c + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[row, c + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
            ws.Cells[row, c + 1].Style.Font.Color.SetColor(Color.White);
            ws.Cells[row, c + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }
        row++;

        foreach (var entrada in data.Entradas)
        {
            // Comprobante header
            ws.Cells[row, 1].Value = entrada.Fecha.ToString("dd/MM/yyyy");
            ws.Cells[row, 2].Value = entrada.Numero;
            ws.Cells[row, 2].Style.Font.Bold = true;
            ws.Cells[row, 4].Value = entrada.Glosa;
            ws.Cells[row, 4].Style.Font.Italic = true;
            row++;

            foreach (var linea in entrada.Lineas)
            {
                ws.Cells[row, 3].Value = linea.CuentaCodigo;
                ws.Cells[row, 4].Value = linea.CuentaNombre;
                ws.Cells[row, 5].Value = linea.Debe;
                ws.Cells[row, 6].Value = linea.Haber;
                ws.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                if (linea.Haber > 0)
                    ws.Cells[row, 4].Style.Indent = 2;
                row++;
            }

            // Sub-total
            ws.Cells[row, 4].Value = "Subtotal:";
            ws.Cells[row, 4].Style.Font.Bold = true;
            ws.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[row, 5].Value = entrada.TotalDebe;
            ws.Cells[row, 6].Value = entrada.TotalHaber;
            ws.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, 5].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            ws.Cells[row, 6].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            row += 2;
        }

        // Grand total
        ws.Cells[row, 4].Value = "TOTAL LIBRO DIARIO:";
        ws.Cells[row, 4].Style.Font.Bold = true;
        ws.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        ws.Cells[row, 5].Value = data.TotalDebe;
        ws.Cells[row, 6].Value = data.TotalHaber;
        ws.Cells[row, 5].Style.Font.Bold = true;
        ws.Cells[row, 6].Style.Font.Bold = true;
        ws.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
        ws.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
        ws.Cells[row, 5].Style.Border.Top.Style = ExcelBorderStyle.Double;
        ws.Cells[row, 6].Style.Border.Top.Style = ExcelBorderStyle.Double;

        ws.Column(1).Width = 14;
        ws.Column(2).Width = 18;
        ws.Column(3).Width = 14;
        ws.Column(4).Width = 40;
        ws.Column(5).Width = 16;
        ws.Column(6).Width = 16;

        return File(pkg.GetAsByteArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Libro_Diario_{data.FechaDesde:yyyyMMdd}_{data.FechaHasta:yyyyMMdd}.xlsx");
    }

    // ????????????????????????????????????????????
    // Helpers Excel
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
