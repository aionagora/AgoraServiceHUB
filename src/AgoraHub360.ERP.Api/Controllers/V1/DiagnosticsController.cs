namespace AgoraHub360.ERP.Api.Controllers.V1;

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller de diagnóstico para verificar que la API funciona.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class DiagnosticsController : ControllerBase
{
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
}
