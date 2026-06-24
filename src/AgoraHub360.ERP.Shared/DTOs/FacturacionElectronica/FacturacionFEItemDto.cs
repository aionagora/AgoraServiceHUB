namespace AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

/// <summary>
/// Item individual de una factura para emisión FE.
/// </summary>
public class FacturacionFEItemDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoMonto { get; set; }
    public string? DetalleAdicional { get; set; }
}
