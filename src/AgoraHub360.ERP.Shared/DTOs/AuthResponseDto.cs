namespace AgoraHub360.ERP.Shared.DTOs;

/// <summary>
/// DTO para respuestas de autenticación JWT.
/// </summary>
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PlatformRole { get; set; } = "None";
    public int? TenantId { get; set; }
    public string TenantRole { get; set; } = "NoAccess";
    public string TenantStatus { get; set; } = "NotSelected";
    public IReadOnlyList<AgoraHub360.ERP.Shared.DTOs.Auth.EmpresaSesionDto> EmpresasDisponibles { get; set; }
        = Array.Empty<AgoraHub360.ERP.Shared.DTOs.Auth.EmpresaSesionDto>();
}
