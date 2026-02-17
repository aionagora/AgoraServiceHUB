namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Persistence.Context;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Controller de diagnóstico para verificar que la API funciona.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly AgoraDbContext _dbContext;

    public DiagnosticsController(AgoraDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Ping simple para verificar conectividad.
    /// </summary>
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping()
    {
        return Ok(new
        {
            status = "ok",
            timestamp = DateTime.UtcNow,
            version = "1.0"
        });
    }

    /// <summary>
    /// Endpoint protegido para verificar que la autenticación funciona.
    /// Devuelve los claims del usuario autenticado.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(new
        {
            user = User.Identity?.Name,
            claims
        });
    }

    /// <summary>
    /// Prueba completa de conexión a la base de datos.
    /// Verifica conectividad, permisos de lectura y datos iniciales.
    /// </summary>
    [HttpGet("database-test")]
    [AllowAnonymous]
    public async Task<IActionResult> DatabaseTest()
    {
        var result = new
        {
            timestamp = DateTime.UtcNow,
            status = "unknown",
            connectionString = MaskConnectionString(
                _dbContext.Database.GetConnectionString() ?? "No connection string"),
            tests = new List<object>()
        };

        try
        {
            // Test 1: Conectividad básica
            var canConnect = await _dbContext.Database.CanConnectAsync();
            ((List<object>)result.tests).Add(new
            {
                test = "Database Connectivity",
                status = canConnect ? "✅ PASS" : "❌ FAIL",
                message = canConnect ? "Conexión exitosa" : "No se pudo conectar"
            });

            if (!canConnect)
            {
                return Ok(new
                {
                    result.timestamp,
                    status = "error",
                    result.connectionString,
                    result.tests,
                    message = "No se pudo establecer conexión con la base de datos"
                });
            }

            // Test 2: Leer Monedas (seed data)
            var monedasCount = await _dbContext.Monedas.CountAsync();
            ((List<object>)result.tests).Add(new
            {
                test = "Read Monedas Table",
                status = monedasCount > 0 ? "✅ PASS" : "⚠️ WARNING",
                message = $"Se encontraron {monedasCount} monedas",
                data = await _dbContext.Monedas
                    .Select(m => new { m.Codigo, m.Nombre, m.Simbolo })
                    .ToListAsync()
            });

            // Test 3: Leer Roles (seed data)
            var rolesCount = await _dbContext.Roles.CountAsync();
            ((List<object>)result.tests).Add(new
            {
                test = "Read Roles Table",
                status = rolesCount > 0 ? "✅ PASS" : "⚠️ WARNING",
                message = $"Se encontraron {rolesCount} roles",
                data = await _dbContext.Roles
                    .Select(r => new { r.Id, r.Nombre, r.Descripcion })
                    .ToListAsync()
            });

            // Test 4: Leer Empresas
            var empresasCount = await _dbContext.Empresas.CountAsync();
            ((List<object>)result.tests).Add(new
            {
                test = "Read Empresas Table",
                status = empresasCount >= 0 ? "✅ PASS" : "❌ FAIL",
                message = $"Se encontraron {empresasCount} empresas"
            });

            // Test 5: Verificar migraciones aplicadas
            var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
            var appliedMigrations = await _dbContext.Database.GetAppliedMigrationsAsync();
            ((List<object>)result.tests).Add(new
            {
                test = "Database Migrations",
                status = !pendingMigrations.Any() ? "✅ PASS" : "⚠️ WARNING",
                message = pendingMigrations.Any() 
                    ? $"Hay {pendingMigrations.Count()} migraciones pendientes"
                    : "Todas las migraciones están aplicadas",
                appliedCount = appliedMigrations.Count(),
                pendingCount = pendingMigrations.Count(),
                pending = pendingMigrations.ToList()
            });

            return Ok(new
            {
                result.timestamp,
                status = "success",
                result.connectionString,
                result.tests,
                message = "Todos los tests de base de datos completados",
                summary = new
                {
                    totalTests = result.tests.Count,
                    passed = result.tests.Count(t => ((dynamic)t).status.ToString().Contains("✅")),
                    warnings = result.tests.Count(t => ((dynamic)t).status.ToString().Contains("⚠️")),
                    failed = result.tests.Count(t => ((dynamic)t).status.ToString().Contains("❌"))
                }
            });
        }
        catch (Exception ex)
        {
            ((List<object>)result.tests).Add(new
            {
                test = "Database Connection",
                status = "❌ FAIL",
                message = "Error al ejecutar tests",
                error = ex.Message,
                innerError = ex.InnerException?.Message
            });

            return Ok(new
            {
                result.timestamp,
                status = "error",
                result.connectionString,
                result.tests,
                message = $"Error: {ex.Message}",
                stackTrace = ex.StackTrace
            });
        }
    }

    /// <summary>
    /// Enmascara la cadena de conexión para ocultar credenciales sensibles.
    /// </summary>
    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "No connection string";

        // Ocultar Password
        var masked = System.Text.RegularExpressions.Regex.Replace(
            connectionString,
            @"(Password|Pwd)=([^;]+)",
            "$1=***",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Ocultar User Id si existe
        masked = System.Text.RegularExpressions.Regex.Replace(
            masked,
            @"(User Id|UID)=([^;]+)",
            "$1=***",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return masked;
    }
}
