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
            var tipoId = filtroTipoComp > 0 ? (int?)filtroTipoComp : null;
            var fileBytes = await AsientoService.ExportarFormatosAsync(
                formato, filtroDesde, filtroHasta,
                string.IsNullOrEmpty(filtroEstado) ? null : filtroEstado, tipoId,
                string.IsNullOrEmpty(filtroSearch) ? null : filtroSearch);

            if (fileBytes is { Length: > 0 })
            {
                var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = formato switch
                {
                    "json" => $"Comprobantes_{ts}.json",
                    "xml" => $"Comprobantes_{ts}.xml",
                    "csv" => $"Comprobantes_{ts}.csv",
                    _ => $"Comprobantes_{ts}.txt"
                };

                string mimeType = formato switch
                {
                    "json" => "application/json",
                    "xml" => "application/xml",
                    "csv" => "text/csv",
                    _ => "text/plain"
                };

                await JS.InvokeVoidAsync("downloadFile", fileName, mimeType, Convert.ToBase64String(fileBytes));
                successMessage = $"Archivo {formato.ToUpper()} generado.";
            }
            else
            {
                errorMessage = $"No se pudo generar el archivo {formato.ToUpper()}.";
            }
        }
        catch (Exception ex) { errorMessage = ex.Message; }
        finally { exportando = false; }
    }
}
