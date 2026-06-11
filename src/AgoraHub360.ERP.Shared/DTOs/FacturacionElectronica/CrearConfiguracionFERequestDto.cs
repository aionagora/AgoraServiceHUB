using System.ComponentModel.DataAnnotations;

namespace AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

/// <summary>
/// DTO de entrada para crear una nueva configuración FE.
/// ClientSecret y PosToken viajan en texto plano desde el frontend
/// y se cifran al almacenarse en Infrastructure.
/// </summary>
public class CrearConfiguracionFERequestDto
{
    [Required]
    [MaxLength(200)]
    public string NombreConfiguracion { get; set; } = string.Empty;

    [Required]
    public int ProveedorFacturacionElectronicaId { get; set; }

    [Required]
    public int AmbienteFacturacionElectronicaId { get; set; }

    [Required]
    [MaxLength(300)]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Se cifrará antes de almacenar. Nunca viaja en DTOs de salida.</summary>
    [Required]
    [MaxLength(1000)]
    public string ClientSecret { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string TokenUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string ApiManagementUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string ApiBillingUrl { get; set; } = string.Empty;

    /// <summary>Se cifrará antes de almacenar. Nunca viaja en DTOs de salida.</summary>
    [Required]
    [MaxLength(1000)]
    public string PosToken { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? SucursalFiscal { get; set; }

    [MaxLength(50)]
    public string? PuntoVentaFiscal { get; set; }

    [Required]
    [MaxLength(50)]
    public string ActivityCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string NitEmisor { get; set; } = string.Empty;

    [Range(5, 60)]
    public int TimeoutSegundos { get; set; } = 10;

    public bool EsConfiguracionActiva { get; set; } = true;

    [MaxLength(1000)]
    public string? Observaciones { get; set; }
}
