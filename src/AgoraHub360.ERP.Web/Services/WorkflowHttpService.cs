namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Workflow;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementación HTTP de <see cref="IWorkflowClientService"/> para Blazor WASM.
/// Mapea las llamadas del componente a los endpoints de /api/v1/wf.
/// Todos los métodos capturan <see cref="HttpRequestException"/> y retornan
/// el valor por defecto del tipo de retorno para evitar crashes en la UI.
/// </summary>
public class WorkflowHttpService : IWorkflowClientService
{
    private readonly HttpClient _http;
    private readonly ILogger<WorkflowHttpService> _logger;
    private const string Base = "api/v1/wf";

    public WorkflowHttpService(HttpClient http, ILogger<WorkflowHttpService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    // ?? Tareas ????????????????????????????????????????????????????????????????

    public async Task<List<TareaDto>> GetByEntityAsync(string entityType, int entityId)
    {
        try
        {
            var url = $"{Base}/tareas?entityType={Uri.EscapeDataString(entityType)}&entityId={entityId}";
            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<TareaDto>>>();
            return result?.Data ?? new();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en WorkflowClientService.{Method}", nameof(GetByEntityAsync));
            return new();
        }
    }

    public async Task<TareaResumenDto?> GetResumenAsync(string entityType, int entityId)
    {
        try
        {
            var url = $"{Base}/tareas/0/resumen?entityType={Uri.EscapeDataString(entityType)}&entityId={entityId}";
            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TareaResumenDto>>();
            return result?.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en WorkflowClientService.{Method}", nameof(GetResumenAsync));
            return null;
        }
    }

    public async Task<TareaDto?> CreateAsync(TareaCreateDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{Base}/tareas", dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TareaDto>>();
            return result?.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en WorkflowClientService.{Method}", nameof(CreateAsync));
            return null;
        }
    }

    public async Task<TareaDto?> CompletarAsync(CompletarTareaDto dto)
    {
        try
        {
            var response = await _http.PatchAsJsonAsync($"{Base}/tareas/{dto.TareaId}/completar", dto);
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TareaDto>>();
            return result?.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en WorkflowClientService.{Method}", nameof(CompletarAsync));
            return null;
        }
    }

    public async Task GenerarHitosAsync(GenerarHitosDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{Base}/tareas/generar-iniciales", dto);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en WorkflowClientService.{Method}", nameof(GenerarHitosAsync));
        }
    }

    public async Task<TareaDto?> UpdateAsync(int id, TareaUpdateDto dto)
    {
        try
        {
            var response = await _http.PatchAsJsonAsync($"{Base}/tareas/{id}", dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TareaDto>>();
            return result?.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en WorkflowClientService.{Method}", nameof(UpdateAsync));
            return null;
        }
    }

    public async Task ReordenarAsync(ReordenarTareasDto dto)
    {
        try
        {
            await _http.PatchAsJsonAsync($"{Base}/tareas/reorder", dto);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en WorkflowClientService.{Method}", nameof(ReordenarAsync));
        }
    }

    public async Task<List<PlantillaTareaDto>> GetPlantillasAsync(string entityType, string? subTipo = null)
    {
        try
        {
            var url = $"{Base}/plantillas?entityType={Uri.EscapeDataString(entityType)}";
            if (!string.IsNullOrEmpty(subTipo))
                url += $"&subTipo={Uri.EscapeDataString(subTipo)}";
            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<PlantillaTareaDto>>>();
            return result?.Data ?? new();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en WorkflowClientService.{Method}", nameof(GetPlantillasAsync));
            return new();
        }
    }

    // ?? ABM Plantillas ????????????????????????????????????????????????????????

    public async Task<List<PlantillaTareaDto>> GetAllPlantillasAsync()
    {
        try
        {
            var response = await _http.GetAsync($"{Base}/plantillas/all");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<PlantillaTareaDto>>>();
            return result?.Data ?? new();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en {Method}", nameof(GetAllPlantillasAsync));
            return new();
        }
    }

    public async Task<PlantillaTareaDto?> CreatePlantillaAsync(PlantillaTareaCreateDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{Base}/plantillas", dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PlantillaTareaDto>>();
            return result?.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en {Method}", nameof(CreatePlantillaAsync));
            return null;
        }
    }

    public async Task<PlantillaTareaDto?> UpdatePlantillaAsync(int id, PlantillaTareaUpdateDto dto)
    {
        try
        {
            var response = await _http.PatchAsJsonAsync($"{Base}/plantillas/{id}", dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PlantillaTareaDto>>();
            return result?.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en {Method}", nameof(UpdatePlantillaAsync));
            return null;
        }
    }

    public async Task DeletePlantillaAsync(int id)
    {
        try
        {
            var response = await _http.DeleteAsync($"{Base}/plantillas/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en {Method}", nameof(DeletePlantillaAsync));
        }
    }
}
