namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Seguridad;

public class SeguridadDinamicaHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/seguridad-dinamica";

    public SeguridadDinamicaHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<SesionContextoDto?> GetContextoSesionAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<SesionContextoDto>>($"{BaseUrl}/contexto-sesion");
            return response?.Data;
        }
        catch
        {
            return null;
        }
    }
}
