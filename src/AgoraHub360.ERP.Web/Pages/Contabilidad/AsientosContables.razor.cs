using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using AgoraHub360.ERP.Shared.Utils;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace AgoraHub360.ERP.Web.Pages.Contabilidad;

public partial class AsientosContables
{
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
                case "json": content = ExportFormatHelper.GenerarJsonPlano(asientos);  fileName = "Comprobantes.json"; mimeType = "application/json"; break;
                case "xml":  content = ExportFormatHelper.GenerarXmlPlano(asientos);   fileName = "Comprobantes.xml";  mimeType = "application/xml";  break;
                case "csv":  content = ExportFormatHelper.GenerarCsvListado(asientos); fileName = "Comprobantes.csv";  mimeType = "text/csv";         break;
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
