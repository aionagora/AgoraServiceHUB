namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.PRC;

public class PriceListHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/prc/price-lists";

    public PriceListHttpService(HttpClient http) => _http = http;

    public async Task<List<PriceListDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<PriceListDto>>>(BaseUrl);
        return response?.Data ?? new();
    }

    public async Task<ApiResponse<PriceListDto>> CreateAsync(CreatePriceListDto dto)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<PriceListDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<PriceListDto>>()
            ?? ApiResponse<PriceListDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<PriceListDto>> UpdateAsync(long id, UpdatePriceListDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<PriceListDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<PriceListDto>>()
            ?? ApiResponse<PriceListDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
        if (!response.IsSuccessStatusCode)
            return ApiResponse<bool>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }

    public async Task<List<PriceListItemDto>> GetItemsAsync(long priceListId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<PriceListItemDto>>>(
            $"{BaseUrl}/{priceListId}/items");
        return response?.Data ?? new();
    }

    public async Task<ApiResponse<PriceListItemDto>> AddItemAsync(long priceListId, CreatePriceListItemDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/{priceListId}/items", dto);
        if (!response.IsSuccessStatusCode)
            return ApiResponse<PriceListItemDto>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<PriceListItemDto>>()
            ?? ApiResponse<PriceListItemDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> DeleteItemAsync(long priceListId, long itemId)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{priceListId}/items/{itemId}");
        if (!response.IsSuccessStatusCode)
            return ApiResponse<bool>.Fail($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }
}
