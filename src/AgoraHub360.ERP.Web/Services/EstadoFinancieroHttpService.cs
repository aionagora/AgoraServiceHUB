namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class EstadoFinancieroHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/contabilidad/estados-financieros";

    public EstadoFinancieroHttpService(HttpClient http) => _http = http;

    // ?? Balance General ??

    public async Task<BalanceGeneralDto?> GetBalanceGeneralAsync(DateTime fechaCorte)
    {
        var url = $"{Base}/balance-general?fechaCorte={fechaCorte:yyyy-MM-dd}";
        var r = await _http.GetFromJsonAsync<ApiResponse<BalanceGeneralDto>>(url);
        return r?.Data;
    }

    public async Task<byte[]?> ExportBalanceGeneralExcelAsync(DateTime fechaCorte)
    {
        var url = $"{Base}/balance-general/excel?fechaCorte={fechaCorte:yyyy-MM-dd}";
        var response = await _http.GetAsync(url);
        return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
    }

    // ?? Estado de Resultados ??

    public async Task<EstadoResultadosDto?> GetEstadoResultadosAsync(DateTime desde, DateTime hasta)
    {
        var url = $"{Base}/estado-resultados?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        var r = await _http.GetFromJsonAsync<ApiResponse<EstadoResultadosDto>>(url);
        return r?.Data;
    }

    public async Task<byte[]?> ExportEstadoResultadosExcelAsync(DateTime desde, DateTime hasta)
    {
        var url = $"{Base}/estado-resultados/excel?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        var response = await _http.GetAsync(url);
        return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
    }

    // ?? Sumas y Saldos ??

    public async Task<SumasYSaldosDto?> GetSumasYSaldosAsync(DateTime desde, DateTime hasta)
    {
        var url = $"{Base}/sumas-saldos?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        var r = await _http.GetFromJsonAsync<ApiResponse<SumasYSaldosDto>>(url);
        return r?.Data;
    }

    public async Task<byte[]?> ExportSumasYSaldosExcelAsync(DateTime desde, DateTime hasta)
    {
        var url = $"{Base}/sumas-saldos/excel?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        var response = await _http.GetAsync(url);
        return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
    }

    // ?? Libro Diario ??

    public async Task<LibroDiarioDto?> GetLibroDiarioAsync(DateTime desde, DateTime hasta, string? estado = null)
    {
        var url = $"{Base}/libro-diario?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        if (!string.IsNullOrEmpty(estado)) url += $"&estado={Uri.EscapeDataString(estado)}";
        var r = await _http.GetFromJsonAsync<ApiResponse<LibroDiarioDto>>(url);
        return r?.Data;
    }

    public async Task<byte[]?> ExportLibroDiarioExcelAsync(DateTime desde, DateTime hasta, string? estado = null)
    {
        var url = $"{Base}/libro-diario/excel?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        if (!string.IsNullOrEmpty(estado)) url += $"&estado={Uri.EscapeDataString(estado)}";
        var response = await _http.GetAsync(url);
        return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
    }
}
