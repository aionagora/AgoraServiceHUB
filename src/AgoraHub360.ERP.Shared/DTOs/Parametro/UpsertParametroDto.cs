namespace AgoraHub360.ERP.Shared.DTOs.Parametro;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// DTO para crear o actualizar un parámetro del sistema.
/// Si la clave ya existe para la empresa, se actualiza; si no, se crea.
/// </summary>
public class UpsertParametroDto
{
    [Required(ErrorMessage = "La clave es requerida.")]
    [MaxLength(100)]
    public string Clave { get; set; } = string.Empty;

    [Required(ErrorMessage = "El valor es requerido.")]
    [MaxLength(500)]
    public string Valor { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [MaxLength(50)]
    public string Categoria { get; set; } = "General";

    [MaxLength(20)]
    public string TipoDato { get; set; } = "String";
}
