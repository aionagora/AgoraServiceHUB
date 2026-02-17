namespace AgoraHub360.ERP.Application;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Application.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registro de servicios de la capa Application.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEmpresaService, EmpresaService>();

        return services;
    }
}
