namespace AgoraHub360.ERP.Web.Services;

using System.Net.Http.Headers;
using Microsoft.JSInterop;

/// <summary>
/// Manejador delegable que intercepta todas las peticiones HTTP salientes
/// y adjunta de manera dinámica el token JWT desde localStorage.
/// </summary>
public class AuthMessageHandler : DelegatingHandler
{
    private readonly IJSRuntime _js;
    private const string TokenKey = "agorahub360_auth_token";

    public AuthMessageHandler(IJSRuntime js)
    {
        _js = js;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // Evitar fallos de ejecución si localStorage no está disponible (ej. SSR o prerendering)
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
