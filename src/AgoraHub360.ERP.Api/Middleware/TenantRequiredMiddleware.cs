namespace AgoraHub360.ERP.Api.Middleware;

using System.Text.Json;

/// <summary>
/// Middleware que valida que las peticiones tenant-aware (rutas que NO son /empresas ni /diagnostics)
/// incluyan el claim EmpresaId cuando el usuario esta autenticado.
/// Las operaciones de escritura (POST/PUT/DELETE) en rutas tenant-aware requieren EmpresaId.
/// </summary>
public class TenantRequiredMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly string[] ExemptPaths = new[]
    {
        "/api/v1/auth/login",
        "/api/v1/auth/seleccionar-empresa",
        "/api/v1/auth/mis-empresas",
        "/api/v1/empresas/mis-empresas",
        "/api/v1/health",
        "/health",
        "/swagger"
    };

    public TenantRequiredMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var method = context.Request.Method;

        // Solo validar en rutas de API que no esten exentas
        var isApiRoute = path.StartsWith("/api/");
        var isExempt = ExemptPaths.Any(p => path.StartsWith(p.ToLowerInvariant()));
        var isWriteOperation = method is "POST" or "PUT" or "DELETE";
        var isTenantAware = isApiRoute && !isExempt;
        var isAuthenticated = context.User.Identity?.IsAuthenticated == true;

        if (isTenantAware && isAuthenticated)
        {
            var empresaIdClaim = context.User.FindFirst("EmpresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out _))
            {
                Console.WriteLine($"[TEMP-LOG] TenantRequired bloqueó {method} {path}. Motivo: EmpresaId ausente/invalid.");
                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = StatusCodes.Status403Forbidden;

                var problem = new
                {
                    type = "https://httpstatuses.com/403",
                    title = "Tenant Required",
                    status = 403,
                    detail = "Debe seleccionar una empresa activa para realizar esta operacion.",
                    traceId = context.TraceIdentifier
                };

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(problem, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
                return;
            }
        }

        await _next(context);
    }
}
