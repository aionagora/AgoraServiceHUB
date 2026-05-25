namespace AgoraHub360.ERP.Web.Services;

using System;

public class HttpExportEstadosFinancierosService
{
    private const string BaseUrl = "api/v1/contabilidad/estados-financieros";

    public string GetBalanceGeneralExportUrl(DateTime fechaCorte, string format)
    {
        return $"{BaseUrl}/balance-general/export?fechaCorte={fechaCorte:yyyy-MM-dd}&format={format}";
    }

    public string GetEstadoResultadosExportUrl(DateTime desde, DateTime hasta, string format)
    {
        return $"{BaseUrl}/estado-resultados/excel?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}&format={format}";
    }

    public string GetFlujoEfectivoExportUrl(DateTime desde, DateTime hasta, string format)
    {
        return $"{BaseUrl}/flujo-efectivo/export?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}&format={format}";
    }

    public string GetLibroMayorExportUrl(Guid cuentaContableId, DateTime desde, DateTime hasta, string format)
    {
        // Nota: en el endpoint del controller figura como entero, 
        // pero se solicitó esta firma. 
        return $"{BaseUrl}/libro-mayor/export?cuentaContableId={cuentaContableId}&desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}&format={format}";
    }
}
