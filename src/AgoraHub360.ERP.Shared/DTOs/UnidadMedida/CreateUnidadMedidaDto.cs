namespace AgoraHub360.ERP.Shared.DTOs.UnidadMedida;

using System.ComponentModel.DataAnnotations;

public class CreateUnidadMedidaDto
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La abreviatura es requerida.")]
    [MaxLength(20, ErrorMessage = "La abreviatura no puede exceder 20 caracteres.")]
    public string Abreviatura { get; set; } = string.Empty;
}
