namespace AgoraHub360.ERP.Shared.DTOs.Seguridad;

using System.ComponentModel.DataAnnotations;

public class UpsertPerfilAccesoDto
{
    [MaxLength(50)]
    [Required]
    public string Codigo { get; set; } = string.Empty;

    [MaxLength(150)]
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(400)]
    public string? Descripcion { get; set; }

    [MaxLength(20)]
    public string TipoUsuario { get; set; } = "Ambos";

    public bool Activo { get; set; } = true;
}
