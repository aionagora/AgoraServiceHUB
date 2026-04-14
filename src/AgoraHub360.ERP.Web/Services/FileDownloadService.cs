using Microsoft.JSInterop;

namespace AgoraHub360.ERP.Web.Services;

/// <summary>
/// Servicio reutilizable para descarga de archivos desde Blazor WASM usando JS interop.
/// Centraliza todas las descargas para evitar llamadas directas a IJSRuntime.
/// </summary>
public class FileDownloadService
{
    private readonly IJSRuntime _js;
    private readonly HttpClient _http;

    public FileDownloadService(IJSRuntime js, HttpClient http)
    {
        _js = js;
        _http = http;
    }

    /// <summary>
    /// Descarga un archivo convirtiendo los bytes a base64 y disparando la descarga vía JS.
    /// </summary>
    public async Task DownloadFromBase64Async(string fileName, string contentType, byte[] fileBytes)
    {
        var base64 = Convert.ToBase64String(fileBytes);
        await _js.InvokeVoidAsync("downloadFileFromBase64", fileName, contentType, base64);
    }

    /// <summary>
    /// Descarga un archivo llamando a la API vía HttpClient (con base URL y JWT correctos)
    /// y luego dispara la descarga en el navegador.
    /// </summary>
    public async Task DownloadFromApiAsync(string relativeUrl, string fileName)
    {
        var response = await _http.GetAsync(relativeUrl);
        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType
                          ?? "application/octet-stream";

        await DownloadFromBase64Async(fileName, contentType, bytes);
    }
}
