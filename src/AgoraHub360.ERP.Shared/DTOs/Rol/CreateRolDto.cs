namespace AgoraHub360.ERP.Shared.DTOs.Rol;

using System.ComponentModel.DataAnnotations;

public class CreateRolDto
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Descripcion { get; set; }
}
