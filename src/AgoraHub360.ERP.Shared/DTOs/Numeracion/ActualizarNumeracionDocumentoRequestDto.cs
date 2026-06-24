namespace AgoraHub360.ERP.Shared.DTOs.Numeracion;

using System.ComponentModel.DataAnnotations;

public class ActualizarNumeracionDocumentoRequestDto
{
    [Required(ErrorMessage = "El tipo de documento es requerido.")]
    [MaxLength(20, ErrorMessage = "El tipo de documento no debe exceder 20 caracteres.")]
    public string TipoDocumento { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es requerida.")]
    [MaxLength(100, ErrorMessage = "La descripción no debe exceder 100 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    public int? SucursalId { get; set; }

    [Required(ErrorMessage = "El prefijo es requerido.")]
    [MaxLength(20, ErrorMessage = "El prefijo no debe exceder 20 caracteres.")]
    public string Prefijo { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "El siguiente número debe ser mayor o igual a 1.")]
    public int SiguienteNumero { get; set; } = 1;

    [Range(1, 10, ErrorMessage = "La longitud del número debe estar entre 1 y 10.")]
    public int LongitudNumero { get; set; } = 6;

    public bool Activo { get; set; } = true;
}
