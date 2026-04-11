namespace AgoraHub360.ERP.Infrastructure.Services;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class AsientoExportService : IAsientoExportService
{
    private readonly ILogger<AsientoExportService> _logger;

    public AsientoExportService(ILogger<AsientoExportService> logger)
    {
        _logger = logger;
    }

    public Task<byte[]> ExportarExcelAsync(IEnumerable<AsientoContableDto> asientos, CancellationToken ct = default)
    {
        _logger.LogInformation("Exportando {count} asientos a Excel listado", asientos.Count());
        return Task.FromResult(ExportarExcel(asientos.ToList()));
    }

    public Task<string> GenerarCsvAsync(IEnumerable<AsientoContableDto> asientos)
    {
        _logger.LogInformation("Exportando {count} asientos a CSV", asientos.Count());
        return Task.FromResult(AgoraHub360.ERP.Shared.Utils.ExportFormatHelper.GenerarCsvListado(asientos.ToList()));
    }

    public Task<string> GenerarJsonAsync(IEnumerable<AsientoContableDto> asientos)
    {
        _logger.LogInformation("Exportando {count} asientos a JSON", asientos.Count());
        return Task.FromResult(AgoraHub360.ERP.Shared.Utils.ExportFormatHelper.GenerarJsonPlano(asientos.ToList()));
    }

    public Task<string> GenerarXmlAsync(IEnumerable<AsientoContableDto> asientos)
    {
        _logger.LogInformation("Exportando {count} asientos a XML", asientos.Count());
        return Task.FromResult(AgoraHub360.ERP.Shared.Utils.ExportFormatHelper.GenerarXmlPlano(asientos.ToList()));
    }

    public byte[] ExportarExcel(IReadOnlyList<AsientoContableDto> asientos)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Comprobantes Contables");

        worksheet.Cell(1, 1).Value = "Tipo";
        worksheet.Cell(1, 2).Value = "Número";
        worksheet.Cell(1, 3).Value = "Fecha";
        worksheet.Cell(1, 4).Value = "Gestión";
        worksheet.Cell(1, 5).Value = "Concepto";
        worksheet.Cell(1, 6).Value = "Glosa";
        worksheet.Cell(1, 7).Value = "Tipo Registro";
        worksheet.Cell(1, 8).Value = "T/C Moneda";
        worksheet.Cell(1, 9).Value = "Valor T/C";
        worksheet.Cell(1, 10).Value = "Tipo Pago";
        worksheet.Cell(1, 11).Value = "Nro Documento";
        worksheet.Cell(1, 12).Value = "Total Debe";
        worksheet.Cell(1, 13).Value = "Total Haber";
        worksheet.Cell(1, 14).Value = "Estado";
        worksheet.Cell(1, 15).Value = "Registrado Por";

        var headerRange = worksheet.Range(1, 1, 1, 15);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(79, 129, 189);
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        int row = 2;
        foreach (var asiento in asientos)
        {
            worksheet.Cell(row, 1).Value = asiento.TipoComprobanteCodigo;
            worksheet.Cell(row, 2).Value = asiento.Numero;
            worksheet.Cell(row, 3).Value = asiento.Fecha.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 4).Value = asiento.Gestion;
            worksheet.Cell(row, 5).Value = asiento.Concepto ?? "";
            worksheet.Cell(row, 6).Value = asiento.Glosa;
            worksheet.Cell(row, 7).Value = asiento.TipoRegistro;
            worksheet.Cell(row, 8).Value = asiento.TipoCambioMoneda ?? "";
            worksheet.Cell(row, 9).Value = asiento.ValorTipoCambio?.ToString("N2") ?? "";
            worksheet.Cell(row, 10).Value = asiento.TipoPagoNombre ?? "";
            worksheet.Cell(row, 11).Value = asiento.NumeroDocumentoPago ?? "";
            worksheet.Cell(row, 12).Value = asiento.TotalDebe;
            worksheet.Cell(row, 13).Value = asiento.TotalHaber;
            worksheet.Cell(row, 14).Value = asiento.Estado;
            worksheet.Cell(row, 15).Value = asiento.RegistradoPorNombre ?? "";

            worksheet.Cell(row, 12).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 13).Style.NumberFormat.Format = "#,##0.00";

            row++;
        }

        worksheet.Columns().AdjustToContents();

        if (asientos.Any())
        {
            worksheet.Cell(row, 11).Value = "TOTALES:";
            worksheet.Cell(row, 11).Style.Font.Bold = true;
            worksheet.Cell(row, 12).FormulaA1 = $"SUM(L2:L{row - 1})";
            worksheet.Cell(row, 13).FormulaA1 = $"SUM(M2:M{row - 1})";
            worksheet.Cell(row, 12).Style.Font.Bold = true;
            worksheet.Cell(row, 13).Style.Font.Bold = true;
            worksheet.Cell(row, 12).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 13).Style.NumberFormat.Format = "#,##0.00";

            var totalRange = worksheet.Range(row, 11, row, 13);
            totalRange.Style.Border.TopBorder = XLBorderStyleValues.Double;
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportarExcelIndividual(AsientoContableDto a)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add($"Comprobante {a.Numero}");

        ws.Cell(1, 1).Value = "COMPROBANTE CONTABLE";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(3, 1).Value = "Tipo:"; ws.Cell(3, 2).Value = $"{a.TipoComprobanteCodigo} - {a.TipoComprobanteNombre}";
        ws.Cell(4, 1).Value = "Número:"; ws.Cell(4, 2).Value = a.Numero;
        ws.Cell(5, 1).Value = "Fecha:"; ws.Cell(5, 2).Value = a.Fecha.ToString("dd/MM/yyyy");
        ws.Cell(6, 1).Value = "Gestión:"; ws.Cell(6, 2).Value = a.Gestion;
        ws.Cell(7, 1).Value = "Estado:"; ws.Cell(7, 2).Value = a.Estado;
        ws.Cell(8, 1).Value = "Concepto:"; ws.Cell(8, 2).Value = a.Concepto ?? "";
        ws.Cell(9, 1).Value = "Glosa:"; ws.Cell(9, 2).Value = a.Glosa;
        ws.Cell(10, 1).Value = "Registrado por:"; ws.Cell(10, 2).Value = a.RegistradoPorNombre ?? "";

        if (a.TipoCambioMoneda != null)
        {
            ws.Cell(11, 1).Value = "Tipo Cambio:";
            ws.Cell(11, 2).Value = $"{a.TipoCambioMoneda} {a.ValorTipoCambio:N2}";
        }
        if (!string.IsNullOrEmpty(a.TipoPagoCodigo) && a.TipoPagoCodigo != "S/D")
        {
            ws.Cell(12, 1).Value = "Documento Pago:";
            ws.Cell(12, 2).Value = $"{a.TipoPagoNombre} {a.NumeroDocumentoPago}";
        }

        ws.Range(3, 1, 12, 1).Style.Font.Bold = true;

        int headerRow = 14;
        ws.Cell(headerRow, 1).Value = "#";
        ws.Cell(headerRow, 2).Value = "Código";
        ws.Cell(headerRow, 3).Value = "Nombre Cuenta";
        ws.Cell(headerRow, 4).Value = "Glosa Detalle";
        ws.Cell(headerRow, 5).Value = "Centro Costo";
        ws.Cell(headerRow, 6).Value = "Debe";
        ws.Cell(headerRow, 7).Value = "Haber";

        var detailHeaderRange = ws.Range(headerRow, 1, headerRow, 7);
        detailHeaderRange.Style.Font.Bold = true;
        detailHeaderRange.Style.Fill.BackgroundColor = XLColor.FromArgb(79, 129, 189);
        detailHeaderRange.Style.Font.FontColor = XLColor.White;

        int row = headerRow + 1;
        foreach (var l in a.Lineas)
        {
            ws.Cell(row, 1).Value = l.NumeroLinea;
            ws.Cell(row, 2).Value = l.CuentaCodigo;
            ws.Cell(row, 3).Value = l.CuentaNombre;
            ws.Cell(row, 4).Value = l.Glosa ?? "";
            ws.Cell(row, 5).Value = l.CentroCostoCodigo ?? "";
            ws.Cell(row, 6).Value = l.Debe;
            ws.Cell(row, 7).Value = l.Haber;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        ws.Cell(row, 5).Value = "TOTALES:";
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Cell(row, 6).Value = a.TotalDebe;
        ws.Cell(row, 7).Value = a.TotalHaber;
        ws.Cell(row, 6).Style.Font.Bold = true;
        ws.Cell(row, 7).Style.Font.Bold = true;
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
        ws.Range(row, 5, row, 7).Style.Border.TopBorder = XLBorderStyleValues.Double;

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportarExcelPlano(IReadOnlyList<AsientoContableDto> asientos)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Detalle Plano");

        var headers = new[]
        {
            "Tipo","Numero","Fecha","Gestion","Concepto","Glosa","TipoRegistro",
            "Estado","TipoCambioMoneda","ValorTipoCambio","TipoPago","NroDocPago",
            "TotalDebe","TotalHaber","RegistradoPor",
            "LineaNro","CuentaCodigo","CuentaNombre","LineaDebe","LineaHaber",
            "LineaGlosa","CentroCosto"
        };
        for (int c = 0; c < headers.Length; c++)
            ws.Cell(1, c + 1).Value = headers[c];

        var planoHeaderRange = ws.Range(1, 1, 1, headers.Length);
        planoHeaderRange.Style.Font.Bold = true;
        planoHeaderRange.Style.Fill.BackgroundColor = XLColor.FromArgb(31, 73, 125);
        planoHeaderRange.Style.Font.FontColor = XLColor.White;

        int row = 2;
        var altRow = false;
        foreach (var a in asientos)
        {
            var lineas = a.Lineas.Count > 0 ? a.Lineas : new List<AsientoContableLineaDto> { new() };
            foreach (var l in lineas)
            {
                ws.Cell(row, 1).Value  = a.TipoComprobanteCodigo;
                ws.Cell(row, 2).Value  = a.Numero;
                ws.Cell(row, 3).Value  = a.Fecha.ToString("dd/MM/yyyy");
                ws.Cell(row, 4).Value  = a.Gestion;
                ws.Cell(row, 5).Value  = a.Concepto ?? "";
                ws.Cell(row, 6).Value  = a.Glosa;
                ws.Cell(row, 7).Value  = a.TipoRegistro;
                ws.Cell(row, 8).Value  = a.Estado;
                ws.Cell(row, 9).Value  = a.TipoCambioMoneda ?? "";
                ws.Cell(row, 10).Value = a.ValorTipoCambio?.ToString("N2") ?? "";
                ws.Cell(row, 11).Value = a.TipoPagoNombre ?? "";
                ws.Cell(row, 12).Value = a.NumeroDocumentoPago ?? "";
                ws.Cell(row, 13).Value = a.TotalDebe;
                ws.Cell(row, 14).Value = a.TotalHaber;
                ws.Cell(row, 15).Value = a.RegistradoPorNombre ?? "";
                if (l.NumeroLinea > 0) ws.Cell(row, 16).Value = l.NumeroLinea;
                ws.Cell(row, 17).Value = l.CuentaCodigo;
                ws.Cell(row, 18).Value = l.CuentaNombre;
                if (l.Debe > 0) ws.Cell(row, 19).Value = l.Debe;
                if (l.Haber > 0) ws.Cell(row, 20).Value = l.Haber;
                ws.Cell(row, 21).Value = l.Glosa ?? "";
                ws.Cell(row, 22).Value = l.CentroCostoCodigo ?? "";

                ws.Cell(row, 13).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 14).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 19).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 20).Style.NumberFormat.Format = "#,##0.00";

                if (altRow)
                {
                    ws.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.FromArgb(242, 242, 242);
                }
                row++;
            }
            altRow = !altRow;
        }

        if (row > 2)
            ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
