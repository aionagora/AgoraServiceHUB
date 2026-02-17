namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Shared.DTOs;

/// <summary>
/// Servicio de autenticación JWT.
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
