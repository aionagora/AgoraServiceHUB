namespace AgoraHub360.ERP.Api.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
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
        // Buscar usuario por email
        var users = await _userRepo.FindAsync(u => u.Email == request.Email && u.Activo, ct);
        var user = users.FirstOrDefault();
        if (user is null)
            return null;

        // Verificar password
        if (user.PasswordHash != HashPassword(request.Password))
            return null;

        // Obtener la primera empresa asignada (para el claim EmpresaId)
        var empresas = await _ueRepo.FindAsync(ue => ue.UsuarioId == user.Id, ct);
        var primeraEmpresa = empresas.FirstOrDefault();
        var empresaId = user.EmpresaActivaId ?? primeraEmpresa?.EmpresaId;
        var rol = primeraEmpresa?.Rol ?? "Viewer";

        // Fallback: si el usuario no tiene empresas asignadas, usar la primera empresa activa del sistema
        if (!empresaId.HasValue)
        {
            var todasEmpresas = await _empresaRepo.FindAsync(e => e.Activo, ct);
            empresaId = todasEmpresas.FirstOrDefault()?.Id;
        }

        // Generar JWT
        var token = GenerateJwtToken(user, empresaId, rol);
        var expiration = DateTime.UtcNow.AddHours(
            _config.GetValue<int>("Jwt:ExpirationHours", 8));

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = "", // No implementado en esta versión mínima
            Expiration = expiration,
            NombreUsuario = user.NombreUsuario,
            Email = user.Email
        };
    }

    public Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        // No implementado en versión mínima
        return Task.FromResult<AuthResponseDto?>(null);
    }

    private string GenerateJwtToken(Usuario user, int? empresaId, string rol)
    {
        var key = _config["Jwt:Key"] ?? "AgoraHub360-ERP-Dev-Secret-Key-2024-MinLength32!";
        var issuer = _config["Jwt:Issuer"] ?? "AgoraHub360.ERP";
        var audience = _config["Jwt:Audience"] ?? "AgoraHub360.ERP.Web";
        var hours = _config.GetValue<int>("Jwt:ExpirationHours", 8);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.NombreUsuario),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, rol),
        };

        if (empresaId.HasValue)
            claims.Add(new Claim("EmpresaId", empresaId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(hours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
