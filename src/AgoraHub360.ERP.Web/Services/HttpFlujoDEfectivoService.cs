namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class HttpFlujoDEfectivoService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/contabilidad/estados-financieros";

    public HttpFlujoDEfectivoService(HttpClient http) => _http = http;

    public async Task<FlujoDEfectivoDto?> GetFlujoDEfectivoAsync(DateTime desde, DateTime hasta)
    {
        var url = $"{Base}/flujo-efectivo?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        var r = await _http.GetFromJsonAsync<ApiResponse<FlujoDEfectivoDto>>(url);
        return r?.Data;
    }
}
