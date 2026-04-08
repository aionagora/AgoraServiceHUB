namespace AgoraHub360.ERP.Shared.DTOs.Cliente;

using System.ComponentModel.DataAnnotations;

public class UpdateClienteDto
{
    [Required(ErrorMessage = "La razón social es requerida.")]
    [MaxLength(200, ErrorMessage = "La razón social no puede exceder 200 caracteres.")]
    public string RazonSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "El NIT es requerido.")]
    [MaxLength(20, ErrorMessage = "El NIT no puede exceder 20 caracteres.")]
    public string NIT { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres.")]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    [MaxLength(100, ErrorMessage = "El email no puede exceder 100 caracteres.")]
    public string? Email { get; set; }

    [MaxLength(300, ErrorMessage = "La dirección no puede exceder 300 caracteres.")]
    public string? Direccion { get; set; }

    public bool Activo { get; set; } = true;
}
