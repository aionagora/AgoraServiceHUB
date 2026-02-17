namespace AgoraHub360.ERP.Application;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registro de servicios de la capa Application.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registrar servicios de aplicación aquí
        return services;
    }
}
