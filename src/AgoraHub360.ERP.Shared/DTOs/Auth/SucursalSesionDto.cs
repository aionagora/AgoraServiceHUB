namespace AgoraHub360.ERP.Shared.DTOs.Auth;

public sealed class SucursalSesionDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsCentral { get; set; }
    public bool EsPredeterminada { get; set; }
}
