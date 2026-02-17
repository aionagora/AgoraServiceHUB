namespace AgoraHub360.ERP.Shared.DTOs;

/// <summary>
/// DTO para solicitudes de login.
/// </summary>
public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
