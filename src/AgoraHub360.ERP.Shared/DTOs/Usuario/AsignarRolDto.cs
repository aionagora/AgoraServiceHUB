namespace AgoraHub360.ERP.Shared.DTOs.Usuario;

using System.ComponentModel.DataAnnotations;

public class AsignarRolDto
{
    [Required]
    public int EmpresaId { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public string Rol { get; set; } = string.Empty;
}
