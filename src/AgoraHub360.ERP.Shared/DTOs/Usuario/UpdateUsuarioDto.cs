namespace AgoraHub360.ERP.Shared.DTOs.Usuario;

using System.ComponentModel.DataAnnotations;

public class UpdateUsuarioDto
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [MaxLength(100)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [MaxLength(200)]
    [EmailAddress(ErrorMessage = "El email no tiene un formato valido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [MaxLength(300)]
    public string NombreCompleto { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
