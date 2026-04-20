namespace AgoraHub360.ERP.Shared.DTOs.Seguridad;

public class PerfilAccesoDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string TipoUsuario { get; set; } = "Ambos";
    public bool Activo { get; set; }
}
