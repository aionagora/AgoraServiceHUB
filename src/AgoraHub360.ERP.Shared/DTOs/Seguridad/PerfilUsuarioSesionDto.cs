namespace AgoraHub360.ERP.Shared.DTOs.Seguridad;

public class PerfilUsuarioSesionDto
{
    public int PerfilAccesoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string TipoUsuario { get; set; } = "Ambos";
}
