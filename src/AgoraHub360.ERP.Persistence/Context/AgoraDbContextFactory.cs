namespace AgoraHub360.ERP.Persistence.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Factory para crear AgoraDbContext en design-time (migraciones).
/// Usa el constructor sin ICurrentUserService para evitar dependencias de runtime.
/// </summary>
public class AgoraDbContextFactory : IDesignTimeDbContextFactory<AgoraDbContext>
{
    public AgoraDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AgoraDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=AgoraHub360;Trusted_Connection=true;TrustServerCertificate=true",
            sqlOptions => sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "core"));

        return new AgoraDbContext(optionsBuilder.Options);
    }
}
