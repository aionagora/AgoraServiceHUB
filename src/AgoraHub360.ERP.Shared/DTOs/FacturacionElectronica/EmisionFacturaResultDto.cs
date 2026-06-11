namespace AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

/// <summary>
/// Resultado de una emisión de factura contra el proveedor FE.
/// No incluye response body completo ni secretos.
/// </summary>
public class EmisionFacturaResultDto
{
    public bool Exitoso { get; set; }
    public string EstadoSiat { get; set; } = string.Empty;
    public string BillUuid { get; set; } = string.Empty;
    public string? Cuf { get; set; }
    public string? Cufd { get; set; }
    public string? SiatQr { get; set; }
    public string? EnlacePdf { get; set; }
    public string? EnlaceXml { get; set; }
    public string? Mensaje { get; set; }
    public string? CodigoRespuestaProveedor { get; set; }
    public string? DescripcionRespuestaProveedor { get; set; }
    public bool EsOffline { get; set; }
}
