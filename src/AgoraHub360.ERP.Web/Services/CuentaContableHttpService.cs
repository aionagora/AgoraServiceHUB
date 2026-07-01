namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;

public class CuentaContableHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/contabilidad/cuentas";

    public CuentaContableHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<CuentaContableDto>> GetAllAsync(byte? tipo = null, bool? permiteMovimientos = null, string? search = null)
    {
        var url = $"{Base}?";
        if (tipo.HasValue) url += $"tipo={tipo}&";
        if (permiteMovimientos.HasValue) url += $"permiteMovimientos={permiteMovimientos}&";
        if (!string.IsNullOrEmpty(search)) url += $"search={Uri.EscapeDataString(search)}&";
        var response = await _http.GetFromJsonAsync<ApiResponse<List<CuentaContableDto>>>(url.TrimEnd('&', '?'));
        return response?.Data ?? new();
    }

    public async Task<List<CuentaContableDto>> GetTreeAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<CuentaContableDto>>>($"{Base}/tree");
        return response?.Data ?? new();
    }

    public async Task<CuentaContableDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<CuentaContableDto>>($"{Base}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<CuentaContableDto>> CreateAsync(CreateCuentaContableDto dto)
    {
        var response = await _http.PostAsJsonAsync(Base, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<CuentaContableDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<CuentaContableDto>>()
            ?? ApiResponse<CuentaContableDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<CuentaContableDto>> UpdateAsync(int id, UpdateCuentaContableDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{Base}/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<CuentaContableDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<CuentaContableDto>>()
            ?? ApiResponse<CuentaContableDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"{Base}/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<int>> SeedPlanCuentasAsync()
    {
        var response = await _http.PostAsync($"{Base}/seed", null);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<int>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<int>>()
            ?? ApiResponse<int>.Fail("Error de comunicación.");
    }

    // ── Importación CSV ──

    public async Task<PlanCuentaImportPreviewDto> ImportPreviewAsync(MultipartFormDataContent content)
    {
        var response = await _http.PostAsync($"{Base}/import-preview", content);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PlanCuentaImportPreviewDto>>();
        return result?.Data ?? new PlanCuentaImportPreviewDto { CanImport = false };
    }

    public async Task<PlanCuentaImportResultDto> ImportConfirmAsync(MultipartFormDataContent content)
    {
        var response = await _http.PostAsync($"{Base}/import-confirm", content);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PlanCuentaImportResultDto>>();
        return result?.Data ?? new PlanCuentaImportResultDto();
    }

    public async Task<byte[]?> DownloadTemplateAsync()
    {
        var response = await _http.GetAsync($"{Base}/import-template");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }
}
