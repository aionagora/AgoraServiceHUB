namespace AgoraHub360.ERP.Shared.DTOs.MDM;

using System.ComponentModel.DataAnnotations;

public class CreateProveedorDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razón social es obligatoria.")]
    [MaxLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    public string RazonSocial { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string? NIT { get; set; }

    [MaxLength(300, ErrorMessage = "Máximo 300 caracteres.")]
    public string? Direccion { get; set; }

    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string? Telefono { get; set; }

    [MaxLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    public string? Email { get; set; }

    [MaxLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    public string? NombreContacto { get; set; }

    [Required(ErrorMessage = "El tipo de proveedor es obligatorio.")]
    [MaxLength(50)]
    public string TipoProveedor { get; set; } = "Local";

    [MaxLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    public string? Pais { get; set; }

    [MaxLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    public string? CondicionPago { get; set; }
}
