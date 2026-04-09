namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class HttpLibroMayorService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/contabilidad/estados-financieros";

    public HttpLibroMayorService(HttpClient http) => _http = http;

    public async Task<LibroMayorDto?> GetLibroMayorAsync(Guid cuentaId, DateTime desde, DateTime hasta)
    {
        var url = $"{Base}/libro-mayor?cuentaContableId={cuentaId}&desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        var r = await _http.GetFromJsonAsync<ApiResponse<LibroMayorDto>>(url);
        return r?.Data;
    }
}
