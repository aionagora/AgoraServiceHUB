namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Auth;

/// <summary>
/// Servicio de autenticación JWT.
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result<CambiarEmpresaResponseDto>> CambiarEmpresaActivaAsync(int usuarioId, int empresaId, CancellationToken cancellationToken = default);
}
