namespace AgoraHub360.ERP.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Infrastructure.FacturacionElectronica;
using AgoraHub360.ERP.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IAsientoExportService, AsientoExportService>();
        services.AddScoped<IAsientoImportService, AsientoImportService>();

        // FE: Facturación Electrónica — Cifrado de secretos
        services.AddScoped<ICifradoService, AesCifradoService>();

        // FE: Facturación Electrónica — Providers
        services.AddHttpClient("CirrusFacturacion");
        services.AddScoped<IFacturacionElectronicaProvider, CirrusFacturacionProvider>();

        return services;
    }
}
