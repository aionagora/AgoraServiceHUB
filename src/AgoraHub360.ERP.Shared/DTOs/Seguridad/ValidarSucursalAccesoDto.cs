namespace AgoraHub360.ERP.Shared.DTOs.Seguridad;

public class ValidarSucursalAccesoDto
{
    public int SucursalId { get; set; }
    public bool PuedeConsultar { get; set; }
    public bool PuedeOperar { get; set; }
}
