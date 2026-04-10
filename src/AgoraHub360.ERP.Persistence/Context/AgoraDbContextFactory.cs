namespace AgoraHub360.ERP.Persistence.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

/// <summary>
/// Factory para crear AgoraDbContext en design-time (migraciones).
/// Usa el constructor sin ICurrentUserService para evitar dependencias de runtime.
/// Lee la cadena de conexión desde appsettings.json del proyecto API.
/// </summary>
public class AgoraDbContextFactory : IDesignTimeDbContextFactory<AgoraDbContext>
{
    public AgoraDbContext CreateDbContext(string[] args)
    {
        // Construir configuración desde appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "AgoraHub360.ERP.Api"))
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<AgoraDbContext>();

        optionsBuilder.UseSqlServer(
            connectionString,
            sqlOptions => sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "core"));

        return new AgoraDbContext(optionsBuilder.Options);
    }
}
