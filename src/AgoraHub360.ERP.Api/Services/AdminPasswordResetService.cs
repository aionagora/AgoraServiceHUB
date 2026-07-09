using System.Security.Cryptography;
using System.Text;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AgoraHub360.ERP.Api.Services;

/// <summary>
/// Servicio temporal de desarrollo para resetear la contraseña del usuario admin.
/// Solo debe ejecutarse en entorno Development.
/// 
/// Configuración (user-secrets o appsettings.Development.json):
///   - AdminPasswordReset:Enabled   (bool, default: true)
///   - AdminPasswordReset:Email     (string, default: admin@agorahub360.com)
///   - AdminPasswordReset:Password  (string, obligatorio)
/// 
/// Ejemplo:
///   dotnet user-secrets set "AdminPasswordReset:Password" "Admin123.**"
/// 
/// CÓMO RETIRAR ESTE CÓDIGO TEMPORAL:
/// 1. Eliminar este archivo (AdminPasswordResetService.cs)
/// 2. En Program.cs, eliminar el bloque "Admin password reset (Development only)"
/// 3. Opcional: limpiar user-secrets:
///    dotnet user-secrets remove "AdminPasswordReset:Password"
///    dotnet user-secrets remove "AdminPasswordReset:Email"
/// </summary>
public class AdminPasswordResetService
{
    private readonly AgoraDbContext _context;
    private readonly ILogger<AdminPasswordResetService> _logger;
    private readonly IConfiguration _configuration;

    public AdminPasswordResetService(
        AgoraDbContext context,
        ILogger<AdminPasswordResetService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task ResetAdminPasswordAsync()
    {
        var enabled = _configuration.GetValue<bool>("AdminPasswordReset:Enabled", true);
        if (!enabled)
        {
            _logger.LogInformation("ℹ️ AdminPasswordReset:Enabled=false. Saltando reseteo de contraseña.");
            return;
        }

        var targetEmail = _configuration["AdminPasswordReset:Email"] ?? "admin@agorahub360.com";
        var newPassword = _configuration["AdminPasswordReset:Password"];

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            _logger.LogWarning("⚠️ AdminPasswordReset:Password no configurado. " +
                "Establecer en user-secrets. " +
                "Ejemplo: dotnet user-secrets set \"AdminPasswordReset:Password\" \"NuevaClave123\"");
            return;
        }

        try
        {
            var adminUser = await _context.Usuarios
                .Where(u => u.Email == targetEmail)
                .FirstOrDefaultAsync();

            if (adminUser is null)
            {
                _logger.LogWarning("⚠️ Usuario {Email} no encontrado. " +
                    "Se creará automáticamente (seed de desarrollo).", targetEmail);

                adminUser = await CrearAdminUserAsync(targetEmail);
                if (adminUser is null)
                {
                    _logger.LogError("❌ No se pudo crear el usuario admin. " +
                        "Verifique que exista una empresa con Id=1 en la tabla core.Empresas.");
                    return;
                }
            }

            var newHash = HashPassword(newPassword);

            if (adminUser.PasswordHash == newHash)
            {
                _logger.LogInformation("ℹ️ La contraseña de {Email} ya está actualizada.", targetEmail);
                return;
            }

            adminUser.PasswordHash = newHash;
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Contraseña restablecida exitosamente para {Email}", targetEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al restablecer contraseña de admin");
        }
    }

    private async Task<Usuario?> CrearAdminUserAsync(string email)
    {
        // Verificar si existe la empresa demo (Id=1) — la crea la migración AddDefaultAdminUser
        var empresaDemo = await _context.Empresas.FindAsync(1);
        if (empresaDemo is null)
            return null;

        var adminUser = new Usuario
        {
            NombreUsuario = "admin",
            Email = email,
            PasswordHash = HashPassword(_configuration["AdminPasswordReset:Password"]!),
            NombreCompleto = "Administrador del Sistema",
            EmpresaActivaId = 1,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _context.Usuarios.AddAsync(adminUser);
        await _context.SaveChangesAsync();

        // Asignar rol Admin en la empresa demo
        var asignacion = new UsuarioEmpresa
        {
            UsuarioId = adminUser.Id,
            EmpresaId = 1,
            Rol = "Admin"
        };
        await _context.UsuarioEmpresas.AddAsync(asignacion);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ Usuario admin creado: {Email} (Id={Id})", email, adminUser.Id);
        return adminUser;
    }

    /// <summary>
    /// Genera hash SHA256 (Base64) — mismo mecanismo que AuthService.HashPassword.
    /// </summary>
    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
