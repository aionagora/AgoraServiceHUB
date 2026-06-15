using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using AgoraHub360.ERP.Domain.Entities.FE;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AgoraHub360.ERP.Api.Controllers.V1;

[ApiController]
[Route("api/v1/diagnostico")]
[AllowAnonymous]
public class DiagnosticoController : ControllerBase
{
    private readonly AgoraDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpCtx;
    private readonly IConfiguration _config;
    private readonly ILogger<DiagnosticoController> _logger;

    public DiagnosticoController(
        AgoraDbContext db,
        IWebHostEnvironment env,
        IHttpContextAccessor httpCtx,
        IConfiguration config,
        ILogger<DiagnosticoController> logger)
    {
        _db = db;
        _env = env;
        _httpCtx = httpCtx;
        _config = config;
        _logger = logger;
    }

    [HttpGet("runtime")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRuntimeInfo()
    {
        var apiAsm = Assembly.GetExecutingAssembly();
        var process = Process.GetCurrentProcess();
        var connStr = _config.GetConnectionString("DefaultConnection") ?? "";

        var maskedConn = Regex.Replace(connStr, @"Password\s*=\s*[^;]+", "Password=***", RegexOptions.IgnoreCase);
        maskedConn = Regex.Replace(maskedConn, @"Pwd\s*=\s*[^;]+", "Pwd=***", RegexOptions.IgnoreCase);
        var connBuilder = new SqlConnectionStringBuilder(connStr);

        string dbName = "ERROR", serverName = "ERROR", suser = "ERROR", sysUser = "ERROR";
        if (!string.IsNullOrEmpty(connStr))
        {
            try
            {
                await using var sqlConn = new SqlConnection(connStr);
                await sqlConn.OpenAsync();
                await using var cmd = sqlConn.CreateCommand();
                cmd.CommandText = "SELECT DB_NAME(), @@SERVERNAME, SUSER_NAME(), SYSTEM_USER";
                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    dbName = reader.GetString(0);
                    serverName = reader.GetString(1);
                    suser = reader.IsDBNull(2) ? "N/A" : reader.GetString(2);
                    sysUser = reader.IsDBNull(3) ? "N/A" : reader.GetString(3);
                }
                await reader.CloseAsync();
            }
            catch (Exception ex)
            {
                dbName = $"ERROR: {ex.Message}";
            }
        }

        var info = new
        {
            EnvironmentName = _env.EnvironmentName,
            MachineName = Environment.MachineName,
            ProcessId = process.Id,
            ProcessStartTime = TimeZoneInfo.ConvertTimeFromUtc(process.StartTime.ToUniversalTime(), TimeZoneInfo.Local).ToString("yyyy-MM-dd HH:mm:ss"),
            AssemblyName = apiAsm.GetName().Name,
            AssemblyLocation = apiAsm.Location,
            AssemblyVersion = apiAsm.GetName().Version?.ToString() ?? "N/A",
            AssemblyInformationalVersion = apiAsm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "N/A",
            CurrentDirectory = Environment.CurrentDirectory,
            ContentRootPath = _env.ContentRootPath,
            WebRootPath = _env.WebRootPath,
            ConnectionString = maskedConn,
            ConnectionServer = connBuilder.DataSource,
            ConnectionDatabase = connBuilder.InitialCatalog,
            ConnectionUserId = connBuilder.UserID,
            DatabaseName = dbName,
            ServerName = serverName,
            SuserName = suser,
            SystemUser = sysUser,
            AspNetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "(not set)",
            Urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "(not set - uses launchSettings)",
            IsDevelopment = _env.IsDevelopment(),
            OsDescription = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            Framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
        };

        return Ok(info);
    }

