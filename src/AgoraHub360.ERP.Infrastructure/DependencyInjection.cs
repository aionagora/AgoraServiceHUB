namespace AgoraHub360.ERP.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registro de servicios de infraestructura (servicios externos, email, storage, etc.).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Registrar servicios de infraestructura aquí
        return services;
    }
}
