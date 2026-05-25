namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Common;

public class GeoCatalogHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/geografia";

    public GeoCatalogHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<GeoCatalogItemDto>> GetPaisesAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<GeoCatalogItemDto>>>($"{BaseUrl}/paises");
        return response?.Data ?? new List<GeoCatalogItemDto>();
    }

    public async Task<List<GeoCatalogItemDto>> GetDepartamentosByPaisAsync(Guid paisId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<GeoCatalogItemDto>>>($"{BaseUrl}/paises/{paisId}/departamentos");
        return response?.Data ?? new List<GeoCatalogItemDto>();
    }

    public async Task<List<GeoCatalogItemDto>> GetProvinciasByDepartamentoAsync(Guid departamentoId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<GeoCatalogItemDto>>>($"{BaseUrl}/departamentos/{departamentoId}/provincias");
        return response?.Data ?? new List<GeoCatalogItemDto>();
    }

    public async Task<List<GeoCatalogItemDto>> GetCiudadesByProvinciaAsync(Guid provinciaId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<GeoCatalogItemDto>>>($"{BaseUrl}/provincias/{provinciaId}/ciudades");
        return response?.Data ?? new List<GeoCatalogItemDto>();
    }

    public async Task<List<GeoCatalogItemDto>> GetZonasByCiudadAsync(Guid ciudadId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<GeoCatalogItemDto>>>($"{BaseUrl}/ciudades/{ciudadId}/zonas");
        return response?.Data ?? new List<GeoCatalogItemDto>();
    }
}
