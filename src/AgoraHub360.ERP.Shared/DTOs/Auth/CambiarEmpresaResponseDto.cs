namespace AgoraHub360.ERP.Shared.DTOs.Auth;

public sealed class CambiarEmpresaResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string PlatformRole { get; set; } = "None";
    public int? TenantId { get; set; }
    public string TenantRole { get; set; } = "NoAccess";
    public string TenantStatus { get; set; } = "NotSelected";
    public EmpresaSesionDto EmpresaActiva { get; set; } = default!;
    public IReadOnlyList<EmpresaSesionDto> EmpresasDisponibles { get; set; } = Array.Empty<EmpresaSesionDto>();
    public IReadOnlyList<SucursalSesionDto> SucursalesDisponibles { get; set; } = Array.Empty<SucursalSesionDto>();
}