    [HttpGet("facturacion-electronica")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFacturacionDiagnostico()
    {
        var result = new Dictionary<string, object>();
        var connStr = _config.GetConnectionString("DefaultConnection") ?? "";

        // 1. EF Runtime Mapping
        var entities = new (string Name, Type Type)[] {
            ("AmbienteFacturacionElectronica", typeof(AmbienteFacturacionElectronica)),
            ("ProveedorFacturacionElectronica", typeof(ProveedorFacturacionElectronica)),
            ("ConfiguracionFacturacionElectronica", typeof(ConfiguracionFacturacionElectronica)),
            ("AuditoriaFacturacion", typeof(AuditoriaFacturacion)),
            ("FacturaVenta", typeof(FacturaVenta)),
            ("FacturaVentaDetalle", typeof(FacturaVentaDetalle)),
            ("VentaFacturacionDatos", typeof(VentaFacturacionDatos)),
            ("SiatMetodoPago", typeof(SiatMetodoPago)),
        };

        var mapping = new List<object>();
        foreach (var (name, type) in entities)
        {
            var et = _db.Model.FindEntityType(type);
            if (et != null)
            {
                var schema = et.FindAnnotation("Relational:Schema")?.Value?.ToString() ?? "dbo";
                var table = et.FindAnnotation("Relational:TableName")?.Value?.ToString() ?? et.ShortName();
                mapping.Add(new { Entity = name, Schema = schema, Table = table, Assembly = et.ClrType.Assembly.GetName().Name });
            }
            else
            {
                mapping.Add(new { Entity = name, Schema = "NOT FOUND", Table = "NOT FOUND", Assembly = "" });
            }
        }
        result["EfRuntimeMapping"] = mapping;

        // Helper to run SQL
        async Task<List<Dictionary<string, object>>> ExecSql(string sql)
        {
            var rows = new List<Dictionary<string, object>>();
            try
            {
                await using var sqlConn = new SqlConnection(connStr);
                await sqlConn.OpenAsync();
                await using var cmd = sqlConn.CreateCommand();
                cmd.CommandText = sql;
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                rows.Add(new Dictionary<string, object> { { "Error", ex.Message } });
            }
            return rows;
        }

        async Task<object?> ExecScalar(string sql)
        {
            try
            {
                await using var sqlConn = new SqlConnection(connStr);
                await sqlConn.OpenAsync();
                await using var cmd = sqlConn.CreateCommand();
                cmd.CommandText = sql;
                return await cmd.ExecuteScalarAsync();
            }
            catch (Exception ex) { return $"ERROR: {ex.Message}"; }
        }

        // 2. Table existence
        var tables = new[] {
            ("cfg", "AmbientesFacturacionElectronica"), ("cfg", "ProveedoresFacturacionElectronica"),
            ("cfg", "ConfiguracionFacturacionElectronica"), ("cfg", "AuditoriaFacturacion"),
            ("vta", "FacturasVenta"), ("vta", "FacturaVentaDetalles"),
            ("vta", "VentaFacturacionDatos"), ("vta", "SiatMetodosPago"),
            ("dbo", "FacturasVenta"),
        };
        var tableExistence = new List<object>();
        foreach (var (schema, table) in tables)
        {
            var val = await ExecScalar($"SELECT COUNT(*) FROM sys.tables t INNER JOIN sys.schemas s ON s.schema_id = t.schema_id WHERE s.name = '{schema}' AND t.name = '{table}'");
            tableExistence.Add(new { Schema = schema, Table = table, Exists = (val is int i && i > 0) });
        }
        result["TableExistenceInDb"] = tableExistence;

        // 3. Record counts
        var countQueries = new (string label, string sql)[] {
            ("cfg.ConfiguracionFacturacionElectronica (activas)", "SELECT COUNT(*) FROM cfg.ConfiguracionFacturacionElectronica WHERE EsConfiguracionActiva = 1"),
            ("cfg.ConfiguracionFacturacionElectronica (total)", "SELECT COUNT(*) FROM cfg.ConfiguracionFacturacionElectronica"),
            ("cfg.AmbientesFacturacionElectronica", "SELECT COUNT(*) FROM cfg.AmbientesFacturacionElectronica"),
            ("cfg.ProveedoresFacturacionElectronica", "SELECT COUNT(*) FROM cfg.ProveedoresFacturacionElectronica"),
            ("cfg.AuditoriaFacturacion", "SELECT COUNT(*) FROM cfg.AuditoriaFacturacion"),
            ("vta.FacturasVenta", "SELECT COUNT(*) FROM vta.FacturasVenta"),
            ("dbo.FacturasVenta (legacy)", "SELECT COUNT(*) FROM dbo.FacturasVenta"),
            ("vta.FacturaVentaDetalles", "SELECT COUNT(*) FROM vta.FacturaVentaDetalles"),
            ("vta.VentaFacturacionDatos", "SELECT COUNT(*) FROM vta.VentaFacturacionDatos"),
            ("vta.SiatMetodosPago", "SELECT COUNT(*) FROM vta.SiatMetodosPago"),
            ("vta.Ventas", "SELECT COUNT(*) FROM vta.Ventas"),
        };
        var counts = new List<object>();
        foreach (var (label, sql) in countQueries)
            counts.Add(new { Table = label, Count = await ExecScalar(sql) });
        result["RecordCounts"] = counts;

        // 4. Configuraciones
        result["ConfiguracionesFE"] = await ExecSql(@"
            SELECT c.Id, c.NombreConfiguracion, c.EmpresaId, c.EsConfiguracionActiva,
                   p.Codigo AS ProveedorCodigo, a.Codigo AS AmbienteCodigo
            FROM cfg.ConfiguracionFacturacionElectronica c
            LEFT JOIN cfg.ProveedoresFacturacionElectronica p ON p.Id = c.ProveedorFacturacionElectronicaId
            LEFT JOIN cfg.AmbientesFacturacionElectronica a ON a.Id = c.AmbienteFacturacionElectronicaId
            ORDER BY c.Id DESC");

        // 5. Config activa detalle
        result["ConfiguracionActiva"] = await ExecSql(@"
            SELECT TOP 1 c.Id, c.NombreConfiguracion, c.EmpresaId, c.EsConfiguracionActiva,
                   c.ProveedorFacturacionElectronicaId, c.AmbienteFacturacionElectronicaId,
                   c.NitEmisor, c.ActivityCode, c.SucursalFiscal, c.PuntoVentaFiscal,
                   c.TimeoutSegundos,
                   LEFT(c.ClientSecretEncrypted, 20) + '...' AS ClientSecretPreview
            FROM cfg.ConfiguracionFacturacionElectronica c
            WHERE c.EsConfiguracionActiva = 1 ORDER BY c.Id DESC");

        // 6. Latest FacturasVenta
        result["LatestFacturasVenta"] = await ExecSql(
            "SELECT TOP 10 Id, NumeroFactura, EstadoFactura, EstadoSiat, ISNULL(BillUuid,'N/A') AS BillUuid, ISNULL(Cuf,'N/A') AS Cuf, EmpresaId, FechaEmision FROM vta.FacturasVenta ORDER BY Id DESC");

        // 7. Latest Auditoria
        result["LatestAuditoriaFacturacion"] = await ExecSql(
            "SELECT TOP 10 Id, FacturaVentaId, ProveedorCodigo, AmbienteCodigo, BillUuid, Cuf, EstadoSiat, Exitoso, FechaHora FROM cfg.AuditoriaFacturacion ORDER BY Id DESC");

        // 8. Migrations
        result["Migrations"] = await ExecSql(
            "SELECT TOP 10 MigrationId, ProductVersion FROM dbo.__EFMigrationsHistory ORDER BY MigrationId DESC");

        // 9. User info
        var httpContext = _httpCtx.HttpContext;
        var user = httpContext?.User;
        result["UserInfo"] = new
        {
            IsAuthenticated = user?.Identity?.IsAuthenticated ?? false,
            UserName = user?.Identity?.Name ?? "(anonymous)",
            AuthenticationType = user?.Identity?.AuthenticationType ?? "(none)",
            Claims = (user?.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList()
                as IEnumerable<object>) ?? new List<object>(),
        };

        return Ok(result);
    }
}
