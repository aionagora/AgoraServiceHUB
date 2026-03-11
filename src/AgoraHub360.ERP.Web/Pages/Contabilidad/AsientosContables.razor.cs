using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace AgoraHub360.ERP.Web.Pages.Contabilidad;

public partial class AsientosContables
{
    // ?? Generadores planos (una fila/registro por línea contable) ?????????????

    private static string GenerarJsonPlano(List<AsientoContableDto> data)
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
        return System.Text.Json.JsonSerializer.Serialize(rows,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    private static string GenerarCsvListado(List<AsientoContableDto> data)
    {
        var sb = new System.Text.StringBuilder();
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

    private static string GenerarXmlPlano(List<AsientoContableDto> data)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<Comprobantes>");
        foreach (var a in data)
        {
            foreach (var l in a.Lineas)
            {
                sb.AppendLine("  <Linea>");
                sb.AppendLine($"    <Numero>{System.Security.SecurityElement.Escape(a.Numero)}</Numero>");
                sb.AppendLine($"    <Fecha>{a.Fecha:yyyy-MM-dd}</Fecha>");
                sb.AppendLine($"    <Gestion>{a.Gestion}</Gestion>");
                sb.AppendLine($"    <TipoComprobante>{System.Security.SecurityElement.Escape(a.TipoComprobanteCodigo)}</TipoComprobante>");
                sb.AppendLine($"    <Estado>{System.Security.SecurityElement.Escape(a.Estado)}</Estado>");
                sb.AppendLine($"    <Concepto>{System.Security.SecurityElement.Escape(a.Concepto ?? "")}</Concepto>");
                sb.AppendLine($"    <Glosa>{System.Security.SecurityElement.Escape(a.Glosa)}</Glosa>");
                sb.AppendLine($"    <NumLinea>{l.NumeroLinea}</NumLinea>");
                sb.AppendLine($"    <CuentaCodigo>{System.Security.SecurityElement.Escape(l.CuentaCodigo)}</CuentaCodigo>");
                sb.AppendLine($"    <CuentaNombre>{System.Security.SecurityElement.Escape(l.CuentaNombre)}</CuentaNombre>");
                sb.AppendLine($"    <GlosaLinea>{System.Security.SecurityElement.Escape(l.Glosa ?? "")}</GlosaLinea>");
                sb.AppendLine($"    <CentroCosto>{System.Security.SecurityElement.Escape(l.CentroCostoCodigo ?? "")}</CentroCosto>");
                sb.AppendLine($"    <Debe>{l.Debe:F2}</Debe>");
                sb.AppendLine($"    <Haber>{l.Haber:F2}</Haber>");
                sb.AppendLine("  </Linea>");
            }
        }
        sb.AppendLine("</Comprobantes>");
        return sb.ToString();
    }

    // ?? Helpers de presentación ?????????????????????????????????????????????????

    private static string GetEstadoBadge(string estado) => estado switch
    {
        "Contabilizado" => "bg-success",
        "Anulado"       => "bg-danger",
        _               => "bg-warning text-dark"
    };

    private static string GetDocBadgeStyle(string mime) => mime switch
    {
        "application/pdf"   => "background:#fee2e2;color:#b91c1c",
        "image/jpeg"
        or "image/jpg"
        or "image/png"      => "background:#dbeafe;color:#1d4ed8",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        or "application/vnd.ms-excel" => "background:#dcfce7;color:#15803d",
        _                   => "background:#f3f4f6;color:#374151"
    };

    private static string GetDocExtLabel(string mime) => mime switch
    {
        "application/pdf"   => "PDF",
        "image/jpeg"
        or "image/jpg"      => "JPG",
        "image/png"         => "PNG",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => "XLSX",
        "application/vnd.ms-excel" => "XLS",
        _                   => "FILE"
    };

    private static string FormatBytes(long bytes)
    {
        if (bytes >= 1_048_576) return $"{bytes / 1_048_576.0:N1} MB";
        if (bytes >= 1_024)     return $"{bytes / 1_024.0:N0} KB";
        return $"{bytes} B";
    }

    // ?? Exportar listado Excel ????????????????????????????????????????????????

    private async Task ExportarExcel()
    {
        exportando = true; errorMessage = null;
        try
        {
            var tipoId = filtroTipoComp > 0 ? (int?)filtroTipoComp : null;
            var fileBytes = await AsientoService.ExportarExcelAsync(filtroDesde, filtroHasta,
                string.IsNullOrEmpty(filtroEstado) ? null : filtroEstado, tipoId,
                string.IsNullOrEmpty(filtroSearch) ? null : filtroSearch);
            if (fileBytes is { Length: > 0 })
            {
                var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                await JS.InvokeVoidAsync("downloadFile",
                    $"Comprobantes_{ts}.xlsx",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Convert.ToBase64String(fileBytes));
                successMessage = "Archivo Excel generado exitosamente.";
            }
            else errorMessage = "No se pudo generar el archivo Excel.";
        }
        catch (Exception ex) { errorMessage = $"Error al exportar: {ex.Message}"; }
        finally { exportando = false; }
    }

    private async Task ExportarExcelPlano()
    {
        exportando = true; errorMessage = null;
        try
        {
            var tipoId = filtroTipoComp > 0 ? (int?)filtroTipoComp : null;
            var fileBytes = await AsientoService.ExportarExcelPlanoAsync(filtroDesde, filtroHasta,
                string.IsNullOrEmpty(filtroEstado) ? null : filtroEstado, tipoId,
                string.IsNullOrEmpty(filtroSearch) ? null : filtroSearch);
            if (fileBytes is { Length: > 0 })
            {
                var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                await JS.InvokeVoidAsync("downloadFile",
                    $"Comprobantes_Plano_{ts}.xlsx",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Convert.ToBase64String(fileBytes));
                successMessage = "Archivo Excel plano generado exitosamente.";
            }
            else errorMessage = "No se pudo generar el archivo Excel plano.";
        }
        catch (Exception ex) { errorMessage = $"Error al exportar: {ex.Message}"; }
        finally { exportando = false; }
    }

    private async Task ExportarListadoFormato(string formato)
    {
        exportando = true; errorMessage = null;
        try
        {
            string content; string fileName; string mimeType;
            switch (formato)
            {
                case "json": content = GenerarJsonPlano(asientos);  fileName = "Comprobantes.json"; mimeType = "application/json"; break;
                case "xml":  content = GenerarXmlPlano(asientos);   fileName = "Comprobantes.xml";  mimeType = "application/xml";  break;
                case "csv":  content = GenerarCsvListado(asientos); fileName = "Comprobantes.csv";  mimeType = "text/csv";         break;
                default: throw new Exception("Formato no reconocido.");
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            await JS.InvokeVoidAsync("downloadFile", fileName, mimeType, Convert.ToBase64String(bytes));
            successMessage = $"Archivo {formato.ToUpper()} generado.";
        }
        catch (Exception ex) { errorMessage = ex.Message; }
        finally { exportando = false; }
    }
}
