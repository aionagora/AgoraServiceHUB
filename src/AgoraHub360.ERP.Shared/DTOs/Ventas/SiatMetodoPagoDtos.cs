namespace AgoraHub360.ERP.Shared.DTOs.Ventas;

using System.ComponentModel.DataAnnotations;

public class SiatMetodoPagoDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? ModoPago { get; set; }
    public bool EsPredeterminado { get; set; }
    public bool Activo { get; set; }
}

public class CrearSiatMetodoPagoRequestDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(150, ErrorMessage = "Máximo 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    public string? Descripcion { get; set; }

    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string? ModoPago { get; set; }

    public bool EsPredeterminado { get; set; }
    public bool Activo { get; set; } = true;
}

public class ActualizarSiatMetodoPagoRequestDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(150, ErrorMessage = "Máximo 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    public string? Descripcion { get; set; }

    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string? ModoPago { get; set; }

    public bool EsPredeterminado { get; set; }
    public bool Activo { get; set; } = true;
}
