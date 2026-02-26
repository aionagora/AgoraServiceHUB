namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class AsientoContableHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/contabilidad/asientos";

    public AsientoContableHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<AsientoContableDto>> GetAllAsync(
        DateTime? desde = null, DateTime? hasta = null,
        string? estado = null, int? tipoComprobanteId = null, string? search = null)
    {
        var url = $"{Base}?";
        if (desde.HasValue) url += $"desde={desde.Value:yyyy-MM-dd}&";
        if (hasta.HasValue) url += $"hasta={hasta.Value:yyyy-MM-dd}&";
        if (!string.IsNullOrEmpty(estado)) url += $"estado={Uri.EscapeDataString(estado)}&";
        if (tipoComprobanteId.HasValue) url += $"tipoComprobanteId={tipoComprobanteId}&";
        if (!string.IsNullOrEmpty(search)) url += $"search={Uri.EscapeDataString(search)}&";
        var response = await _http.GetFromJsonAsync<ApiResponse<List<AsientoContableDto>>>(url.TrimEnd('&', '?'));
        return response?.Data ?? new();
    }

    public async Task<AsientoContableDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<AsientoContableDto>>($"{Base}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<AsientoContableDto>> CreateAsync(CreateAsientoContableDto dto)
    {
        var response = await _http.PostAsJsonAsync(Base, dto);
        return await ParseResponse<AsientoContableDto>(response);
    }

    public async Task<ApiResponse<AsientoContableDto>> UpdateAsync(long id, UpdateAsientoContableDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{Base}/{id}", dto);
        return await ParseResponse<AsientoContableDto>(response);
    }

    public async Task<ApiResponse<AsientoContableDto>> CopiarAsync(long id)
    {
        var response = await _http.PostAsync($"{Base}/{id}/copiar", null);
        return await ParseResponse<AsientoContableDto>(response);
    }

    public async Task<ApiResponse<AsientoContableDto>> ContabilizarAsync(long id)
    {
        var response = await _http.PostAsync($"{Base}/{id}/contabilizar", null);
        return await ParseResponse<AsientoContableDto>(response);
    }

    public async Task<ApiResponse<AsientoContableDto>> AnularAsync(long id)
    {
        var response = await _http.PostAsync($"{Base}/{id}/anular", null);
        return await ParseResponse<AsientoContableDto>(response);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
    {
        var response = await _http.DeleteAsync($"{Base}/{id}");
        return await ParseResponse<bool>(response);
    }

    // ?? Catálogos ??
    public async Task<List<TipoComprobanteDto>> GetTiposComprobanteAsync()
    {
        var r = await _http.GetFromJsonAsync<ApiResponse<List<TipoComprobanteDto>>>($"{Base}/tipos-comprobante");
        return r?.Data ?? new();
    }

    public async Task<List<TipoCambioDto>> GetTiposCambioAsync()
    {
        var r = await _http.GetFromJsonAsync<ApiResponse<List<TipoCambioDto>>>($"{Base}/tipos-cambio");
        return r?.Data ?? new();
    }

    public async Task<List<TipoPagoDto>> GetTiposPagoAsync()
    {
        var r = await _http.GetFromJsonAsync<ApiResponse<List<TipoPagoDto>>>($"{Base}/tipos-pago");
        return r?.Data ?? new();
    }

    public async Task<ApiResponse<int>> SeedCatalogosAsync()
    {
        var response = await _http.PostAsync($"{Base}/seed-catalogos", null);
        return await ParseResponse<int>(response);
    }

    /// <summary>
    /// Exporta los comprobantes contables filtrados a un archivo Excel
    /// </summary>
    public async Task<byte[]?> ExportarExcelAsync(
        DateTime? desde = null, DateTime? hasta = null,
        string? estado = null, int? tipoComprobanteId = null, string? search = null)
    {
        try
        {
            var url = $"{Base}/exportar-excel?";
            if (desde.HasValue) url += $"desde={desde.Value:yyyy-MM-dd}&";
            if (hasta.HasValue) url += $"hasta={hasta.Value:yyyy-MM-dd}&";
            if (!string.IsNullOrEmpty(estado)) url += $"estado={Uri.EscapeDataString(estado)}&";
            if (tipoComprobanteId.HasValue) url += $"tipoComprobanteId={tipoComprobanteId}&";
            if (!string.IsNullOrEmpty(search)) url += $"search={Uri.EscapeDataString(search)}&";

            var response = await _http.GetAsync(url.TrimEnd('&', '?'));
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    private static async Task<ApiResponse<T>> ParseResponse<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<T>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<T>>()
            ?? ApiResponse<T>.Fail("Error de comunicación.");
    }
}
