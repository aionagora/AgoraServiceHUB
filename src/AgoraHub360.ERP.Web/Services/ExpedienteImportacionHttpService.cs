namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Compras;

public class ExpedienteImportacionHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/compras/expedientes";

    public ExpedienteImportacionHttpService(HttpClient http) => _http = http;

    public async Task<List<ExpedienteImportacionDto>> GetAllAsync(
        string? estado = null,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        var url = $"{Base}?";
        if (!string.IsNullOrEmpty(estado)) url += $"estado={Uri.EscapeDataString(estado)}&";
        if (desde.HasValue) url += $"desde={desde.Value:yyyy-MM-dd}&";
        if (hasta.HasValue) url += $"hasta={hasta.Value:yyyy-MM-dd}&";
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ExpedienteImportacionDto>>>(url.TrimEnd('&', '?'));
        return response?.Data ?? new List<ExpedienteImportacionDto>();
    }

    public async Task<ExpedienteImportacionDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<ExpedienteImportacionDto>>($"{Base}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<ExpedienteImportacionDto>> CreateAsync(CreateExpedienteImportacionDto dto)
    {
        var r = await _http.PostAsJsonAsync(Base, dto);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<ExpedienteImportacionDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<ExpedienteImportacionDto>>()
            ?? ApiResponse<ExpedienteImportacionDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<ExpedienteImportacionDto>> UpdateEmbarqueAsync(long id, UpdateExpedienteEmbarqueDto dto)
    {
        var r = await _http.PutAsJsonAsync($"{Base}/{id}/embarque", dto);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<ExpedienteImportacionDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<ExpedienteImportacionDto>>()
            ?? ApiResponse<ExpedienteImportacionDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<ExpedienteImportacionDto>> ConfirmarSalidaAsync(long id, DateTime fechaSalida)
    {
        var r = await _http.PostAsync($"{Base}/{id}/confirmar-salida?fechaSalida={fechaSalida:yyyy-MM-dd}", null);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<ExpedienteImportacionDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<ExpedienteImportacionDto>>()
            ?? ApiResponse<ExpedienteImportacionDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<ExpedienteImportacionDto>> RegistrarArriboAsync(long id, RegistrarArriboDto dto)
    {
        var r = await _http.PostAsJsonAsync($"{Base}/{id}/arribo", dto);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<ExpedienteImportacionDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<ExpedienteImportacionDto>>()
            ?? ApiResponse<ExpedienteImportacionDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<ExpedienteImportacionDto>> IniciarDespachoAduaneroAsync(long id, RegistrarDespachoAduaneroDto dto)
    {
        var r = await _http.PostAsJsonAsync($"{Base}/{id}/despacho-aduanero", dto);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<ExpedienteImportacionDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<ExpedienteImportacionDto>>()
            ?? ApiResponse<ExpedienteImportacionDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<ExpedienteImportacionDto>> RegistrarObservacionAduanaAsync(long id, RegistrarObservacionAduanaDto dto)
    {
        var r = await _http.PostAsJsonAsync($"{Base}/{id}/observacion-aduana", dto);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<ExpedienteImportacionDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<ExpedienteImportacionDto>>()
            ?? ApiResponse<ExpedienteImportacionDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<ExpedienteImportacionDto>> SubsanarObservacionAsync(long id, string? observaciones = null)
    {
        var url = $"{Base}/{id}/subsanar-observacion";
        if (!string.IsNullOrEmpty(observaciones)) url += $"?observaciones={Uri.EscapeDataString(observaciones)}";
        var r = await _http.PostAsync(url, null);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<ExpedienteImportacionDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<ExpedienteImportacionDto>>()
            ?? ApiResponse<ExpedienteImportacionDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<ExpedienteImportacionDto>> RegistrarLevanteAsync(long id, RegistrarLevanteDto dto)
    {
        var r = await _http.PostAsJsonAsync($"{Base}/{id}/levante", dto);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<ExpedienteImportacionDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<ExpedienteImportacionDto>>()
            ?? ApiResponse<ExpedienteImportacionDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<ExpedienteImportacionDto>> AddHitoAsync(long id, AddHitoExpedienteDto dto)
    {
        var r = await _http.PostAsJsonAsync($"{Base}/{id}/hitos", dto);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<ExpedienteImportacionDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<ExpedienteImportacionDto>>()
            ?? ApiResponse<ExpedienteImportacionDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<ExpedienteImportacionDto>> CerrarAsync(long id)
    {
        var r = await _http.PostAsync($"{Base}/{id}/cerrar", null);
        if (!r.IsSuccessStatusCode)
            return ApiResponse<ExpedienteImportacionDto>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<ExpedienteImportacionDto>>()
            ?? ApiResponse<ExpedienteImportacionDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
    {
        var r = await _http.DeleteAsync($"{Base}/{id}");
        if (!r.IsSuccessStatusCode)
            return ApiResponse<bool>.Fail($"Error HTTP {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadFromJsonAsync<ApiResponse<bool>>()
            ?? ApiResponse<bool>.Fail("Error de comunicación.");
    }
}
