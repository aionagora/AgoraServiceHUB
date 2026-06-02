namespace AgoraHub360.ERP.Shared.DTOs.Empresa;

using System.ComponentModel.DataAnnotations;

public class CrearEmpresaDemoCompletaRequestDto
{
    [Required(ErrorMessage = "El código demo es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El código demo no puede exceder 20 caracteres.")]
    [RegularExpression("^DEMO\\d{2,3}$", ErrorMessage = "El código demo debe tener formato DEMO## o DEMO###, por ejemplo DEMO01 o DEMO101.")]
    public string CodigoDemo { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "El nombre de empresa no puede exceder 200 caracteres.")]
    public string? NombreEmpresa { get; set; }

    [MaxLength(50, ErrorMessage = "El NIT no puede exceder 50 caracteres.")]
    public string? Nit { get; set; }

    public bool ResetSiExiste { get; set; }
}
