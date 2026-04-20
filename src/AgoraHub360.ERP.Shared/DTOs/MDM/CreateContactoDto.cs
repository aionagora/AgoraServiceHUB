namespace AgoraHub360.ERP.Shared.DTOs.MDM;

using System.ComponentModel.DataAnnotations;

public class CreateContactoDto
{
    public int? ClienteSucursalId { get; set; }

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [MaxLength(150, ErrorMessage = "Máximo 150 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [MaxLength(150, ErrorMessage = "Máximo 150 caracteres.")]
    public string? Apellidos { get; set; }

    [MaxLength(120, ErrorMessage = "Máximo 120 caracteres.")]
    public string? Cargo { get; set; }

    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string? Telefono { get; set; }

    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string? Celular { get; set; }

    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string? WhatsApp { get; set; }

    [MaxLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    [EmailAddress(ErrorMessage = "Formato de email inválido.")]
    public string? Email { get; set; }

    public bool EsPrincipal { get; set; }
    public bool Activo { get; set; } = true;
}
