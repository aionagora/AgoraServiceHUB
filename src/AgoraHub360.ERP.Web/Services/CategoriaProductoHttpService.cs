namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.CategoriaProducto;

public class CategoriaProductoHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/categorias";

    public CategoriaProductoHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<CategoriaProductoDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<CategoriaProductoDto>>>(BaseUrl);
        return response?.Data ?? new List<CategoriaProductoDto>();
    }

    public async Task<CategoriaProductoDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<CategoriaProductoDto>>($"{BaseUrl}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<CategoriaProductoDto>> CreateAsync(CreateCategoriaProductoDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<CategoriaProductoDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<CategoriaProductoDto>>()
            ?? ApiResponse<CategoriaProductoDto>.Fail("Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<CategoriaProductoDto>> UpdateAsync(int id, UpdateCategoriaProductoDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<CategoriaProductoDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<CategoriaProductoDto>>()
            ?? ApiResponse<CategoriaProductoDto>.Fail("Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación con el servidor.");
    }
}
