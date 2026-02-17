namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Numeracion;

public class NumeracionHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/numeraciones";

    public NumeracionHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<NumeracionDocumentoDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<NumeracionDocumentoDto>>>(BaseUrl);
        return response?.Data ?? new();
    }

    public async Task<ApiResponse<NumeracionDocumentoDto>> CreateAsync(CreateNumeracionDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        return await response.Content.ReadFromJsonAsync<ApiResponse<NumeracionDocumentoDto>>()
            ?? ApiResponse<NumeracionDocumentoDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<NumeracionDocumentoDto>> UpdateAsync(int id, UpdateNumeracionDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        return await response.Content.ReadFromJsonAsync<ApiResponse<NumeracionDocumentoDto>>()
            ?? ApiResponse<NumeracionDocumentoDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }
}
