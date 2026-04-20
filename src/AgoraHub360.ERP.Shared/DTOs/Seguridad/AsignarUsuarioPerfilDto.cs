namespace AgoraHub360.ERP.Shared.DTOs.Seguridad;

using System.ComponentModel.DataAnnotations;

public class AsignarUsuarioPerfilDto
{
    [Range(1, int.MaxValue)]
    public int UsuarioId { get; set; }

    [Range(1, int.MaxValue)]
    public int PerfilAccesoId { get; set; }

    public DateTime? VigenteDesde { get; set; }
    public DateTime? VigenteHasta { get; set; }
    public bool Activo { get; set; } = true;
}
