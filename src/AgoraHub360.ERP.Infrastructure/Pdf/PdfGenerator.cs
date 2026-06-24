using AgoraHub360.ERP.Shared.DTOs.Reportes;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AgoraHub360.ERP.Infrastructure.Pdf;

/// <summary>
/// Generador de documentos PDF usando QuestPDF.
/// Todos los reportes usan estilos corporativos consistentes.
/// </summary>
public static class PdfGenerator
{
    private static readonly Color PrimaryColor = Colors.Blue.Darken3;
    private static readonly Color AccentColor = Colors.Blue.Medium;
    private static readonly Color HeaderBg = Colors.Grey.Lighten4;
    private static readonly Color BorderColor = Colors.Grey.Lighten2;

    // ─── Estilos base ───────────────────────────────────────────────────────

    private static void EncabezadoEmpresa(PageDescriptor page, string nombre, string? nit, string? dir, string? tel, string? email)
    {
        page.Header().Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(nombre).FontSize(16).Bold().FontColor(PrimaryColor);
                    if (!string.IsNullOrEmpty(nit))
                        c.Item().Text($"NIT: {nit}").FontSize(9).FontColor(Colors.Grey.Darken2);
                    if (!string.IsNullOrEmpty(dir))
                        c.Item().Text($"Dir: {dir}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    if (!string.IsNullOrEmpty(tel))
                        c.Item().Text($"Tel: {tel} | Email: {email}").FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
            col.Item().PaddingVertical(6).LineHorizontal(1).LineColor(PrimaryColor);
        });
    }

    private static void PiePagina(PageDescriptor page)
    {
        page.Footer().Column(col =>
        {
            col.Item().PaddingTop(4).LineHorizontal(1).LineColor(BorderColor);
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().AlignLeft().Text("AgoraHUB360 ERP — Documento generado electrónicamente")
                    .FontSize(7).FontColor(Colors.Grey.Darken1);
                row.AutoItem().AlignRight().Text(x =>
                {
                    x.Span("Pág. ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                    x.Span(" de ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    x.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
        });
    }

    private static void TituloDocumento(ColumnDescriptor col, string titulo, string? subtitulo = null)
    {
        col.Item().PaddingBottom(8).Column(c =>
        {
            c.Item().AlignCenter().Text(titulo).FontSize(14).Bold().FontColor(PrimaryColor);
            if (!string.IsNullOrEmpty(subtitulo))
                c.Item().AlignCenter().Text(subtitulo).FontSize(10).FontColor(Colors.Grey.Darken2);
        });
    }

    private static string Moneda(decimal monto, string? moneda = "BOB") => $"{monto:N2} {moneda}";

    private static LabelValueCellStyle LabelStyle => new()
    {
        LabelFontSize = 8,
        ValueFontSize = 10,
        LabelColor = Colors.Grey.Darken2,
        ValueColor = Colors.Black
    };

    // ─── Recibo de Pago ────────────────────────────────────────────────────

    public static byte[] GenerarReciboPago(ReciboPagoDto dto)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                EncabezadoEmpresa(page, dto.EmpresaNombre, dto.EmpresaNit, dto.EmpresaDireccion, dto.EmpresaTelefono, dto.EmpresaEmail);

                page.Content().Column(col =>
                {
                    TituloDocumento(col, "RECIBO DE PAGO", $"N° {dto.NumeroRecibo}");

                    // Datos del cliente
                    col.Item().Background(HeaderBg).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Cliente: {dto.ClienteNombre ?? "N/A"}").FontSize(11).Bold();
                            c.Item().Text($"NIT: {dto.ClienteNit ?? "N/A"}").FontSize(9).FontColor(Colors.Grey.Darken2);
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text($"Fecha: {dto.FechaPago:dd/MM/yyyy HH:mm}").FontSize(10);
                            c.Item().Text($"Factura: {dto.NumeroFactura}").FontSize(9).FontColor(Colors.Grey.Darken2);
                        });
                    });

                    col.Item().PaddingVertical(8);

                    // Detalle del pago
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Background(PrimaryColor).Padding(4).Text("Concepto").FontSize(9).Bold().FontColor(Colors.White);
                            header.Cell().Background(PrimaryColor).Padding(4).Text("Método Pago").FontSize(9).Bold().FontColor(Colors.White);
                            header.Cell().Background(PrimaryColor).Padding(4).Text("Referencia").FontSize(9).Bold().FontColor(Colors.White);
                            header.Cell().Background(PrimaryColor).Padding(4).AlignRight().Text("Monto").FontSize(9).Bold().FontColor(Colors.White);
                        });

                        // Data row
                        table.Cell().Padding(4).Text("Pago registrado").FontSize(9);
                        table.Cell().Padding(4).Text(dto.MetodoPago).FontSize(9);
                        table.Cell().Padding(4).Text(dto.Referencia ?? "-").FontSize(9);
                        table.Cell().Padding(4).AlignRight().Text(Moneda(dto.MontoPagado, dto.MonedaCodigo)).FontSize(9).Bold();
                    });

                    col.Item().PaddingVertical(8);

                    // Saldos
                    col.Item().Background(HeaderBg).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Saldo Anterior:").FontSize(9).FontColor(Colors.Grey.Darken2);
                            c.Item().Text(Moneda(dto.SaldoAnterior, dto.MonedaCodigo)).FontSize(12).Bold();
                        });
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("Monto Pagado:").FontSize(9).FontColor(Colors.Grey.Darken2);
                            c.Item().Text(Moneda(dto.MontoPagado, dto.MonedaCodigo)).FontSize(12).Bold().FontColor(Colors.Green.Darken2);
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("Saldo Posterior:").FontSize(9).FontColor(Colors.Grey.Darken2);
                            c.Item().Text(Moneda(dto.SaldoPosterior, dto.MonedaCodigo)).FontSize(12).Bold().FontColor(dto.SaldoPosterior > 0 ? Colors.Red.Darken2 : Colors.Green.Darken2);
                        });
                    });

                    if (!string.IsNullOrEmpty(dto.Observaciones))
                    {
                        col.Item().PaddingTop(8).Column(c =>
                        {
                            c.Item().Text("Observaciones").FontSize(9).Bold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text(dto.Observaciones).FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    }

                    col.Item().PaddingTop(8).AlignRight().Text($"Usuario: {dto.UsuarioRegistro ?? "N/A"}").FontSize(8).FontColor(Colors.Grey.Darken2);
                });

                PiePagina(page);
            });
        }).GeneratePdf();
    }

    // ─── Estado de Cuenta Cliente ───────────────────────────────────────────

    public static byte[] GenerarEstadoCuenta(EstadoCuentaClienteDto dto)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                EncabezadoEmpresa(page, dto.EmpresaNombre, dto.EmpresaNit, dto.EmpresaDireccion, dto.EmpresaTelefono, dto.EmpresaEmail);

                page.Content().Column(col =>
                {
                    var rango = $"{(dto.FechaDesde?.ToString("dd/MM/yyyy") ?? "Inicio")} - {(dto.FechaHasta?.ToString("dd/MM/yyyy") ?? "Actual")}";
                    TituloDocumento(col, "ESTADO DE CUENTA", $"Cliente: {dto.ClienteNombre} — {rango}");

                    // Datos cliente
                    col.Item().Background(HeaderBg).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Cliente: {dto.ClienteNombre}").FontSize(11).Bold();
                            c.Item().Text($"NIT: {dto.ClienteNit ?? "N/A"}").FontSize(9);
                            c.Item().Text($"Dirección: {dto.ClienteDireccion ?? "N/A"}").FontSize(9);
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Teléfono: {dto.ClienteTelefono ?? "N/A"}").FontSize(9);
                            if (dto.FechaDesde.HasValue)
                                c.Item().Text($"Desde: {dto.FechaDesde:dd/MM/yyyy}").FontSize(9);
                            if (dto.FechaHasta.HasValue)
                                c.Item().Text($"Hasta: {dto.FechaHasta:dd/MM/yyyy}").FontSize(9);
                        });
                    });

                    col.Item().PaddingVertical(6);

                    // Resumen cards
                    col.Item().Row(row =>
                    {
                        void AddCard(string label, decimal value, string color)
                        {
                            row.RelativeItem().Padding(2).Background(HeaderBg).Padding(6).Column(c =>
                            {
                                c.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken2);
                                c.Item().Text(Moneda(value, dto.MonedaCodigo)).FontSize(12).Bold().FontColor(color);
                            });
                        }
                        AddCard("Total Facturado", dto.TotalFacturado, Colors.Black);
                        AddCard("Total Pagado", dto.TotalPagado, Colors.Green.Darken2);
                        AddCard("Saldo Pendiente", dto.SaldoPendiente, Colors.Red.Darken2);
                        AddCard("Saldo Vencido", dto.SaldoVencido, Colors.Red.Darken4);
                    });

                    col.Item().PaddingVertical(8);

                    // Tabla de documentos
                    col.Item().Text("Detalle de Documentos").FontSize(11).Bold().FontColor(PrimaryColor);
                    col.Item().PaddingTop(4);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(PrimaryColor).Padding(3).Text("Factura").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).Text("Emisión").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).Text("Vencimiento").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).AlignRight().Text("Original").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).AlignRight().Text("Pagado").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).AlignRight().Text("Saldo").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).AlignCenter().Text("Estado").FontSize(8).Bold().FontColor(Colors.White);
                        });

                        foreach (var doc in dto.Documentos)
                        {
                            var bgColor = doc.DiasVencidos > 0 ? Colors.Red.Lighten5 : Colors.White;
                            table.Cell().Background(bgColor).Padding(3).Text(doc.NumeroFactura).FontSize(8);
                            table.Cell().Background(bgColor).Padding(3).Text(doc.FechaEmision.ToString("dd/MM/yyyy")).FontSize(8);
                            table.Cell().Background(bgColor).Padding(3).Text(doc.FechaVencimiento?.ToString("dd/MM/yyyy") ?? "-").FontSize(8);
                            table.Cell().Background(bgColor).Padding(3).AlignRight().Text(Moneda(doc.MontoOriginal)).FontSize(8);
                            table.Cell().Background(bgColor).Padding(3).AlignRight().Text(Moneda(doc.MontoPagado)).FontSize(8);
                            table.Cell().Background(bgColor).Padding(3).AlignRight().Text(Moneda(doc.Saldo)).FontSize(8).Bold();
                            table.Cell().Background(bgColor).Padding(3).AlignCenter().Text(doc.Estado).FontSize(8);
                        }
                    });

                    // Pagos si aplica
                    if (dto.Pagos?.Count > 0)
                    {
                        col.Item().PaddingTop(8);
                        col.Item().Text("Detalle de Pagos").FontSize(11).Bold().FontColor(PrimaryColor);
                        col.Item().PaddingTop(4);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn(2);
                                c.RelativeColumn();
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background(PrimaryColor).Padding(3).Text("Fecha").FontSize(8).Bold().FontColor(Colors.White);
                                h.Cell().Background(PrimaryColor).Padding(3).Text("Factura").FontSize(8).Bold().FontColor(Colors.White);
                                h.Cell().Background(PrimaryColor).Padding(3).AlignRight().Text("Monto").FontSize(8).Bold().FontColor(Colors.White);
                                h.Cell().Background(PrimaryColor).Padding(3).Text("Método").FontSize(8).Bold().FontColor(Colors.White);
                                h.Cell().Background(PrimaryColor).Padding(3).Text("Referencia").FontSize(8).Bold().FontColor(Colors.White);
                            });

                            foreach (var pago in dto.Pagos)
                            {
                                table.Cell().Padding(3).Text(pago.FechaPago.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
                                table.Cell().Padding(3).Text(pago.NumeroFactura).FontSize(8);
                                table.Cell().Padding(3).AlignRight().Text(Moneda(pago.Monto)).FontSize(8).Bold();
                                table.Cell().Padding(3).Text(pago.MetodoPago).FontSize(8);
                                table.Cell().Padding(3).Text(pago.Referencia ?? "-").FontSize(8);
                            }
                        });
                    }
                });

                PiePagina(page);
            });
        }).GeneratePdf();
    }

    // ─── Factura PDF ───────────────────────────────────────────────────────

    public static byte[] GenerarFactura(FacturaPdfDto dto)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                EncabezadoEmpresa(page, dto.EmpresaNombre, dto.EmpresaNit, dto.EmpresaDireccion, dto.EmpresaTelefono, dto.EmpresaEmail);

                page.Content().Column(col =>
                {
                    TituloDocumento(col, "FACTURA", $"N° {dto.NumeroFactura}");

                    // Datos cliente y factura
                    col.Item().Background(HeaderBg).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("DATOS DEL CLIENTE").FontSize(9).Bold().FontColor(PrimaryColor);
                            c.Item().Text($"Cliente: {dto.ClienteNombre}").FontSize(10).Bold();
                            c.Item().Text($"NIT/CI: {dto.ClienteNit}").FontSize(9);
                            if (!string.IsNullOrEmpty(dto.ClienteDireccion))
                                c.Item().Text($"Dirección: {dto.ClienteDireccion}").FontSize(9);
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("DATOS DE LA FACTURA").FontSize(9).Bold().FontColor(PrimaryColor);
                            c.Item().Text($"N° {dto.NumeroFactura}").FontSize(10).Bold();
                            c.Item().Text($"Fecha: {dto.FechaEmision:dd/MM/yyyy}").FontSize(9);
                            c.Item().Text($"Moneda: {dto.MonedaCodigo} | TC: {dto.TipoCambio:N4}").FontSize(9);
                            if (!string.IsNullOrEmpty(dto.NumeroAutorizacion))
                                c.Item().Text($"Aut.: {dto.NumeroAutorizacion}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().PaddingTop(4).Text($"Estado: {dto.EstadoFactura}").FontSize(9).Bold().FontColor(
                                dto.EstadoFactura.Contains("Emitida", StringComparison.OrdinalIgnoreCase) ? Colors.Green.Darken2 :
                                dto.EstadoFactura.Contains("Anulada", StringComparison.OrdinalIgnoreCase) ? Colors.Red.Darken2 :
                                Colors.Orange.Darken2);
                        });
                    });

                    col.Item().PaddingVertical(6);

                    // Tabla de productos
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(4);
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(PrimaryColor).Padding(3).Text("Producto/Servicio").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).AlignRight().Text("Cant.").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).AlignRight().Text("P. Unit.").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).AlignRight().Text("Dscto.").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).AlignRight().Text("Imp.").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).AlignRight().Text("Total").FontSize(8).Bold().FontColor(Colors.White);
                        });

                        foreach (var det in dto.Detalles)
                        {
                            table.Cell().Padding(3).Text(det.Descripcion).FontSize(8);
                            table.Cell().Padding(3).AlignRight().Text($"{det.Cantidad:N2} {det.UnidadMedida}").FontSize(8);
                            table.Cell().Padding(3).AlignRight().Text(Moneda(det.PrecioUnitario)).FontSize(8);
                            table.Cell().Padding(3).AlignRight().Text(Moneda(det.DescuentoMonto)).FontSize(8);
                            table.Cell().Padding(3).AlignRight().Text(Moneda(det.ImpuestoMonto)).FontSize(8);
                            table.Cell().Padding(3).AlignRight().Text(Moneda(det.TotalLinea)).FontSize(8).Bold();
                        }
                    });

                    col.Item().PaddingVertical(6);

                    // Totales
                    col.Item().AlignRight().Table(totalTable =>
                    {
                        totalTable.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn();
                        });

                        void AddTotalRow(string label, decimal value, bool bold = false, string? color = null)
                        {
                            totalTable.Cell().Padding(2).AlignRight().Text(label).FontSize(9).Bold();
                            totalTable.Cell().Padding(2).AlignRight().Text(Moneda(value, dto.MonedaCodigo))
                                .FontSize(bold ? 11 : 9);
                            if (bold)
                                totalTable.Cell().Padding(2).AlignRight().Text(Moneda(value, dto.MonedaCodigo))
                                    .FontSize(11).Bold().FontColor(color ?? Colors.Black);
                            else
                                totalTable.Cell().Padding(2).AlignRight().Text(Moneda(value, dto.MonedaCodigo))
                                    .FontSize(9).FontColor(color ?? Colors.Black);
                        }

                        AddTotalRow("Subtotal:", dto.Subtotal, false);
                        if (dto.DescuentoTotal > 0)
                            AddTotalRow("Descuento:", dto.DescuentoTotal, color: Colors.Red.Darken2);
                        AddTotalRow("Impuestos:", dto.ImpuestoTotal);
                        AddTotalRow("TOTAL:", dto.Total, bold: true, color: PrimaryColor);
                    });

                    // Datos SIAT
                    if (!string.IsNullOrEmpty(dto.Cuf) || !string.IsNullOrEmpty(dto.Leyenda))
                    {
                        col.Item().PaddingTop(8).Background(HeaderBg).Padding(8).Column(siat =>
                        {
                            siat.Item().Text("DATOS DE FACTURACIÓN ELECTRÓNICA").FontSize(9).Bold().FontColor(PrimaryColor);
                            if (!string.IsNullOrEmpty(dto.Cuf))
                                siat.Item().Text($"CUF: {dto.Cuf}").FontSize(7).FontColor(Colors.Grey.Darken2);
                            if (!string.IsNullOrEmpty(dto.Cufd))
                                siat.Item().Text($"CUFD: {dto.Cufd}").FontSize(7).FontColor(Colors.Grey.Darken2);
                            if (!string.IsNullOrEmpty(dto.Leyenda))
                                siat.Item().PaddingTop(4).Text(dto.Leyenda).FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                            siat.Item().Text($"Estado SIAT: {dto.EstadoSiat}").FontSize(8).FontColor(Colors.Grey.Darken2);
                        });
                    }

                    if (!string.IsNullOrEmpty(dto.Observaciones))
                    {
                        col.Item().PaddingTop(8).Column(c =>
                        {
                            c.Item().Text("Observaciones").FontSize(9).Bold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text(dto.Observaciones).FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    }
                });

                PiePagina(page);
            });
        }).GeneratePdf();
    }

    // ─── Reporte General CxC ───────────────────────────────────────────────

    public static byte[] GenerarReporteCxcGeneral(ReporteCxcGeneralDto dto)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                EncabezadoEmpresa(page, dto.EmpresaNombre, dto.EmpresaNit, null, null, null);

                page.Content().Column(col =>
                {
                    TituloDocumento(col, "REPORTE GENERAL DE CUENTAS POR COBRAR");

                    // Resumen cards
                    col.Item().Row(row =>
                    {
                        void AddCard(string label, string value, string color)
                        {
                            row.RelativeItem().Padding(2).Background(HeaderBg).Padding(6).Column(c =>
                            {
                                c.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken2);
                                c.Item().Text(value).FontSize(11).Bold().FontColor(color);
                            });
                        }
                        AddCard("Total Cuentas", dto.TotalCuentas.ToString(), Colors.Black);
                        AddCard("Pendientes", dto.CuentasPendientes.ToString(), Colors.Orange.Darken2);
                        AddCard("Vencidas", dto.CuentasVencidas.ToString(), Colors.Red.Darken2);
                        AddCard("Pagadas", dto.CuentasPagadas.ToString(), Colors.Green.Darken2);
                        AddCard("Saldo Pendiente", Moneda(dto.TotalSaldoPendiente, dto.MonedaCodigo), PrimaryColor);
                    });

                    col.Item().PaddingVertical(6);

                    // Tabla
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(PrimaryColor).Padding(2).Text("Cliente").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(2).Text("Factura").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(2).Text("Emisión").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(2).Text("Vencimiento").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(2).AlignRight().Text("Original").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(2).AlignRight().Text("Pagado").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(2).AlignRight().Text("Saldo").FontSize(7).Bold().FontColor(Colors.White);
                        });

                        foreach (var item in dto.Items)
                        {
                            var isVencida = item.FechaVencimiento.HasValue && item.FechaVencimiento.Value < DateTime.Today && item.SaldoPendiente > 0;
                            var bgColor = isVencida ? Colors.Red.Lighten5 : Colors.White;
                            table.Cell().Background(bgColor).Padding(2).Text(item.ClienteNombre ?? "N/A").FontSize(7);
                            table.Cell().Background(bgColor).Padding(2).Text(item.NumeroFactura).FontSize(7);
                            table.Cell().Background(bgColor).Padding(2).Text(item.FechaEmision.ToString("dd/MM/yyyy")).FontSize(7);
                            table.Cell().Background(bgColor).Padding(2).Text(item.FechaVencimiento?.ToString("dd/MM/yyyy") ?? "-").FontSize(7);
                            table.Cell().Background(bgColor).Padding(2).AlignRight().Text(Moneda(item.MontoOriginal)).FontSize(7);
                            table.Cell().Background(bgColor).Padding(2).AlignRight().Text(Moneda(item.MontoPagado)).FontSize(7);
                            table.Cell().Background(bgColor).Padding(2).AlignRight().Text(Moneda(item.SaldoPendiente)).FontSize(7).Bold();
                        }
                    });

                    // Totales por estado
                    col.Item().PaddingTop(8);
                    col.Item().Text("Totales por Estado").FontSize(10).Bold().FontColor(PrimaryColor);
                    col.Item().PaddingTop(4);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(PrimaryColor).Padding(3).Text("Estado").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).AlignRight().Text("Cantidad").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).AlignRight().Text("Total Saldo").FontSize(8).Bold().FontColor(Colors.White);
                        });

                        foreach (var t in dto.TotalesPorEstado)
                        {
                            table.Cell().Padding(3).Text(t.Estado).FontSize(8);
                            table.Cell().Padding(3).AlignRight().Text(t.Cantidad.ToString()).FontSize(8);
                            table.Cell().Padding(3).AlignRight().Text(Moneda(t.TotalSaldo)).FontSize(8);
                        }
                    });
                });

                PiePagina(page);
            });
        }).GeneratePdf();
    }

    // ─── Reporte CxC Vencidas ──────────────────────────────────────────────

    public static byte[] GenerarReporteCxcVencidas(ReporteCxcVencidasDto dto)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                EncabezadoEmpresa(page, dto.EmpresaNombre, dto.EmpresaNit, null, null, null);

                page.Content().Column(col =>
                {
                    TituloDocumento(col, "REPORTE DE CUENTAS POR COBRAR VENCIDAS", $"Al {dto.FechaCorte:dd/MM/yyyy}");

                    // Resumen
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Padding(2).Background(Colors.Red.Lighten5).Padding(6).Column(c =>
                        {
                            c.Item().Text("Total Vencidas").FontSize(9).FontColor(Colors.Red.Darken2);
                            c.Item().Text(dto.TotalVencidas.ToString()).FontSize(16).Bold().FontColor(Colors.Red.Darken4);
                        });
                        row.RelativeItem().Padding(2).Background(Colors.Red.Lighten5).Padding(6).Column(c =>
                        {
                            c.Item().Text("Saldo Vencido Total").FontSize(9).FontColor(Colors.Red.Darken2);
                            c.Item().Text(Moneda(dto.TotalSaldoVencido, dto.MonedaCodigo)).FontSize(16).Bold().FontColor(Colors.Red.Darken4);
                        });
                        row.RelativeItem().Padding(2).Background(HeaderBg).Padding(6).Column(c =>
                        {
                            c.Item().Text("Fecha de Corte").FontSize(9).FontColor(Colors.Grey.Darken2);
                            c.Item().Text(dto.FechaCorte.ToString("dd/MM/yyyy")).FontSize(14).Bold();
                        });
                    });

                    col.Item().PaddingVertical(6);

                    // Tabla detalle
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(Colors.Red.Darken3).Padding(2).Text("Cliente").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(Colors.Red.Darken3).Padding(2).Text("Documento").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(Colors.Red.Darken3).Padding(2).Text("Emisión").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(Colors.Red.Darken3).Padding(2).Text("Vencimiento").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(Colors.Red.Darken3).Padding(2).AlignRight().Text("Días Venc.").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(Colors.Red.Darken3).Padding(2).AlignRight().Text("Original").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(Colors.Red.Darken3).Padding(2).AlignRight().Text("Pagado").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(Colors.Red.Darken3).Padding(2).AlignRight().Text("Saldo").FontSize(7).Bold().FontColor(Colors.White);
                        });

                        foreach (var item in dto.Items)
                        {
                            var bgColor = item.DiasVencidos > 90 ? Colors.Red.Lighten4 : item.DiasVencidos > 60 ? Colors.Orange.Lighten4 : Colors.Yellow.Lighten4;
                            table.Cell().Background(bgColor).Padding(2).Text(item.ClienteNombre ?? "N/A").FontSize(7);
                            table.Cell().Background(bgColor).Padding(2).Text(item.NumeroFactura).FontSize(7);
                            table.Cell().Background(bgColor).Padding(2).Text(item.FechaEmision.ToString("dd/MM/yyyy")).FontSize(7);
                            table.Cell().Background(bgColor).Padding(2).Text(item.FechaVencimiento?.ToString("dd/MM/yyyy") ?? "-").FontSize(7);
                            table.Cell().Background(bgColor).Padding(2).AlignRight().Text(item.DiasVencidos.ToString()).FontSize(7).Bold().FontColor(Colors.Red.Darken2);
                            table.Cell().Background(bgColor).Padding(2).AlignRight().Text(Moneda(item.MontoOriginal)).FontSize(7);
                            table.Cell().Background(bgColor).Padding(2).AlignRight().Text(Moneda(item.MontoPagado)).FontSize(7);
                            table.Cell().Background(bgColor).Padding(2).AlignRight().Text(Moneda(item.SaldoPendiente)).FontSize(7).Bold();
                        }
                    });
                });

                PiePagina(page);
            });
        }).GeneratePdf();
    }

    public static byte[] GenerarVentaPdf(ReporteVentaDto dto)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1.8f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                EncabezadoEmpresa(page, dto.EmpresaNombre, dto.EmpresaNit, dto.EmpresaDireccion, dto.EmpresaTelefono, dto.EmpresaEmail);

                page.Content().Column(col =>
                {
                    TituloDocumento(col, "NOTA DE VENTA", dto.NumeroVenta);

                    // Bloque cliente + datos venta (2 columnas)
                    col.Item().Background(HeaderBg).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("CLIENTE").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                            c.Item().PaddingTop(2).Text(dto.ClienteNombre).FontSize(11).Bold();
                            c.Item().Text($"NIT: {dto.ClienteNit}").FontSize(9).FontColor(Colors.Grey.Darken2);
                            if (!string.IsNullOrEmpty(dto.ClienteDireccion))
                                c.Item().Text($"Dir: {dto.ClienteDireccion}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            if (!string.IsNullOrEmpty(dto.ClienteTelefono))
                                c.Item().Text($"Tel: {dto.ClienteTelefono}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("VENTA").FontSize(8).Bold().FontColor(Colors.Grey.Darken2).AlignRight();
                            c.Item().PaddingTop(2).Text($"Fecha: {dto.FechaVenta:dd/MM/yyyy}").FontSize(10).AlignRight();
                            c.Item().Text($"Vto. Pago: {(dto.FechaVencimientoPago?.ToString("dd/MM/yyyy") ?? "-")}").FontSize(9).FontColor(Colors.Grey.Darken2).AlignRight();
                            c.Item().Text($"Moneda: {dto.MonedaCodigo}  TC: {dto.TipoCambio:N4}").FontSize(9).FontColor(Colors.Grey.Darken2).AlignRight();
                            if (!string.IsNullOrEmpty(dto.SucursalNombre))
                                c.Item().Text($"Sucursal: {dto.SucursalNombre}").FontSize(8).FontColor(Colors.Grey.Darken1).AlignRight();
                            if (!string.IsNullOrEmpty(dto.AlmacenNombre))
                                c.Item().Text($"Almacén: {dto.AlmacenNombre}").FontSize(8).FontColor(Colors.Grey.Darken1).AlignRight();
                        });
                    });

                    col.Item().PaddingVertical(6);

                    // Tabla detalle
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(18);
                            c.RelativeColumn(2);
                            c.RelativeColumn(5);
                            c.RelativeColumn(3);
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(PrimaryColor).Padding(3).Text("#").FontSize(8).Bold().FontColor(Colors.White).AlignCenter();
                            h.Cell().Background(PrimaryColor).Padding(3).Text("SKU").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).Text("Producto / Servicio").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).Text("Detalle").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).AlignRight().Text("Cant.").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).AlignRight().Text("P.Unit").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).AlignRight().Text("Desc.").FontSize(8).Bold().FontColor(Colors.White);
                            h.Cell().Background(PrimaryColor).Padding(3).AlignRight().Text("Total").FontSize(8).Bold().FontColor(Colors.White);
                        });

                        var i = 0;
                        foreach (var item in dto.Items)
                        {
                            i++;
                            var bgColor = i % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                            var desc = item.DescuentoPorcentaje > 0
                                ? (item.Cantidad * item.PrecioUnitario * item.DescuentoPorcentaje / 100m + item.DescuentoMonto)
                                : item.DescuentoMonto;
                            table.Cell().Background(bgColor).Padding(2).Text(i.ToString()).FontSize(8).AlignCenter();
                            table.Cell().Background(bgColor).Padding(2).Text(item.Sku).FontSize(8);
                            table.Cell().Background(bgColor).Padding(2).Text(item.Descripcion).FontSize(8);
                            table.Cell().Background(bgColor).Padding(2).Text(item.DetalleAdicional ?? "").FontSize(8);
                            table.Cell().Background(bgColor).Padding(2).AlignRight().Text(item.Cantidad.ToString("N2")).FontSize(8);
                            table.Cell().Background(bgColor).Padding(2).AlignRight().Text(item.PrecioUnitario.ToString("N2")).FontSize(8);
                            table.Cell().Background(bgColor).Padding(2).AlignRight().Text(desc.ToString("N2")).FontSize(8);
                            table.Cell().Background(bgColor).Padding(2).AlignRight().Text(item.TotalLinea.ToString("N2")).FontSize(8).Bold();
                        }
                    });

                    col.Item().PaddingVertical(6);

                    // Resumen alineado a la derecha
                    col.Item().AlignRight().Column(colR =>
                    {
                        colR.Item().Background(HeaderBg).Padding(8).Table(tab =>
                        {
                            tab.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(); });
                            void AddRow(string label, string value, bool isBold = false)
                            {
                                tab.Cell().Padding(2).AlignRight().Text(label).FontSize(10);
                                if (isBold)
                                    tab.Cell().Padding(2).AlignRight().Text(value).FontSize(12).Bold().FontColor(PrimaryColor);
                                else
                                    tab.Cell().Padding(2).AlignRight().Text(value).FontSize(10).FontColor(Colors.Black);
                            }
                            AddRow("Subtotal:", Moneda(dto.Subtotal, dto.MonedaCodigo));
                            if (dto.DescuentoTotal > 0)
                                AddRow("Descuento:", Moneda(dto.DescuentoTotal, dto.MonedaCodigo));
                            AddRow("TOTAL:", Moneda(dto.Total, dto.MonedaCodigo), true);
                        });
                    });

                    // Observaciones
                    if (!string.IsNullOrWhiteSpace(dto.Observaciones))
                    {
                        col.Item().PaddingTop(8).Column(c =>
                        {
                            c.Item().Text("Observaciones").FontSize(9).Bold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text(dto.Observaciones).FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    }
                });

                PiePagina(page);
            });
        }).GeneratePdf();
    }

    private struct LabelValueCellStyle
    {
        public int LabelFontSize { get; set; }
        public int ValueFontSize { get; set; }
        public string LabelColor { get; set; }
        public string ValueColor { get; set; }
    }
}
