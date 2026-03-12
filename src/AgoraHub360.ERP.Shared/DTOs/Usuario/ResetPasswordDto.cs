namespace AgoraHub360.ERP.Shared.DTOs.Usuario;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Usado por el Admin para resetear la contraseña de cualquier usuario.
/// </summary>
public class ResetPasswordDto
{
    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string NuevaPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme la contraseña.")]
    [Compare(nameof(NuevaPassword), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarPassword { get; set; } = string.Empty;
}
