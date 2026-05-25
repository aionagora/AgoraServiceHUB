namespace AgoraHub360.ERP.Shared.DTOs.MDM;

public class ContactoUsuarioAccesoDto
{
    public int Id { get; set; }
    public int ContactoId { get; set; }
    public int EmpresaId { get; set; }
    public int UsuarioId { get; set; }
    public int? PerfilAccesoId { get; set; }
    public bool AccesoWeb { get; set; }
    public bool AccesoMovil { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
