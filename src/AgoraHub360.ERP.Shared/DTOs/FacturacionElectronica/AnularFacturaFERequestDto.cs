using System.ComponentModel.DataAnnotations;

namespace AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

/// <summary>
/// DTO de entrada para anular una factura electrónica.
/// </summary>
public class AnularFacturaFERequestDto
{
    [Required]
    [MinLength(5)]
    [MaxLength(500)]
    public string MotivoAnulacion { get; set; } = string.Empty;
}
