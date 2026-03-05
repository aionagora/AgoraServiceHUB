namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Compras;

public class OrdenCompraHttpService
{
    private readonly HttpClient _http;
    private const string Base = "api/v1/compras/ordenes";

    public OrdenCompraHttpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<OrdenCompraDto>> GetAllAsync(
        int? proveedorId = null,
        string? estado = null,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        var url = $"{Base}?";
        if (proveedorId.HasValue) url += $"proveedorId={proveedorId}&";
        if (!string.IsNullOrEmpty(estado)) url += $"estado={Uri.EscapeDataString(estado)}&";
        if (desde.HasValue) url += $"desde={desde.Value:yyyy-MM-dd}&";
        if (hasta.HasValue) url += $"hasta={hasta.Value:yyyy-MM-dd}&";

        var response = await _http.GetFromJsonAsync<ApiResponse<List<OrdenCompraDto>>>(url.TrimEnd('&', '?'));
        return response?.Data ?? new List<OrdenCompraDto>();
    }

    public async Task<OrdenCompraDto?> GetByIdAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<OrdenCompraDto>>($"{Base}/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<OrdenCompraDto>> CreateAsync(CreateOrdenCompraDto dto)
    {
        var response = await _http.PostAsJsonAsync(Base, dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<OrdenCompraDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>()
            ?? ApiResponse<OrdenCompraDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<OrdenCompraDto>> UpdateAsync(long id, UpdateOrdenCompraDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{Base}/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<OrdenCompraDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>()
            ?? ApiResponse<OrdenCompraDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<OrdenCompraDto>> AddLineaAsync(long ordenId, AddOrdenCompraLineaDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/{ordenId}/lineas", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<OrdenCompraDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>()
            ?? ApiResponse<OrdenCompraDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<OrdenCompraDto>> UpdateLineaAsync(long ordenId, long lineaId, UpdateOrdenCompraLineaDto dto)
    {
        var response = await _http.PutAsJsonAsync($"{Base}/{ordenId}/lineas/{lineaId}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<OrdenCompraDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>()
            ?? ApiResponse<OrdenCompraDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<OrdenCompraDto>> RemoveLineaAsync(long ordenId, long lineaId)
    {
        var response = await _http.DeleteAsync($"{Base}/{ordenId}/lineas/{lineaId}");
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<OrdenCompraDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>()
            ?? ApiResponse<OrdenCompraDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<OrdenCompraDto>> CambiarEstadoAsync(long id, string nuevoEstado)
    {
        var response = await _http.PostAsync($"{Base}/{id}/estado?nuevoEstado={Uri.EscapeDataString(nuevoEstado)}", null);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<OrdenCompraDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>()
            ?? ApiResponse<OrdenCompraDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<OrdenCompraDto>> AprobarRechazarAsync(long id, AprobarRechazarOrdenCompraDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/{id}/aprobar", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<OrdenCompraDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<OrdenCompraDto>>()
            ?? ApiResponse<OrdenCompraDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<ConfirmacionProveedorDto>> RegistrarConfirmacionProveedorAsync(long id, RegistrarConfirmacionProveedorDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/{id}/confirmacion-proveedor", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<ConfirmacionProveedorDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<ConfirmacionProveedorDto>>()
            ?? ApiResponse<ConfirmacionProveedorDto>.Fail("Error de comunicación.");
    }

    public async Task<List<ConfirmacionProveedorDto>> GetConfirmacionesAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ConfirmacionProveedorDto>>>($"{Base}/{id}/confirmaciones");
        return response?.Data ?? new List<ConfirmacionProveedorDto>();
    }

    public async Task<ApiResponse<PagoOrdenCompraDto>> ProgramarPagoAsync(long id, ProgramarPagoDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/{id}/pagos", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<PagoOrdenCompraDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<PagoOrdenCompraDto>>()
            ?? ApiResponse<PagoOrdenCompraDto>.Fail("Error de comunicación.");
    }

    public async Task<ApiResponse<PagoOrdenCompraDto>> EjecutarPagoAsync(long id, long pagoId, EjecutarPagoDto dto)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/{id}/pagos/{pagoId}/ejecutar", dto);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return ApiResponse<PagoOrdenCompraDto>.Fail($"Error HTTP {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<PagoOrdenCompraDto>>()
            ?? ApiResponse<PagoOrdenCompraDto>.Fail("Error de comunicación.");
    }

    public async Task<List<PagoOrdenCompraDto>> GetPagosAsync(long id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<PagoOrdenCompraDto>>>($"{Base}/{id}/pagos");
        return response?.Data ?? new List<PagoOrdenCompraDto>();
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
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
}
