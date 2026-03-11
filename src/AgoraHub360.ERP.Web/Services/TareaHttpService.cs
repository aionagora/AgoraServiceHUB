namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Workflow;

public class TareaHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/wf/tareas";

    public TareaHttpService(HttpClient http) => _http = http;

    public async Task<List<TareaDto>> GetByEntityAsync(string entityType, int entityId)
    {
        var url = $"{Base}?entityType={Uri.EscapeDataString(entityType)}&entityId={entityId}";
        var response = await _http.GetFromJsonAsync<ApiResponse<List<TareaDto>>>(url);
        return response?.Data ?? new List<TareaDto>();
    }

    public async Task<ApiResponse<TareaDto>> CompletarAsync(int id)
    {
        var r = await _http.PatchAsync($"{Base}/{id}/completar", null);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<TareaDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<TareaDto>>()
            ?? ApiResponse<TareaDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<List<TareaDto>>> GenerarInicialesAsync(string entityType, int entityId)
    {
        var r = await _http.PostAsync($"{Base}/{Uri.EscapeDataString(entityType)}/{entityId}/generar-iniciales", null);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<List<TareaDto>>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<List<TareaDto>>>()
            ?? ApiResponse<List<TareaDto>>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<List<TareaDto>>> ReorderAsync(ReorderTareasDto dto)
    {
        var r = await _http.PatchAsJsonAsync($"{Base}/reorder", dto);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<List<TareaDto>>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<List<TareaDto>>>()
            ?? ApiResponse<List<TareaDto>>.Fail("Error de comunicación.");
    }
}
