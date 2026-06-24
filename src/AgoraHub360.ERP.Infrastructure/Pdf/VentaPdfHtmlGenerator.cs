using AgoraHub360.ERP.Shared.DTOs.Reportes;
using System.Text;

namespace AgoraHub360.ERP.Infrastructure.Pdf;

/// <summary>
/// Genera el HTML de una NOTA DE VENTA comercial para convertir a PDF.
/// Diseño minimalista, corporativo, simétrico, formato Carta.
/// </summary>
public static class VentaPdfHtmlGenerator
{
    public static string Generar(ReporteVentaDto dto)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
        sb.AppendLine("<style>");
        sb.AppendLine("@page { size: letter; margin: 12mm; }");
        sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; font-size: 10pt; color: #222; margin:0; padding:0; }");
        sb.AppendLine("h1 { text-align:center; font-size:16pt; font-weight:600; letter-spacing:1px; color:#1a1a2e; margin-bottom:4px; }");
        sb.AppendLine(".subtitle { text-align:center; font-size:9pt; color:#666; margin-bottom:16px; }");
        sb.AppendLine("table { width:100%; border-collapse:collapse; table-layout:fixed; font-size:9pt; }");
        sb.AppendLine("th { background:#f0f0f0; padding:5px 3px; text-align:left; border-bottom:1px solid #ccc; font-weight:600; }");
        sb.AppendLine("td { padding:4px 3px; border-bottom:1px solid #eee; }");
        sb.AppendLine(".num { text-align:center; width:6%; }");
        sb.AppendLine(".sku { text-align:left; width:13%; }");
        sb.AppendLine(".prod { text-align:left; width:28%; }");
        sb.AppendLine(".det { text-align:left; width:22%; }");
        sb.AppendLine(".cant { text-align:right; width:8%; }");
        sb.AppendLine(".punit { text-align:right; width:9%; }");
        sb.AppendLine(".desc { text-align:right; width:7%; }");
        sb.AppendLine(".total { text-align:right; width:7%; font-weight:600; }");
        sb.AppendLine(".info { display:flex; justify-content:space-between; margin-bottom:12px; font-size:9pt; line-height:1.6; }");
        sb.AppendLine(".info div { width:48%; }");
        sb.AppendLine(".info .right { text-align:right; }");
        sb.AppendLine(".footer { text-align:center; font-size:7.5pt; color:#999; border-top:1px solid #ddd; padding-top:8px; margin-top:16px; }");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine("<h1>NOTA DE VENTA</h1>");
        sb.AppendLine($"<div class='subtitle'>{(string.IsNullOrWhiteSpace(dto.NumeroVenta) ? $"#{dto.NumeroVenta}" : dto.NumeroVenta)}</div>");

        // Info cliente
        sb.AppendLine("<div class='info'>");
        sb.AppendLine($"<div><strong>Cliente:</strong> {dto.ClienteNombre}<br/><strong>NIT:</strong> {dto.ClienteNit}</div>");
        sb.AppendLine($"<div class='right'><strong>Fecha:</strong> {dto.FechaVenta:dd/MM/yyyy}<br/><strong>Vto. Pago:</strong> {(dto.FechaVencimientoPago?.ToString("dd/MM/yyyy") ?? "-")}<br/><strong>Moneda:</strong> {dto.MonedaCodigo} TC: {dto.TipoCambio:N4}</div>");
        sb.AppendLine("</div>");

        // Tabla detalle
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th class='num'>#</th><th class='sku'>SKU</th><th class='prod'>Producto / Servicio</th><th class='det'>Detalle</th><th class='cant'>Cant.</th><th class='punit'>P.Unit</th><th class='desc'>Desc.</th><th class='total'>Total</th></tr>");
        var i = 0;
        foreach (var item in dto.Items)
        {
            i++;
            var detalle = item.DetalleAdicional ?? "";
            sb.AppendLine($"<tr><td class='num'>{i}</td><td class='sku'>{item.Sku}</td><td class='prod'>{item.Descripcion}</td><td class='det'>{detalle}</td><td class='cant'>{item.Cantidad:N2}</td><td class='punit'>{item.PrecioUnitario:N2}</td><td class='desc'>{(item.DescuentoPorcentaje > 0 ? (item.Cantidad * item.PrecioUnitario * item.DescuentoPorcentaje / 100m + item.DescuentoMonto):0):N2}</td><td class='total'>{item.TotalLinea:N2}</td></tr>");
        }
        sb.AppendLine("</table>");

        // Resumen
        sb.AppendLine("<div style='text-align:right; margin-top:8px; font-size:10pt;'>");
        sb.AppendLine($"<div style='margin-bottom:2px;'><strong>Subtotal:</strong> {dto.Subtotal:N2}</div>");
        sb.AppendLine($"<div style='margin-bottom:2px;'><strong>Descuento:</strong> {dto.DescuentoTotal:N2}</div>");
        sb.AppendLine($"<div style='font-weight:700; font-size:11pt; margin-top:4px; border-top:2px solid #1a1a2e; padding-top:4px;'><strong>TOTAL:</strong> {dto.Total:N2}</div>");
        sb.AppendLine("</div>");

        // Observaciones
        if (!string.IsNullOrWhiteSpace(dto.Observaciones))
        {
            sb.AppendLine($"<div style='margin-top:8px; font-size:8.5pt; color:#555; border-top:1px dashed #ccc; padding-top:6px;'><strong>Observaciones:</strong> {dto.Observaciones}</div>");
        }

        // Pie
        sb.AppendLine($"<div class='footer'>Documento generado por AgoraHUB360 ERP — {DateTime.Now:dd/MM/yyyy HH:mm}</div>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }
}
