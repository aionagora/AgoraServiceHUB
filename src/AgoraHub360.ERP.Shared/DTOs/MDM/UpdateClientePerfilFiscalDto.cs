namespace AgoraHub360.ERP.Shared.DTOs.MDM;

using System.ComponentModel.DataAnnotations;

public class UpdateClientePerfilFiscalDto
{
    public int? ClienteSucursalId { get; set; }

    [Required(ErrorMessage = "El alias es obligatorio.")]
    [MaxLength(120, ErrorMessage = "El alias no puede exceder 120 caracteres.")]
    public string Alias { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de documento de identidad es obligatorio.")]
    [MaxLength(30, ErrorMessage = "El tipo de documento de identidad no puede exceder 30 caracteres.")]
    public string TipoDocumentoIdentidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de documento es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El número de documento no puede exceder 50 caracteres.")]
    public string NumeroDocumento { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "El complemento no puede exceder 20 caracteres.")]
    public string? Complemento { get; set; }

    [Required(ErrorMessage = "La razón social es obligatoria.")]
    [MaxLength(200, ErrorMessage = "La razón social no puede exceder 200 caracteres.")]
    public string RazonSocial { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "El tipo de persona no puede exceder 20 caracteres.")]
    public string? TipoPersona { get; set; }

    [MaxLength(50, ErrorMessage = "El tipo de perfil fiscal no puede exceder 50 caracteres.")]
    public string? TipoPerfilFiscal { get; set; }

    [MaxLength(200, ErrorMessage = "El email de facturación no puede exceder 200 caracteres.")]
    [EmailAddress(ErrorMessage = "El formato del email de facturación no es válido.")]
    public string? EmailFactura { get; set; }

    [MaxLength(50, ErrorMessage = "El teléfono de facturación no puede exceder 50 caracteres.")]
    public string? TelefonoFactura { get; set; }

    public bool RequiereEmail { get; set; }
    public bool EsPredeterminado { get; set; }

    [MaxLength(100, ErrorMessage = "El código de cliente API no puede exceder 100 caracteres.")]
    public string? CodigoClienteApi { get; set; }

    [MaxLength(100, ErrorMessage = "El código externo de facturación no puede exceder 100 caracteres.")]
    public string? CodigoExternoFacturacion { get; set; }

    [MaxLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres.")]
    public string? Observaciones { get; set; }

    public bool Activo { get; set; } = true;
}
