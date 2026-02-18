namespace AgoraHub360.ERP.Persistence;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Persistence.Context;
using AgoraHub360.ERP.Persistence.Interceptors;
using AgoraHub360.ERP.Persistence.Repositories;
using AgoraHub360.ERP.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registro de servicios de persistencia (DbContext, interceptores, repositorios).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<EntityVersioningInterceptor>();

        services.AddDbContext<AgoraDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
            var versionInterceptor = sp.GetRequiredService<EntityVersioningInterceptor>();

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "core");
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });

            options.AddInterceptors(auditInterceptor, versionInterceptor);
        });

        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<AgoraDbContext>());

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Servicio de consulta de AuditLog (implementación en Persistence por acceso IQueryable)
        services.AddScoped<IAuditLogService, AuditLogService>();

        return services;
    }
}
