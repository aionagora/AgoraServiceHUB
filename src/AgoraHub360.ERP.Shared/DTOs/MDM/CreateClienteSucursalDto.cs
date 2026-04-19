namespace AgoraHub360.ERP.Shared.DTOs.MDM;

using System.ComponentModel.DataAnnotations;

public class CreateClienteSucursalDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    public int? PaisId { get; set; }
    public int? DepartamentoId { get; set; }
    public int? ProvinciaId { get; set; }
    public int? CiudadId { get; set; }
    public int? ZonaId { get; set; }

    [MaxLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    public string? Direccion { get; set; }

    [MaxLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    public string? Referencia { get; set; }

    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }

    public int? ContactoPrincipalId { get; set; }

    public bool EsPrincipal { get; set; }
    public bool Activo { get; set; } = true;

    [MaxLength(1000, ErrorMessage = "Máximo 1000 caracteres.")]
    public string? Observaciones { get; set; }
}
