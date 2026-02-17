namespace AgoraHub360.ERP.Shared.DTOs.Empresa;

using System.ComponentModel.DataAnnotations;

public class UpdateEmpresaDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? NIT { get; set; }

    [MaxLength(500)]
    public string? Direccion { get; set; }

    [MaxLength(50)]
    public string? Telefono { get; set; }

    [MaxLength(200)]
    [EmailAddress(ErrorMessage = "El email no tiene un formato valido.")]
    public string? Email { get; set; }

    [MaxLength(3)]
    public string? MonedaBaseId { get; set; }

    public bool Activo { get; set; } = true;
}
