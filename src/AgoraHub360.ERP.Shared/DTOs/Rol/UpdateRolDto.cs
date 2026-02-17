namespace AgoraHub360.ERP.Shared.DTOs.Rol;

using System.ComponentModel.DataAnnotations;

public class UpdateRolDto
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
}
