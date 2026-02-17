namespace AgoraHub360.ERP.Shared.DTOs.Numeracion;

using System.ComponentModel.DataAnnotations;

public class CreateNumeracionDto
{
    [Required(ErrorMessage = "El tipo de documento es requerido.")]
    [MaxLength(20)]
    public string TipoDocumento { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es requerida.")]
    [MaxLength(100)]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El prefijo es requerido.")]
    [MaxLength(20)]
    public string Prefijo { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "El número inicial debe ser al menos 1.")]
    public int SiguienteNumero { get; set; } = 1;

    [Range(1, 10, ErrorMessage = "Los dígitos deben estar entre 1 y 10.")]
    public int Digitos { get; set; } = 6;
}
