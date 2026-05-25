namespace AgoraHub360.ERP.Shared.DTOs.Seguridad;

public class PerfilPermisoDto
{
    public int Id { get; set; }
    public int PerfilAccesoId { get; set; }
    public int FormularioSistemaId { get; set; }
    public int? AccionSistemaId { get; set; }
    public bool Permitido { get; set; }
}
