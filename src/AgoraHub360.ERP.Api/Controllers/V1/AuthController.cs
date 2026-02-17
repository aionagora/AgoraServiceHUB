namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Autentica un usuario y devuelve un token JWT.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);

        if (result is null)
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Credenciales inválidas."));

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login exitoso."));
    }

    /// <summary>
    /// Devuelve la información del usuario autenticado.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var rol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var empresaId = User.FindFirst("EmpresaId")?.Value;

        var info = new
        {
            UserId = userId,
            UserName = userName,
            Email = email,
            Rol = rol,
            EmpresaId = empresaId
        };

        return Ok(ApiResponse<object>.Ok(info));
    }
}
