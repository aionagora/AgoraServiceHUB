namespace AgoraHub360.ERP.Application.Interfaces;

/// <summary>
/// Servicio para obtener información del usuario y tenant actual.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    int? UserIdInt { get; }
    string? UserName { get; }
    int? EmpresaId { get; }
    bool IsInRole(string role);
}
