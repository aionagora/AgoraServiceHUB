namespace AgoraHub360.ERP.Shared.DTOs.MDM;

using System.ComponentModel.DataAnnotations;

public class CreateAlmacenDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    public int? SucursalId { get; set; }

    [MaxLength(300, ErrorMessage = "Máximo 300 caracteres.")]
    public string? Direccion { get; set; }

    [MaxLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    public string? Responsable { get; set; }

    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string? Telefono { get; set; }
}
