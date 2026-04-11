namespace AgoraHub360.ERP.Shared.Utils;

using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Security;

public static class ExportFormatHelper
{
    public static string GenerarJsonPlano(List<AsientoContableDto> data)
    {
        var rows = new List<Dictionary<string, object?>>();
        foreach (var a in data)
        {
            foreach (var l in a.Lineas)
            {
                rows.Add(new Dictionary<string, object?>
                {
                    ["Numero"]          = a.Numero,
                    ["Fecha"]           = a.Fecha.ToString("yyyy-MM-dd"),
                    ["Gestion"]         = a.Gestion,
                    ["TipoComprobante"] = a.TipoComprobanteCodigo,
                    ["Estado"]          = a.Estado,
                    ["Concepto"]        = a.Concepto,
                    ["Glosa"]           = a.Glosa,
                    ["NumLinea"]        = l.NumeroLinea,
                    ["CuentaCodigo"]    = l.CuentaCodigo,
                    ["CuentaNombre"]    = l.CuentaNombre,
                    ["GlosaLinea"]      = l.Glosa,
                    ["CentroCosto"]     = l.CentroCostoCodigo,
                    ["Debe"]            = l.Debe,
                    ["Haber"]           = l.Haber
                });
            }
        }
        return JsonSerializer.Serialize(rows,
            new JsonSerializerOptions { WriteIndented = true });
    }

    public static string GenerarCsvListado(List<AsientoContableDto> data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Numero,Fecha,Gestion,TipoComprobante,Estado,Concepto,Glosa,NumLinea,CuentaCodigo,CuentaNombre,GlosaLinea,CentroCosto,Debe,Haber");
        foreach (var a in data)
        {
            foreach (var l in a.Lineas)
            {
                static string Esc(string? s) => $"\"{(s ?? "").Replace("\"", "\"\"")}\"";
                sb.AppendLine(string.Join(",",
                    Esc(a.Numero),
                    Esc(a.Fecha.ToString("yyyy-MM-dd")),
                    a.Gestion,
                    Esc(a.TipoComprobanteCodigo),
                    Esc(a.Estado),
                    Esc(a.Concepto),
                    Esc(a.Glosa),
                    l.NumeroLinea,
                    Esc(l.CuentaCodigo),
                    Esc(l.CuentaNombre),
                    Esc(l.Glosa),
                    Esc(l.CentroCostoCodigo),
                    l.Debe.ToString("F2"),
                    l.Haber.ToString("F2")));
            }
        }
        return sb.ToString();
    }

    public static string GenerarXmlPlano(List<AsientoContableDto> data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<Comprobantes>");
        foreach (var a in data)
        {
            foreach (var l in a.Lineas)
            {
                sb.AppendLine("  <Linea>");
                sb.AppendLine($"    <Numero>{SecurityElement.Escape(a.Numero)}</Numero>");
                sb.AppendLine($"    <Fecha>{a.Fecha:yyyy-MM-dd}</Fecha>");
                sb.AppendLine($"    <Gestion>{a.Gestion}</Gestion>");
                sb.AppendLine($"    <TipoComprobante>{SecurityElement.Escape(a.TipoComprobanteCodigo)}</TipoComprobante>");
                sb.AppendLine($"    <Estado>{SecurityElement.Escape(a.Estado)}</Estado>");
                sb.AppendLine($"    <Concepto>{SecurityElement.Escape(a.Concepto ?? "")}</Concepto>");
                sb.AppendLine($"    <Glosa>{SecurityElement.Escape(a.Glosa)}</Glosa>");
                sb.AppendLine($"    <NumLinea>{l.NumeroLinea}</NumLinea>");
                sb.AppendLine($"    <CuentaCodigo>{SecurityElement.Escape(l.CuentaCodigo)}</CuentaCodigo>");
                sb.AppendLine($"    <CuentaNombre>{SecurityElement.Escape(l.CuentaNombre)}</CuentaNombre>");
                sb.AppendLine($"    <GlosaLinea>{SecurityElement.Escape(l.Glosa ?? "")}</GlosaLinea>");
                sb.AppendLine($"    <CentroCosto>{SecurityElement.Escape(l.CentroCostoCodigo ?? "")}</CentroCosto>");
                sb.AppendLine($"    <Debe>{l.Debe:F2}</Debe>");
                sb.AppendLine($"    <Haber>{l.Haber:F2}</Haber>");
                sb.AppendLine("  </Linea>");
            }
        }
        sb.AppendLine("</Comprobantes>");
        return sb.ToString();
    }
}
