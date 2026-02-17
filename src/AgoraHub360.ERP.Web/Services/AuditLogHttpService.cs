namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.AuditLog;

public class AuditLogHttpService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/v1/audit-logs";

    public AuditLogHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PaginatedResultDto<AuditLogDto>> GetLogsAsync(AuditLogFilterDto filter)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(filter.Entidad))
            queryParams.Add($"entidad={Uri.EscapeDataString(filter.Entidad)}");
        if (!string.IsNullOrWhiteSpace(filter.EntidadId))
            queryParams.Add($"entidadId={Uri.EscapeDataString(filter.EntidadId)}");
        if (!string.IsNullOrWhiteSpace(filter.Accion))
            queryParams.Add($"accion={Uri.EscapeDataString(filter.Accion)}");
        if (!string.IsNullOrWhiteSpace(filter.Usuario))
            queryParams.Add($"usuario={Uri.EscapeDataString(filter.Usuario)}");
        if (filter.EmpresaId.HasValue)
            queryParams.Add($"empresaId={filter.EmpresaId}");
        if (filter.FechaDesde.HasValue)
            queryParams.Add($"fechaDesde={filter.FechaDesde:o}");
        if (filter.FechaHasta.HasValue)
            queryParams.Add($"fechaHasta={filter.FechaHasta:o}");

        queryParams.Add($"pagina={filter.Pagina}");
        queryParams.Add($"tamanoPagina={filter.TamanoPagina}");

        var url = $"{BaseUrl}?{string.Join("&", queryParams)}";
        var response = await _http.GetFromJsonAsync<ApiResponse<PaginatedResultDto<AuditLogDto>>>(url);
        return response?.Data ?? new PaginatedResultDto<AuditLogDto>();
    }

    public async Task<AuditLogDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<AuditLogDto>>($"{BaseUrl}/{id}");
        return response?.Data;
    }

    public async Task<List<string>> GetEntidadesDistintasAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<string>>>($"{BaseUrl}/entidades");
        return response?.Data ?? new();
    }
}
