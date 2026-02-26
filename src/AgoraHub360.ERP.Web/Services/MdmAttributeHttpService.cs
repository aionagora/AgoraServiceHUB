namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class MdmAttributeHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/mdm/attributes";

    public MdmAttributeHttpService(HttpClient http) => _http = http;

    public async Task<List<AttributeDefinitionDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<AttributeDefinitionDto>>>(BaseUrl);
        return response?.Data ?? new();
    }

    public async Task<List<AttributeDefinitionDto>> GetByIndustryAsync(int industryId)
    {
        var url = $"{BaseUrl}?industryId={industryId}";
        var response = await _http.GetFromJsonAsync<ApiResponse<List<AttributeDefinitionDto>>>(url);
        return response?.Data ?? new();
    }

    public async Task<AttributeDefinitionDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<AttributeDefinitionDto>>($"{BaseUrl}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<AttributeDefinitionDto>> CreateAsync(CreateAttributeDefinitionDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<AttributeDefinitionDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<AttributeDefinitionDto>>()
            ?? ApiResponse<AttributeDefinitionDto>.Fail("Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
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

    public async Task<ApiResponse<AttributeOptionDto>> AddOptionAsync(CreateAttributeOptionDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/{dto.AttributeId}/options", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<AttributeOptionDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<AttributeOptionDto>>()
            ?? ApiResponse<AttributeOptionDto>.Fail("Error de comunicación con el servidor.");
    }

    public async Task<ApiResponse<bool>> DeleteOptionAsync(long attributeId, long optionId)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{attributeId}/options/{optionId}");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación con el servidor.");
    }

    public async Task<List<ProductAttributeDto>> GetProductAttributesAsync(long productId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ProductAttributeDto>>>(
            $"{BaseUrl}/product/{productId}");
        return response?.Data ?? new();
    }

    public async Task<ApiResponse<ProductAttributeDto>> UpsertProductAttributeAsync(UpsertProductAttributeDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/product", dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<ProductAttributeDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductAttributeDto>>()
            ?? ApiResponse<ProductAttributeDto>.Fail("Error de comunicación.");
    }
}
