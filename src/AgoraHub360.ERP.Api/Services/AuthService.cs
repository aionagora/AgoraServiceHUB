namespace AgoraHub360.ERP.Api.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.Constants;
using AgoraHub360.ERP.Shared.DTOs.Auth;
using AgoraHub360.ERP.Shared.DTOs;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Servicio de autenticación JWT básico.
/// Valida credenciales contra la tabla Usuarios y genera tokens JWT.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IRepository<Usuario> _userRepo;
    private readonly IRepository<UsuarioEmpresa> _ueRepo;
    private readonly IRepository<Empresa> _empresaRepo;
    private readonly IConfiguration _config;

    public AuthService(
        IRepository<Usuario> userRepo,
        IRepository<UsuarioEmpresa> ueRepo,
        IRepository<Empresa> empresaRepo,
        IConfiguration config)
    {
        _userRepo = userRepo;
        _ueRepo = ueRepo;
        _empresaRepo = empresaRepo;
        _config = config;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var users = await _userRepo.FindAsync(u => u.Email == request.Email && u.Activo, ct);
        var user = users.FirstOrDefault();
        if (user is null)
            return null;

        if (user.PasswordHash != HashPassword(request.Password))
            return null;

        var platformRole = ResolvePlatformRole(user);
        var isPlatformAdmin = IsPlatformAdmin(platformRole);

        var empresasAsignadas = await _ueRepo.FindAsync(ue => ue.UsuarioId == user.Id, ct);
        var empresaIds = empresasAsignadas.Select(e => e.EmpresaId).Distinct().ToList();
        var empresasActivas = await _empresaRepo.FindAsync(e => empresaIds.Contains(e.Id) && e.Activo, ct);
        var empresasActivasAsignadas = empresasAsignadas.Where(e => empresasActivas.Any(a => a.Id == e.EmpresaId)).ToList();

        if (!isPlatformAdmin && empresasActivasAsignadas.Count == 0)
            return null;

        int? tenantId = null;
        string tenantRole = Roles.NoAccess;
        var tenantStatus = TenantStatus.NotSelected;
        UsuarioEmpresa? empresaSeleccionada = null;

        if (empresasActivasAsignadas.Count == 1)
        {
            empresaSeleccionada = empresasActivasAsignadas[0];
            user.EmpresaActivaId = empresaSeleccionada.EmpresaId;
        }
        else if (user.EmpresaActivaId.HasValue)
        {
            empresaSeleccionada = empresasActivasAsignadas.FirstOrDefault(e => e.EmpresaId == user.EmpresaActivaId.Value);
        }

        if (empresaSeleccionada is null && empresasActivasAsignadas.Count > 1)
        {
            // Mantener tenant no seleccionado para forzar elección segura cuando hay múltiples empresas
            tenantStatus = TenantStatus.NotSelected;
            user.EmpresaActivaId = null;
        }
        else if (empresaSeleccionada is null && !isPlatformAdmin)
        {
            return null;
        }

        if (empresaSeleccionada is not null)
        {
            tenantId = empresaSeleccionada.EmpresaId;
            tenantRole = NormalizeTenantRole(empresaSeleccionada.Rol);
            tenantStatus = TenantStatus.Selected;
        }

        var token = GenerateJwtToken(user, platformRole, tenantId, tenantRole, tenantStatus);
        var expiration = DateTime.UtcNow.AddHours(
            _config.GetValue<int>("Jwt:ExpirationHours", 8));

        await _userRepo.UpdateAsync(user, ct);

        var empresasDisponibles = empresasActivas
            .Join(empresasActivasAsignadas,
                e => e.Id,
                ue => ue.EmpresaId,
                (e, ue) => new EmpresaSesionDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Nit = e.NIT,
                    Rol = NormalizeTenantRole(ue.Rol),
                    EsActiva = tenantId.HasValue && tenantId.Value == e.Id
                })
            .OrderBy(e => e.Nombre)
            .ToList();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = "",
            Expiration = expiration,
            NombreUsuario = user.NombreUsuario,
            Email = user.Email,
            PlatformRole = platformRole,
            TenantId = tenantId,
            TenantRole = tenantRole,
            TenantStatus = tenantStatus,
            EmpresasDisponibles = empresasDisponibles.AsReadOnly()
        };
    }

    public Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        // No implementado en versión mínima
        return Task.FromResult<AuthResponseDto?>(null);
    }

    public async Task<Result<CambiarEmpresaResponseDto>> CambiarEmpresaActivaAsync(int usuarioId, int empresaId, CancellationToken ct = default)
    {
        var usuario = await _userRepo.GetByIdAsync(usuarioId, ct);
        if (usuario is null || !usuario.Activo)
            return Result<CambiarEmpresaResponseDto>.Failure("Usuario no encontrado o inactivo.");

        var asignaciones = await _ueRepo.FindAsync(ue => ue.UsuarioId == usuarioId, ct);
        var empresaIdsAsignadas = asignaciones.Select(a => a.EmpresaId).Distinct().ToList();
        var empresasActivas = await _empresaRepo.FindAsync(e => empresaIdsAsignadas.Contains(e.Id) && e.Activo, ct);

        // Seguridad: solo se considera membresía efectiva si la empresa asignada está activa.
        var asignacionesActivas = asignaciones
            .Where(a => empresasActivas.Any(e => e.Id == a.EmpresaId))
            .ToList();

        var asignacion = asignacionesActivas.FirstOrDefault(a => a.EmpresaId == empresaId);
        if (asignacion is null)
            return Result<CambiarEmpresaResponseDto>.Failure("La empresa seleccionada no pertenece al usuario.");

        var empresa = await _empresaRepo.GetByIdAsync(empresaId, ct);
        if (empresa is null || !empresa.Activo)
            return Result<CambiarEmpresaResponseDto>.Failure("La empresa seleccionada no está activa.");

        usuario.EmpresaActivaId = empresaId;
        await _userRepo.UpdateAsync(usuario, ct);

        var platformRole = ResolvePlatformRole(usuario);
        var tenantRole = NormalizeTenantRole(asignacion.Rol);

        var token = GenerateJwtToken(usuario, platformRole, empresaId, tenantRole, TenantStatus.Selected);
        var expiration = DateTime.UtcNow.AddHours(
            _config.GetValue<int>("Jwt:ExpirationHours", 8));

        var empresasDisponibles = asignacionesActivas.Select(a => new EmpresaSesionDto
        {
            Id = a.EmpresaId,
            Nombre = empresasActivas.FirstOrDefault(e => e.Id == a.EmpresaId)?.Nombre ?? $"Empresa #{a.EmpresaId}",
            Nit = empresasActivas.FirstOrDefault(e => e.Id == a.EmpresaId)?.NIT,
            Rol = NormalizeTenantRole(a.Rol),
            EsActiva = a.EmpresaId == empresaId
        }).ToList();

        var response = new CambiarEmpresaResponseDto
        {
            Token = token,
            Expiration = expiration,
            PlatformRole = platformRole,
            TenantId = empresaId,
            TenantRole = tenantRole,
            TenantStatus = TenantStatus.Selected,
            EmpresaActiva = new EmpresaSesionDto
            {
                Id = empresa.Id,
                Nombre = empresa.Nombre,
                Nit = empresa.NIT,
                Rol = tenantRole,
                EsActiva = true
            },
            EmpresasDisponibles = empresasDisponibles.AsReadOnly(),
            SucursalesDisponibles = Array.Empty<SucursalSesionDto>()
        };

        return Result<CambiarEmpresaResponseDto>.Success(response);
    }

    private string GenerateJwtToken(Usuario user, string platformRole, int? tenantId, string tenantRole, string tenantStatus)
    {
        var key = _config["Jwt:Key"] ?? "AgoraHub360-ERP-Dev-Secret-Key-2026-MinLength32!";
        var issuer = _config["Jwt:Issuer"] ?? "AgoraHub360.ERP";
        var audience = _config["Jwt:Audience"] ?? "AgoraHub360.ERP.Web";
        var hours = _config.GetValue<int>("Jwt:ExpirationHours", 8);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.NombreUsuario),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, tenantRole), // compatibilidad temporal
            new(ClaimTypesCustom.PlatformRole, platformRole),
            new(ClaimTypesCustom.TenantRole, tenantRole),
            new(ClaimTypesCustom.TenantStatus, tenantStatus),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };

        if (tenantId.HasValue)
        {
            claims.Add(new Claim(ClaimTypesCustom.TenantId, tenantId.Value.ToString()));
            claims.Add(new Claim(ClaimTypesCustom.EmpresaId, tenantId.Value.ToString())); // compatibilidad legacy
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(hours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool IsPlatformAdmin(string platformRole)
        => string.Equals(platformRole, Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase)
           || string.Equals(platformRole, Roles.SystemAdmin, StringComparison.OrdinalIgnoreCase);

    private static string ResolvePlatformRole(Usuario user)
    {
        // Compatibilidad temporal: inferir platform role a partir del usuario actual
        if (string.Equals(user.Email, "admin@agorahub360.com", StringComparison.OrdinalIgnoreCase))
            return Roles.SuperAdmin;

        if (string.Equals(user.NombreUsuario, "superadmin", StringComparison.OrdinalIgnoreCase))
            return Roles.SuperAdmin;

        if (string.Equals(user.NombreUsuario, "admin", StringComparison.OrdinalIgnoreCase))
            return Roles.SystemAdmin;

        return Roles.None;
    }

    private static string NormalizeTenantRole(string? rol)
    {
        if (string.IsNullOrWhiteSpace(rol))
            return Roles.NoAccess;

        return rol.Trim() switch
        {
            Roles.Admin => Roles.AdminEmpresa,
            Roles.Manager => Roles.Supervisor,
            Roles.User => Roles.Operador,
            Roles.Viewer => Roles.Viewer,
            _ => rol.Trim()
        };
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
