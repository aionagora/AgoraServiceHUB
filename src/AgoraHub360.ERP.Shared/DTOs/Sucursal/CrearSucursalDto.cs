namespace AgoraHub360.ERP.Shared.DTOs.Sucursal;

using System.ComponentModel.DataAnnotations;

public class CrearSucursalDto
{
    [Required(ErrorMessage = "La empresa es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una empresa válida.")]
    public int EmpresaId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "El código no puede exceder 50 caracteres.")]
    public string? Codigo { get; set; }

    [MaxLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres.")]
    public string? Direccion { get; set; }

    [MaxLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres.")]
    public string? Ciudad { get; set; }

    [MaxLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres.")]
    public string? Telefono { get; set; }

    [MaxLength(200, ErrorMessage = "El email no puede exceder 200 caracteres.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    public string? Email { get; set; }

    public bool EsCentral { get; set; }

    public bool Activo { get; set; } = true;
}
