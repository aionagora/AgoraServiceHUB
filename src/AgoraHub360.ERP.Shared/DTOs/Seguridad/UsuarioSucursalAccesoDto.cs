namespace AgoraHub360.ERP.Shared.DTOs.Seguridad;

public class UsuarioSucursalAccesoDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int EmpresaId { get; set; }
    public int SucursalId { get; set; }
    public bool EsPredeterminada { get; set; }
    public bool PuedeConsultar { get; set; }
    public bool PuedeOperar { get; set; }
    public bool Activo { get; set; }
}
