namespace AgoraHub360.ERP.Shared.DTOs.Auth;

public sealed class CambiarEmpresaResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public EmpresaSesionDto EmpresaActiva { get; set; } = default!;
    public IReadOnlyList<EmpresaSesionDto> EmpresasDisponibles { get; set; } = Array.Empty<EmpresaSesionDto>();
    public IReadOnlyList<SucursalSesionDto> SucursalesDisponibles { get; set; } = Array.Empty<SucursalSesionDto>();
}
