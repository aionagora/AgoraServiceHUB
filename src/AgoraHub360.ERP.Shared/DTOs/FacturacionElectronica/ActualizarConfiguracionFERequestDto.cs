using System.ComponentModel.DataAnnotations;

namespace AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

/// <summary>
/// DTO de entrada para actualizar una configuración FE existente.
/// ClientSecret y PosToken son opcionales: si se envían null/vacío,
/// se conservan los valores cifrados existentes.
/// </summary>
public class ActualizarConfiguracionFERequestDto
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

    /// <summary>Opcional. Si es null/vacío no se reemplaza el valor cifrado existente.</summary>
    [MaxLength(1000)]
    public string? ClientSecret { get; set; }

    [Required]
    [MaxLength(500)]
    public string TokenUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string ApiManagementUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string ApiBillingUrl { get; set; } = string.Empty;

    /// <summary>Opcional. Si es null/vacío no se reemplaza el valor cifrado existente.</summary>
    [MaxLength(1000)]
    public string? PosToken { get; set; }

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
