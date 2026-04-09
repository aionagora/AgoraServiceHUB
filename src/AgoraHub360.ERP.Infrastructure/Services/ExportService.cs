namespace AgoraHub360.ERP.Infrastructure.Services;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class ExportService : IExportService
{
    public ExportService()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    #region Helpers Excel
    private ExcelWorksheet ConfigurarReporteExcel(ExcelPackage package, string nombreEmpresa, string titulo, string subtitulo, int maxColumns)
    {
        var ws = package.Workbook.Worksheets.Add("Reporte");
        
        ws.Cells[1, 1].Value = nombreEmpresa;
        if (maxColumns > 1) ws.Cells[1, 1, 1, maxColumns].Merge = true;
        ws.Cells[1, 1].Style.Font.Bold = true;
        ws.Cells[1, 1].Style.Font.Size = 14;

        ws.Cells[2, 1].Value = titulo;
        if (maxColumns > 1) ws.Cells[2, 1, 2, maxColumns].Merge = true;
        ws.Cells[2, 1].Style.Font.Bold = true;
        ws.Cells[2, 1].Style.Font.Size = 12;

        ws.Cells[3, 1].Value = subtitulo;
        if (maxColumns > 1) ws.Cells[3, 1, 3, maxColumns].Merge = true;

        return ws;
    }

    private void AplicarEstiloHeaderExcel(ExcelWorksheet ws, int row, params string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[row, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(20, 50, 90));
        }
    }

    private void AplicarEstiloFilaDatosExcel(ExcelWorksheet ws, int row, int colCount, bool isAlternate)
    {
        if (isAlternate)
        {
            ws.Cells[row, 1, row, colCount].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[row, 1, row, colCount].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240, 240, 240));
        }
    }

    private void FormatoNumerico(ExcelWorksheet ws, int row, int col)
    {
        ws.Cells[row, col].Style.Numberformat.Format = "#,##0.00";
    }
    #endregion

    #region Helpers PDF
    private Document GenerarDocumentoPdfBase(string nombreEmpresa, string titulo, string subtitulo, Action<ColumnDescriptor> contenido)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text(nombreEmpresa).FontSize(14).Bold();
                    col.Item().Text(titulo).FontSize(12).Bold();
                    col.Item().Text(subtitulo).FontSize(10);
                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(contenido);

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        });
    }
    #endregion

    // =======================================================
    // BALANCE GENERAL
    // =======================================================
    public byte[] ExportarBalanceGeneralExcel(BalanceGeneralDto dto, string nombreEmpresa)
    {
        using var package = new ExcelPackage();
        var ws = ConfigurarReporteExcel(package, nombreEmpresa, "BALANCE GENERAL", $"Al {dto.FechaCorte:dd/MM/yyyy} - Gestión {dto.Gestion}", 3);
        AplicarEstiloHeaderExcel(ws, 5, "Código", "Cuenta", "Saldo");

        int row = 6;
        bool isAlt = false;

        void EscribirNodos(List<BalanceGrupoDto> nodos, int indent)
        {
            foreach(var n in nodos)
            {
                AplicarEstiloFilaDatosExcel(ws, row, 3, isAlt);
                ws.Cells[row, 1].Value = n.Codigo;
                ws.Cells[row, 2].Value = n.Nombre;
                ws.Cells[row, 2].Style.Indent = indent;
                ws.Cells[row, 3].Value = n.Saldo;
                FormatoNumerico(ws, row, 3);
                if (n.EsAgrupador) ws.Cells[row, 1, row, 3].Style.Font.Bold = true;
                row++;
                isAlt = !isAlt;
                EscribirNodos(n.SubCuentas, indent + 1);
            }
        }

        ws.Cells[row, 1].Value = "ACTIVOS";
        ws.Cells[row, 1, row, 3].Style.Font.Bold = true;
        row++;
        EscribirNodos(dto.Activos, 0);
        
        ws.Cells[row, 2].Value = "TOTAL ACTIVOS";
        ws.Cells[row, 3].Value = dto.TotalActivos;
        ws.Cells[row, 2, row, 3].Style.Font.Bold = true;
        FormatoNumerico(ws, row, 3);
        row += 2;

        ws.Cells[row, 1].Value = "PASIVOS";
        ws.Cells[row, 1, row, 3].Style.Font.Bold = true;
        row++;
        EscribirNodos(dto.Pasivos, 0);

        ws.Cells[row, 2].Value = "TOTAL PASIVOS";
        ws.Cells[row, 3].Value = dto.TotalPasivos;
        ws.Cells[row, 2, row, 3].Style.Font.Bold = true;
        FormatoNumerico(ws, row, 3);
        row += 2;

        ws.Cells[row, 1].Value = "PATRIMONIO";
        ws.Cells[row, 1, row, 3].Style.Font.Bold = true;
        row++;
        EscribirNodos(dto.Patrimonio, 0);

        ws.Cells[row, 2].Value = "TOTAL PATRIMONIO";
        ws.Cells[row, 3].Value = dto.TotalPatrimonio;
        ws.Cells[row, 2, row, 3].Style.Font.Bold = true;
        FormatoNumerico(ws, row, 3);
        row += 2;

        ws.Cells[row, 2].Value = "TOTAL PASIVO + PATRIMONIO";
        ws.Cells[row, 3].Value = dto.TotalPasivoPatrimonio;
        ws.Cells[row, 2, row, 3].Style.Font.Bold = true;
        FormatoNumerico(ws, row, 3);

        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] ExportarBalanceGeneralPdf(BalanceGeneralDto dto, string nombreEmpresa)
    {
        void EscribirNodosPdf(ColumnDescriptor col, List<BalanceGrupoDto> nodos, int indent)
        {
            foreach(var n in nodos)
            {
                col.Item().PaddingLeft(indent * 10).Row(r => 
                {
                    var t = r.RelativeItem(8).Text($"{n.Codigo} - {n.Nombre}");
                    var v = r.RelativeItem(2).AlignRight().Text($"{n.Saldo:N2}");
                    if(n.EsAgrupador) { t.Bold(); v.Bold(); }
                });
                EscribirNodosPdf(col, n.SubCuentas, indent + 1);
            }
        }

        var doc = GenerarDocumentoPdfBase(nombreEmpresa, "BALANCE GENERAL", $"Al {dto.FechaCorte:dd/MM/yyyy} - Gestión {dto.Gestion}", col =>
        {
            col.Item().PaddingTop(5).Text("ACTIVOS").Bold();
            EscribirNodosPdf(col, dto.Activos, 0);
            col.Item().PaddingBottom(10).AlignRight().Text($"TOTAL ACTIVOS: {dto.TotalActivos:N2}").Bold();

            col.Item().PaddingTop(5).Text("PASIVOS").Bold();
            EscribirNodosPdf(col, dto.Pasivos, 0);
            col.Item().PaddingBottom(10).AlignRight().Text($"TOTAL PASIVOS: {dto.TotalPasivos:N2}").Bold();

            col.Item().PaddingTop(5).Text("PATRIMONIO").Bold();
            EscribirNodosPdf(col, dto.Patrimonio, 0);
            col.Item().PaddingBottom(10).AlignRight().Text($"TOTAL PATRIMONIO: {dto.TotalPatrimonio:N2}").Bold();

            col.Item().PaddingTop(10).AlignRight().Text($"TOTAL PASIVO + PATRIMONIO: {dto.TotalPasivoPatrimonio:N2}").Bold();
        });

        return doc.GeneratePdf();
    }

    // =======================================================
    // ESTADO DE RESULTADOS
    // =======================================================
    public byte[] ExportarEstadoResultadosExcel(EstadoResultadosDto dto, string nombreEmpresa)
    {
        using var package = new ExcelPackage();
        var ws = ConfigurarReporteExcel(package, nombreEmpresa, "ESTADO DE RESULTADOS", $"Del {dto.FechaDesde:dd/MM/yyyy} al {dto.FechaHasta:dd/MM/yyyy} - Gestión {dto.Gestion}", 3);
        AplicarEstiloHeaderExcel(ws, 5, "Código", "Cuenta", "Saldo");

        int row = 6;
        bool isAlt = false;

        void EscribirNodos(List<BalanceGrupoDto> nodos, int indent)
        {
            foreach(var n in nodos)
            {
                AplicarEstiloFilaDatosExcel(ws, row, 3, isAlt);
                ws.Cells[row, 1].Value = n.Codigo;
                ws.Cells[row, 2].Value = n.Nombre;
                ws.Cells[row, 2].Style.Indent = indent;
                ws.Cells[row, 3].Value = n.Saldo;
                FormatoNumerico(ws, row, 3);
                if (n.EsAgrupador) ws.Cells[row, 1, row, 3].Style.Font.Bold = true;
                row++;
                isAlt = !isAlt;
                EscribirNodos(n.SubCuentas, indent + 1);
            }
        }

        ws.Cells[row, 1].Value = "INGRESOS";
        ws.Cells[row, 1, row, 3].Style.Font.Bold = true;
        row++;
        EscribirNodos(dto.Ingresos, 0);
        
        ws.Cells[row, 2].Value = "TOTAL INGRESOS";
        ws.Cells[row, 3].Value = dto.TotalIngresos;
        ws.Cells[row, 2, row, 3].Style.Font.Bold = true;
        FormatoNumerico(ws, row, 3);
        row += 2;

        ws.Cells[row, 1].Value = "COSTOS";
        ws.Cells[row, 1, row, 3].Style.Font.Bold = true;
        row++;
        EscribirNodos(dto.Costos, 0);

        ws.Cells[row, 2].Value = "TOTAL COSTOS";
        ws.Cells[row, 3].Value = dto.TotalCostos;
        ws.Cells[row, 2, row, 3].Style.Font.Bold = true;
        FormatoNumerico(ws, row, 3);
        row += 2;

        ws.Cells[row, 2].Value = "UTILIDAD BRUTA";
        ws.Cells[row, 3].Value = dto.UtilidadBruta;
        ws.Cells[row, 2, row, 3].Style.Font.Bold = true;
        FormatoNumerico(ws, row, 3);
        row += 2;

        ws.Cells[row, 1].Value = "GASTOS";
        ws.Cells[row, 1, row, 3].Style.Font.Bold = true;
        row++;
        EscribirNodos(dto.Gastos, 0);

        ws.Cells[row, 2].Value = "TOTAL GASTOS";
        ws.Cells[row, 3].Value = dto.TotalGastos;
        ws.Cells[row, 2, row, 3].Style.Font.Bold = true;
        FormatoNumerico(ws, row, 3);
        row += 2;

        ws.Cells[row, 2].Value = "UTILIDAD NETA";
        ws.Cells[row, 3].Value = dto.UtilidadNeta;
        ws.Cells[row, 2, row, 3].Style.Font.Bold = true;
        FormatoNumerico(ws, row, 3);

        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] ExportarEstadoResultadosPdf(EstadoResultadosDto dto, string nombreEmpresa)
    {
        void EscribirNodosPdf(ColumnDescriptor col, List<BalanceGrupoDto> nodos, int indent)
        {
            foreach(var n in nodos)
            {
                col.Item().PaddingLeft(indent * 10).Row(r => 
                {
                    var t = r.RelativeItem(8).Text($"{n.Codigo} - {n.Nombre}");
                    var v = r.RelativeItem(2).AlignRight().Text($"{n.Saldo:N2}");
                    if(n.EsAgrupador) { t.Bold(); v.Bold(); }
                });
                EscribirNodosPdf(col, n.SubCuentas, indent + 1);
            }
        }

        var doc = GenerarDocumentoPdfBase(nombreEmpresa, "ESTADO DE RESULTADOS", $"Del {dto.FechaDesde:dd/MM/yyyy} al {dto.FechaHasta:dd/MM/yyyy} - Gestión {dto.Gestion}", col =>
        {
            col.Item().PaddingTop(5).Text("INGRESOS").Bold();
            EscribirNodosPdf(col, dto.Ingresos, 0);
            col.Item().PaddingBottom(10).AlignRight().Text($"TOTAL INGRESOS: {dto.TotalIngresos:N2}").Bold();

            col.Item().PaddingTop(5).Text("COSTOS").Bold();
            EscribirNodosPdf(col, dto.Costos, 0);
            col.Item().PaddingBottom(10).AlignRight().Text($"TOTAL COSTOS: {dto.TotalCostos:N2}").Bold();

            col.Item().PaddingBottom(10).AlignRight().Text($"UTILIDAD BRUTA: {dto.UtilidadBruta:N2}").Bold();

            col.Item().PaddingTop(5).Text("GASTOS").Bold();
            EscribirNodosPdf(col, dto.Gastos, 0);
            col.Item().PaddingBottom(10).AlignRight().Text($"TOTAL GASTOS: {dto.TotalGastos:N2}").Bold();

            col.Item().PaddingTop(10).AlignRight().Text($"UTILIDAD NETA: {dto.UtilidadNeta:N2}").Bold();
        });

        return doc.GeneratePdf();
    }

    // =======================================================
    // LIBRO DIARIO
    // =======================================================
    public byte[] ExportarLibroDiarioExcel(LibroDiarioDto dto, string nombreEmpresa)
    {
        using var package = new ExcelPackage();
        var ws = ConfigurarReporteExcel(package, nombreEmpresa, "LIBRO DIARIO", $"Del {dto.FechaDesde:dd/MM/yyyy} al {dto.FechaHasta:dd/MM/yyyy}", 6);
        AplicarEstiloHeaderExcel(ws, 5, "Fecha", "Número", "Código Cuenta", "Nombre Cuenta", "Debe", "Haber");

        int row = 6;
        bool isAlt = false;

        foreach (var entrada in dto.Entradas)
        {
            AplicarEstiloFilaDatosExcel(ws, row, 6, isAlt);
            ws.Cells[row, 1].Value = entrada.Fecha.ToString("dd/MM/yyyy");
            ws.Cells[row, 2].Value = entrada.Numero;
            ws.Cells[row, 4].Value = entrada.Glosa;
            ws.Cells[row, 4].Style.Font.Italic = true;
            row++;

            foreach (var linea in entrada.Lineas)
            {
                AplicarEstiloFilaDatosExcel(ws, row, 6, isAlt);
                ws.Cells[row, 3].Value = linea.CuentaCodigo;
                ws.Cells[row, 4].Value = linea.CuentaNombre;
                ws.Cells[row, 5].Value = linea.Debe;
                ws.Cells[row, 6].Value = linea.Haber;
                FormatoNumerico(ws, row, 5);
                FormatoNumerico(ws, row, 6);
                row++;
            }
            
            AplicarEstiloFilaDatosExcel(ws, row, 6, isAlt);
            ws.Cells[row, 4].Value = "Total Asiento:";
            ws.Cells[row, 5].Value = entrada.TotalDebe;
            ws.Cells[row, 6].Value = entrada.TotalHaber;
            ws.Cells[row, 4, row, 6].Style.Font.Bold = true;
            FormatoNumerico(ws, row, 5);
            FormatoNumerico(ws, row, 6);
            row += 2;
            isAlt = !isAlt;
        }

        ws.Cells[row, 4].Value = "TOTAL GENERAL:";
        ws.Cells[row, 5].Value = dto.TotalDebe;
        ws.Cells[row, 6].Value = dto.TotalHaber;
        ws.Cells[row, 4, row, 6].Style.Font.Bold = true;
        FormatoNumerico(ws, row, 5);
        FormatoNumerico(ws, row, 6);

        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] ExportarLibroDiarioPdf(LibroDiarioDto dto, string nombreEmpresa)
    {
        var doc = GenerarDocumentoPdfBase(nombreEmpresa, "LIBRO DIARIO", $"Del {dto.FechaDesde:dd/MM/yyyy} al {dto.FechaHasta:dd/MM/yyyy}", col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(6);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(h =>
                {
                    h.Cell().Text("Fecha").Bold();
                    h.Cell().Text("Número").Bold();
                    h.Cell().Text("Código").Bold();
                    h.Cell().Text("Nombre Cuenta / Glosa").Bold();
                    h.Cell().AlignRight().Text("Debe").Bold();
                    h.Cell().AlignRight().Text("Haber").Bold();
                });

                foreach (var entrada in dto.Entradas)
                {
                    table.Cell().Text(entrada.Fecha.ToString("dd/MM/yyyy"));
                    table.Cell().Text(entrada.Numero);
                    table.Cell().ColumnSpan(4).Text(entrada.Glosa ?? string.Empty).Italic();

                    foreach (var linea in entrada.Lineas)
                    {
                        table.Cell().Text("");
                        table.Cell().Text("");
                        table.Cell().Text(linea.CuentaCodigo);
                        table.Cell().Text(linea.CuentaNombre);
                        table.Cell().AlignRight().Text($"{linea.Debe:N2}");
                        table.Cell().AlignRight().Text($"{linea.Haber:N2}");
                    }
                    
                    table.Cell().ColumnSpan(4).AlignRight().Text("Total Asiento:").Bold();
                    table.Cell().AlignRight().Text($"{entrada.TotalDebe:N2}").Bold();
                    table.Cell().PaddingBottom(10).AlignRight().Text($"{entrada.TotalHaber:N2}").Bold();
                }

                table.Cell().ColumnSpan(4).AlignRight().Text("TOTAL GENERAL:").Bold();
                table.Cell().AlignRight().Text($"{dto.TotalDebe:N2}").Bold();
                table.Cell().AlignRight().Text($"{dto.TotalHaber:N2}").Bold();
            });
        });

        return doc.GeneratePdf();
    }

    // =======================================================
    // SUMAS Y SALDOS
    // =======================================================
    public byte[] ExportarSumasYSaldosExcel(SumasYSaldosDto dto, string nombreEmpresa)
    {
        using var package = new ExcelPackage();
        var ws = ConfigurarReporteExcel(package, nombreEmpresa, "BALANCE DE COMPROBACIÓN DE SUMAS Y SALDOS", $"Del {dto.FechaDesde:dd/MM/yyyy} al {dto.FechaHasta:dd/MM/yyyy} - Gestión {dto.Gestion}", 6);
        AplicarEstiloHeaderExcel(ws, 5, "Código", "Cuenta", "Suma Debe", "Suma Haber", "Saldo Deudor", "Saldo Acreedor");

        int row = 6;
        bool isAlt = false;

        foreach(var linea in dto.Lineas)
        {
            AplicarEstiloFilaDatosExcel(ws, row, 6, isAlt);
            ws.Cells[row, 1].Value = linea.Codigo;
            ws.Cells[row, 2].Value = linea.Nombre;
            ws.Cells[row, 3].Value = linea.SumaDebe;
            ws.Cells[row, 4].Value = linea.SumaHaber;
            ws.Cells[row, 5].Value = linea.SaldoDeudor;
            ws.Cells[row, 6].Value = linea.SaldoAcreedor;
            FormatoNumerico(ws, row, 3);
            FormatoNumerico(ws, row, 4);
            FormatoNumerico(ws, row, 5);
            FormatoNumerico(ws, row, 6);
            row++;
            isAlt = !isAlt;
        }

        ws.Cells[row, 2].Value = "TOTALES:";
        ws.Cells[row, 3].Value = dto.TotalSumaDebe;
        ws.Cells[row, 4].Value = dto.TotalSumaHaber;
        ws.Cells[row, 5].Value = dto.TotalSaldoDeudor;
        ws.Cells[row, 6].Value = dto.TotalSaldoAcreedor;
        ws.Cells[row, 2, row, 6].Style.Font.Bold = true;
        FormatoNumerico(ws, row, 3);
        FormatoNumerico(ws, row, 4);
        FormatoNumerico(ws, row, 5);
        FormatoNumerico(ws, row, 6);

        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    // =======================================================
    // FLUJO DE EFECTIVO
    // =======================================================
    public byte[] ExportarFlujoDEfectivoExcel(FlujoDEfectivoDto dto, string nombreEmpresa)
    {
        using var package = new ExcelPackage();
        var ws = ConfigurarReporteExcel(package, nombreEmpresa, "ESTADO DE FLUJO DE EFECTIVO", $"Del {dto.Desde:dd/MM/yyyy} al {dto.Hasta:dd/MM/yyyy}", 3);
        AplicarEstiloHeaderExcel(ws, 5, "Código", "Cuenta", "Monto");

        int row = 6;
        bool isAlt = false;
        
        void EscribirSeccion(string titulo, List<FlujoDEfectivoLineaDto> lineas, decimal total)
        {
            ws.Cells[row, 1].Value = titulo;
            ws.Cells[row, 1, row, 3].Style.Font.Bold = true;
            row++;

            foreach(var l in lineas)
            {
                AplicarEstiloFilaDatosExcel(ws, row, 3, isAlt);
                ws.Cells[row, 1].Value = l.CodigoCuenta;
                ws.Cells[row, 2].Value = l.NombreCuenta;
                ws.Cells[row, 3].Value = l.Monto;
                FormatoNumerico(ws, row, 3);
                row++;
                isAlt = !isAlt;
            }
            
            ws.Cells[row, 2].Value = $"TOTAL {titulo}";
            ws.Cells[row, 3].Value = total;
            ws.Cells[row, 2, row, 3].Style.Font.Bold = true;
            FormatoNumerico(ws, row, 3);
            row += 2;
        }

        EscribirSeccion("ACTIVIDADES OPERACIONALES", dto.LineasOperacional, dto.TotalOperacional);
        EscribirSeccion("ACTIVIDADES DE INVERSIÓN", dto.LineasInversion, dto.TotalInversion);
        EscribirSeccion("ACTIVIDADES DE FINANCIACIÓN", dto.LineasFinanciacion, dto.TotalFinanciacion);

        ws.Cells[row, 2].Value = "VARIACIÓN NETA EFECTIVO";
        ws.Cells[row, 3].Value = dto.VariacionNetaEfectivo;
        ws.Cells[row, 2, row, 3].Style.Font.Bold = true;
        FormatoNumerico(ws, row, 3);
        row++;
        
        ws.Cells[row, 2].Value = "SALDO INICIAL EFECTIVO";
        ws.Cells[row, 3].Value = dto.SaldoInicialEfectivo;
        FormatoNumerico(ws, row, 3);
        row++;

        ws.Cells[row, 2].Value = "SALDO FINAL EFECTIVO";
        ws.Cells[row, 3].Value = dto.SaldoFinalEfectivo;
        ws.Cells[row, 2, row, 3].Style.Font.Bold = true;
        FormatoNumerico(ws, row, 3);

        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] ExportarFlujoDEfectivoPdf(FlujoDEfectivoDto dto, string nombreEmpresa)
    {
        void EscribirSeccionPdf(ColumnDescriptor col, string titulo, List<FlujoDEfectivoLineaDto> lineas, decimal total)
        {
            col.Item().PaddingTop(5).Text(titulo).Bold();
            foreach (var l in lineas)
            {
                col.Item().Row(r => { 
                    r.RelativeItem(2).Text(l.CodigoCuenta); 
                    r.RelativeItem(6).Text(l.NombreCuenta); 
                    r.RelativeItem(2).AlignRight().Text($"{l.Monto:N2}"); 
                });
            }
            col.Item().PaddingBottom(10).AlignRight().Text($"TOTAL {titulo}: {total:N2}").Bold();
        }

        var doc = GenerarDocumentoPdfBase(nombreEmpresa, "ESTADO DE FLUJO DE EFECTIVO", $"Del {dto.Desde:dd/MM/yyyy} al {dto.Hasta:dd/MM/yyyy}", col =>
        {
            EscribirSeccionPdf(col, "ACTIVIDADES OPERACIONALES", dto.LineasOperacional, dto.TotalOperacional);
            EscribirSeccionPdf(col, "ACTIVIDADES DE INVERSIÓN", dto.LineasInversion, dto.TotalInversion);
            EscribirSeccionPdf(col, "ACTIVIDADES DE FINANCIACIÓN", dto.LineasFinanciacion, dto.TotalFinanciacion);

            col.Item().PaddingTop(10).AlignRight().Text($"VARIACIÓN NETA EFECTIVO: {dto.VariacionNetaEfectivo:N2}").Bold();
            col.Item().AlignRight().Text($"SALDO INICIAL EFECTIVO: {dto.SaldoInicialEfectivo:N2}");
            col.Item().AlignRight().Text($"SALDO FINAL EFECTIVO: {dto.SaldoFinalEfectivo:N2}").Bold();
        });

        return doc.GeneratePdf();
    }

    // =======================================================
    // LIBRO MAYOR
    // =======================================================
    public byte[] ExportarLibroMayorExcel(LibroMayorDto dto, string nombreEmpresa)
    {
        using var package = new ExcelPackage();
        var ws = ConfigurarReporteExcel(package, nombreEmpresa, "LIBRO MAYOR", $"Cuenta: {dto.CodigoCuenta} - {dto.NombreCuenta}", 6);
        AplicarEstiloHeaderExcel(ws, 5, "Fecha", "Número Asiento", "Glosa", "Debe", "Haber", "Saldo Progresivo");

        int row = 6;
        bool isAlt = false;

        ws.Cells[row, 1].Value = $"Saldo Anterior: {dto.SaldoAnterior:N2}";
        ws.Cells[row, 1, row, 6].Merge = true;
        ws.Cells[row, 1].Style.Font.Bold = true;
        row++;

        foreach(var linea in dto.Lineas)
        {
            AplicarEstiloFilaDatosExcel(ws, row, 6, isAlt);
            ws.Cells[row, 1].Value = linea.Fecha.ToString("dd/MM/yyyy");
            ws.Cells[row, 2].Value = linea.NumeroAsiento;
            ws.Cells[row, 3].Value = linea.Glosa;
            ws.Cells[row, 4].Value = linea.Debe;
            ws.Cells[row, 5].Value = linea.Haber;
            ws.Cells[row, 6].Value = linea.SaldoProgresivo;
            FormatoNumerico(ws, row, 4);
            FormatoNumerico(ws, row, 5);
            FormatoNumerico(ws, row, 6);
            row++;
            isAlt = !isAlt;
        }

        ws.Cells[row, 3].Value = "TOTAL PERÍODO:";
        ws.Cells[row, 4].Value = dto.TotalDebe;
        ws.Cells[row, 5].Value = dto.TotalHaber;
        ws.Cells[row, 3, row, 5].Style.Font.Bold = true;
        FormatoNumerico(ws, row, 4);
        FormatoNumerico(ws, row, 5);
        row++;
        
         ws.Cells[row, 5].Value = "SALDO FINAL:";
         ws.Cells[row, 6].Value = dto.SaldoFinal;
         ws.Cells[row, 5, row, 6].Style.Font.Bold = true;
         FormatoNumerico(ws, row, 6);

        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public byte[] ExportarLibroMayorPdf(LibroMayorDto dto, string nombreEmpresa)
    {
        var doc = GenerarDocumentoPdfBase(nombreEmpresa, "LIBRO MAYOR", $"Cuenta: {dto.CodigoCuenta} - {dto.NombreCuenta}", col =>
        {
            col.Item().PaddingBottom(10).Text($"Saldo Anterior: {dto.SaldoAnterior:N2}").Bold();

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(h =>
                {
                    h.Cell().Text("Fecha").Bold();
                    h.Cell().Text("Número").Bold();
                    h.Cell().Text("Glosa").Bold();
                    h.Cell().AlignRight().Text("Debe").Bold();
                    h.Cell().AlignRight().Text("Haber").Bold();
                    h.Cell().AlignRight().Text("Saldo").Bold();
                });

                foreach (var linea in dto.Lineas)
                {
                    table.Cell().Text(linea.Fecha.ToString("dd/MM/yyyy"));
                    table.Cell().Text(linea.NumeroAsiento);
                    table.Cell().Text(linea.Glosa);
                    table.Cell().AlignRight().Text($"{linea.Debe:N2}");
                    table.Cell().AlignRight().Text($"{linea.Haber:N2}");
                    table.Cell().AlignRight().Text($"{linea.SaldoProgresivo:N2}");
                }
            });

            col.Item().PaddingTop(10).AlignRight().Text($"TOTAL DEBE: {dto.TotalDebe:N2} | TOTAL HABER: {dto.TotalHaber:N2}").Bold();
            col.Item().AlignRight().Text($"SALDO FINAL: {dto.SaldoFinal:N2}").Bold();
        });

        return doc.GeneratePdf();
    }
}
