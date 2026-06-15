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

    [HttpGet("facturacion-electronica/preflight/{ventaId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PreflightEmitirFE(long ventaId)
    {
        var connStr = _config.GetConnectionString("DefaultConnection") ?? "";
        var result = new Dictionary<string, object>();
        result["ventaId"] = ventaId;

        async Task<object?> Scalar(string sql)
        {
            try
            {
                await using var c = new SqlConnection(connStr);
                await c.OpenAsync();
                await using var cmd = c.CreateCommand();
                cmd.CommandText = sql;
                return await cmd.ExecuteScalarAsync();
            }
            catch (Exception ex) { return $"ERROR: {ex.Message}"; }
        }

        async Task<Dictionary<string, object>?> Row(string sql)
        {
            try
            {
                await using var c = new SqlConnection(connStr);
                await c.OpenAsync();
                await using var cmd = c.CreateCommand();
                cmd.CommandText = sql;
                await using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < r.FieldCount; i++)
                        row[r.GetName(i)] = r.IsDBNull(i) ? null : r.GetValue(i);
                    return row;
                }
                return null;
            }
            catch { return null; }
        }

        // 1. Venta
        var venta = await Row($"SELECT Id, EmpresaId, FechaVenta FROM vta.Ventas WHERE Id = {ventaId}");
        result["venta"] = venta;
        if (venta == null) { result["PuedeEmitir"] = false; result["MotivoBloqueo"] = "La venta no existe."; return Ok(result); }

        var ventaEmpresaId = venta.ContainsKey("EmpresaId") ? Convert.ToInt32(venta["EmpresaId"]) : 0;

        // 2. User info from JWT
        var user = _httpCtx.HttpContext?.User;
        var jwtEmpresaIdStr = user?.FindFirst("EmpresaId")?.Value ?? user?.FindFirst("empresaId")?.Value ?? "0";
        int.TryParse(jwtEmpresaIdStr, out var jwtEmpresaId);
        result["jwtEmpresaId"] = jwtEmpresaId;
        result["jwtUser"] = user?.Identity?.Name ?? "(anonymous)";
        result["jwtIsAuthenticated"] = user?.Identity?.IsAuthenticated ?? false;
        result["jwtClaims"] = user?.Claims.Select(c => $"{c.Type}={c.Value}").ToList() ?? new();

        // 3. EmpresaId check
        result["empresaIdMatch"] = ventaEmpresaId == jwtEmpresaId;
        if (ventaEmpresaId != jwtEmpresaId)
        {
            result["PuedeEmitir"] = false;
            result["MotivoBloqueo"] = $"La venta pertenece a EmpresaId={ventaEmpresaId} pero el JWT tiene EmpresaId={jwtEmpresaId}. El usuario solo puede emitir facturas de su propia empresa.";
            return Ok(result);
        }

        // 4. VentaFacturacionDatos
        var factDatos = await Row($"SELECT Id, EmpresaId, Facturar, NitFactura, RazonSocialFactura, EstadoFactura, FacturaId, ClientePerfilFiscalId FROM vta.VentaFacturacionDatos WHERE VentaId = {ventaId}");
        result["ventaFacturacionDatos"] = factDatos;
        if (factDatos == null)
        {
            result["PuedeEmitir"] = false;
            result["MotivoBloqueo"] = "La venta no tiene datos de facturación (VentaFacturacionDatos). Complete los datos fiscales antes de emitir.";
            return Ok(result);
        }

        var factDatosEmpresaId = factDatos.ContainsKey("EmpresaId") ? Convert.ToInt32(factDatos["EmpresaId"]) : 0;
        result["factDatosEmpresaIdMatch"] = factDatosEmpresaId == jwtEmpresaId;
        if (factDatosEmpresaId != jwtEmpresaId)
        {
            result["PuedeEmitir"] = false;
            result["MotivoBloqueo"] = $"VentaFacturacionDatos pertenece a EmpresaId={factDatosEmpresaId} pero JWT tiene EmpresaId={jwtEmpresaId}.";
            return Ok(result);
        }

        var nitFactura = factDatos.ContainsKey("NitFactura") ? factDatos["NitFactura"]?.ToString() : null;
        var razonSocial = factDatos.ContainsKey("RazonSocialFactura") ? factDatos["RazonSocialFactura"]?.ToString() : null;
        var facturar = factDatos.ContainsKey("Facturar") ? Convert.ToBoolean(factDatos["Facturar"]) : false;
        var factDatosEstadoFactura = factDatos.ContainsKey("EstadoFactura") ? Convert.ToInt32(factDatos["EstadoFactura"]) : 0;
        var facturaIdExistente = factDatos.ContainsKey("FacturaId") ? factDatos["FacturaId"] : null;
        var clientePerfilFiscalId = factDatos.ContainsKey("ClientePerfilFiscalId") ? factDatos["ClientePerfilFiscalId"] : null;

        // 5. Campos fiscales obligatorios
        var missingFields = new List<string>();
        if (facturar && string.IsNullOrWhiteSpace(nitFactura)) missingFields.Add("NitFactura");
        if (facturar && string.IsNullOrWhiteSpace(razonSocial)) missingFields.Add("RazonSocialFactura");
        result["missingFiscalFields"] = missingFields;

        // 6. ClientePerfilFiscal
        if (clientePerfilFiscalId != null && clientePerfilFiscalId != DBNull.Value)
        {
            var perfilFiscal = await Row($"SELECT Id, EmpresaId, ClienteId FROM mdm.ClientePerfilesFiscales WHERE Id = {clientePerfilFiscalId}");
            result["clientePerfilFiscal"] = perfilFiscal;
            if (perfilFiscal != null)
            {
                var perfilEmpresaId = perfilFiscal.ContainsKey("EmpresaId") ? Convert.ToInt32(perfilFiscal["EmpresaId"]) : 0;
                result["perfilFiscalEmpresaIdMatch"] = perfilEmpresaId == jwtEmpresaId;
            }
        }
        else
        {
            result["clientePerfilFiscal"] = null;
        }

        // 7. Configuración FE activa
        var configFe = await Row($"SELECT TOP 1 c.Id, c.EmpresaId, c.EsConfiguracionActiva, c.NitEmisor, c.ActivityCode, p.Codigo AS ProveedorCodigo, p.Nombre AS ProveedorNombre, a.Codigo AS AmbienteCodigo FROM cfg.ConfiguracionFacturacionElectronica c LEFT JOIN cfg.ProveedoresFacturacionElectronica p ON p.Id = c.ProveedorFacturacionElectronicaId LEFT JOIN cfg.AmbientesFacturacionElectronica a ON a.Id = c.AmbienteFacturacionElectronicaId WHERE c.EmpresaId = {jwtEmpresaId} AND c.EsConfiguracionActiva = 1 AND c.Activo = 1");
        result["configuracionFE"] = configFe;
        if (configFe == null)
        {
            result["PuedeEmitir"] = false;
            result["MotivoBloqueo"] = $"No existe configuración FE activa para EmpresaId={jwtEmpresaId}. Solo hay configuración para EmpresaId=27.";
            return Ok(result);
        }

        var configEmpresaId = configFe.ContainsKey("EmpresaId") ? Convert.ToInt32(configFe["EmpresaId"]) : 0;
        result["configEmpresaIdMatch"] = configEmpresaId == jwtEmpresaId;

        // 8. Factura previa
        var facturaPrevia = await Row($"SELECT Id, NumeroFactura, EstadoFactura, EstadoSiat, BillUuid, Cuf FROM vta.FacturasVenta WHERE VentaId = {ventaId} ORDER BY Id DESC");
        result["facturaPrevia"] = facturaPrevia;

        // 9. Detalles de venta
        var detallesCount = await Scalar($"SELECT COUNT(*) FROM vta.VentaDetalles WHERE VentaId = {ventaId}");
        result["detallesCount"] = detallesCount;
        if (detallesCount is int dCount && dCount == 0)
        {
            result["PuedeEmitir"] = false;
            result["MotivoBloqueo"] = "La venta no tiene detalles.";
            return Ok(result);
        }

        // 10. Resultado final
        var reasons = new List<string>();
        if (factDatosEstadoFactura == 2) reasons.Add("La venta ya tiene factura generada (EstadoFactura=Generada).");
        if (facturaIdExistente != null && facturaIdExistente != DBNull.Value) reasons.Add($"Ya existe FacturaVentaId={facturaIdExistente} vinculada a esta venta.");
        if (missingFields.Count > 0) reasons.Add($"Faltan campos fiscales: {string.Join(", ", missingFields)}");
        if (!facturar) reasons.Add("La venta no está marcada para facturar (Facturar=false).");

        result["PuedeEmitir"] = reasons.Count == 0;
        result["MotivoBloqueo"] = reasons.Count > 0 ? string.Join(" | ", reasons) : "Puede emitir. Todos los chequeos pasaron.";
        return Ok(result);
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
