namespace AgoraHub360.ERP.Api.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
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

        var empresasAsignadas = await _ueRepo.FindAsync(ue => ue.UsuarioId == user.Id, ct);
        var empresaIds = empresasAsignadas.Select(e => e.EmpresaId).Distinct().ToList();
        var empresasActivas = await _empresaRepo.FindAsync(e => empresaIds.Contains(e.Id) && e.Activo, ct);
        var empresasActivasAsignadas = empresasAsignadas.Where(e => empresasActivas.Any(a => a.Id == e.EmpresaId)).ToList();
        if (empresasActivas.Count == 0)
            return null;

        UsuarioEmpresa? empresaSeleccionada = null;
        if (user.EmpresaActivaId.HasValue)
        {
            empresaSeleccionada = empresasActivasAsignadas.FirstOrDefault(e => e.EmpresaId == user.EmpresaActivaId.Value);
        }

        if (empresaSeleccionada is null)
        {
            empresaSeleccionada = empresasActivasAsignadas.FirstOrDefault();
            if (empresaSeleccionada is null)
                return null;
            user.EmpresaActivaId = empresaSeleccionada.EmpresaId;
        }

        var token = GenerateJwtToken(user, empresaSeleccionada.EmpresaId, empresaSeleccionada.Rol);
        var expiration = DateTime.UtcNow.AddHours(
            _config.GetValue<int>("Jwt:ExpirationHours", 8));

        await _userRepo.UpdateAsync(user, ct);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = "",
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

    public async Task<Result<CambiarEmpresaResponseDto>> CambiarEmpresaActivaAsync(int usuarioId, int empresaId, CancellationToken ct = default)
    {
        var usuario = await _userRepo.GetByIdAsync(usuarioId, ct);
        if (usuario is null || !usuario.Activo)
            return Result<CambiarEmpresaResponseDto>.Failure("Usuario no encontrado o inactivo.");

        var asignaciones = await _ueRepo.FindAsync(ue => ue.UsuarioId == usuarioId, ct);
        var asignacion = asignaciones.FirstOrDefault(a => a.EmpresaId == empresaId);
        if (asignacion is null)
            return Result<CambiarEmpresaResponseDto>.Failure("La empresa seleccionada no pertenece al usuario.");

        var empresa = await _empresaRepo.GetByIdAsync(empresaId, ct);
        if (empresa is null || !empresa.Activo)
            return Result<CambiarEmpresaResponseDto>.Failure("La empresa seleccionada no está activa.");

        usuario.EmpresaActivaId = empresaId;
        await _userRepo.UpdateAsync(usuario, ct);

        var token = GenerateJwtToken(usuario, empresaId, asignacion.Rol);
        var expiration = DateTime.UtcNow.AddHours(
            _config.GetValue<int>("Jwt:ExpirationHours", 8));

        var empresasDisponibles = asignaciones.Select(a => new EmpresaSesionDto
        {
            Id = a.EmpresaId,
            Nombre = a.Empresa?.Nombre ?? $"Empresa #{a.EmpresaId}",
            Nit = a.Empresa?.NIT,
            Rol = a.Rol,
            EsActiva = a.EmpresaId == empresaId
        }).ToList();

        var response = new CambiarEmpresaResponseDto
        {
            Token = token,
            Expiration = expiration,
            EmpresaActiva = new EmpresaSesionDto
            {
                Id = empresa.Id,
                Nombre = empresa.Nombre,
                Nit = empresa.NIT,
                Rol = asignacion.Rol,
                EsActiva = true
            },
            EmpresasDisponibles = empresasDisponibles.AsReadOnly(),
            SucursalesDisponibles = Array.Empty<SucursalSesionDto>()
        };

        return Result<CambiarEmpresaResponseDto>.Success(response);
    }

    private string GenerateJwtToken(Usuario user, int empresaId, string rol)
    {
        var key = _config["Jwt:Key"] ?? "AgoraHub360-ERP-Dev-Secret-Key-2026-MinLength32!";
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

        claims.Add(new Claim("EmpresaId", empresaId.ToString()));

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
