namespace AgoraHub360.ERP.Shared.DTOs.Auth;

public sealed class EmpresaSesionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Nit { get; set; }
    public string Rol { get; set; } = string.Empty;
    public bool EsActiva { get; set; }
}
