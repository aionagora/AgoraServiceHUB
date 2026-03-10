namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Json;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using AgoraHub360.ERP.Shared.DTOs.DOC;

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

    /// <summary>
    /// Exporta un comprobante individual a Excel
    /// </summary>
    public async Task<byte[]?> ExportarExcelIndividualAsync(long comprobanteId)
    {
        try
        {
            var response = await _http.GetAsync($"{Base}/{comprobanteId}/exportar-excel");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsByteArrayAsync();
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Exporta listado detallado plano (una fila por línea) para migración
    /// </summary>
    public async Task<byte[]?> ExportarExcelPlanoAsync(
        DateTime? desde = null, DateTime? hasta = null,
        string? estado = null, int? tipoComprobanteId = null, string? search = null)
    {
        try
        {
            var url = $"{Base}/exportar-excel-plano?";
            if (desde.HasValue) url += $"desde={desde.Value:yyyy-MM-dd}&";
            if (hasta.HasValue) url += $"hasta={hasta.Value:yyyy-MM-dd}&";
            if (!string.IsNullOrEmpty(estado)) url += $"estado={Uri.EscapeDataString(estado)}&";
            if (tipoComprobanteId.HasValue) url += $"tipoComprobanteId={tipoComprobanteId}&";
            if (!string.IsNullOrEmpty(search)) url += $"search={Uri.EscapeDataString(search)}&";
            var response = await _http.GetAsync(url.TrimEnd('&', '?'));
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsByteArrayAsync();
            return null;
        }
        catch
        {
            return null;
        }
    }

    // ?? Documentos adjuntos ???????????????????????????????????????????????????

    /// <summary>Lista los documentos adjuntos a un comprobante.</summary>
    public async Task<List<ComprobanteDocumentoDto>> GetDocumentosAsync(long comprobanteId)
    {
        var r = await _http.GetFromJsonAsync<ApiResponse<List<ComprobanteDocumentoDto>>>(
            $"{Base}/{comprobanteId}/documentos");
        return r?.Data ?? new();
    }

    /// <summary>
    /// Sube un archivo a api/v1/documentos y luego lo vincula al comprobante.
    /// Devuelve el DTO del adjunto creado.
    /// </summary>
    public async Task<ApiResponse<ComprobanteDocumentoDto>> SubirYAdjuntarAsync(
        long comprobanteId, Stream fileStream, string fileName, string mimeType, string? descripcion = null)
    {
        // 1) Upload del archivo
        using var content   = new MultipartFormDataContent();
        using var sc        = new StreamContent(fileStream);
        sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
        content.Add(sc, "file", fileName);

        var uploadResp = await _http.PostAsync("api/v1/documentos", content);
        var uploadResult = await ParseResponse<DocumentUploadResultDto>(uploadResp);
        if (!uploadResult.Success || uploadResult.Data is null)
            return ApiResponse<ComprobanteDocumentoDto>.Fail(uploadResult.Message ?? "Error al subir el archivo.");

        // 2) Vincular al comprobante
        var adjunto = new AdjuntarDocumentoDto
        {
            DocumentId  = uploadResult.Data.DocumentId,
            Descripcion = descripcion
        };

        var adjResponse = await _http.PostAsJsonAsync($"{Base}/{comprobanteId}/documentos", adjunto);
        return await ParseResponse<ComprobanteDocumentoDto>(adjResponse);
    }

    /// <summary>Elimina el vínculo de un documento adjunto a un comprobante.</summary>
    public async Task<ApiResponse<bool>> RemoverDocumentoAsync(long comprobanteId, int docId)
    {
        var response = await _http.DeleteAsync($"{Base}/{comprobanteId}/documentos/{docId}");
        return await ParseResponse<bool>(response);
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
