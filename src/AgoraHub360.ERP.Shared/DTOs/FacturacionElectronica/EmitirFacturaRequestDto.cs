namespace AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

/// <summary>
/// DTO interno de emisión hacia el proveedor FE.
/// No contiene secretos — las credenciales se obtienen desde ConfiguracionFEDto.
/// </summary>
public class EmitirFacturaRequestDto
{
    public long FacturaVentaId { get; set; }
    public long VentaId { get; set; }
    public int EmpresaId { get; set; }
    public string BillUuid { get; set; } = string.Empty;
    public string ActivityCode { get; set; } = string.Empty;
    public string NitEmisor { get; set; } = string.Empty;
    public string BeneficiaryDocNumber { get; set; } = string.Empty;
    public string? DocNumberComplement { get; set; }
    public string IdentityDocTypeCode { get; set; } = string.Empty;
    public string BillingName { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public string? BeneficiaryEmail { get; set; }
    public string PaymentMethodCode { get; set; } = string.Empty;
    public string? CardNumber { get; set; }
    public decimal AdditionalDiscount { get; set; }
    public decimal GiftCardAmount { get; set; }
    public List<FacturacionFEItemDto> Items { get; set; } = new();
}
