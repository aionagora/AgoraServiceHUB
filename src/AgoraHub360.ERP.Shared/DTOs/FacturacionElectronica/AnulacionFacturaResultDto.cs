namespace AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

/// <summary>
/// Resultado de una anulación de factura contra el proveedor FE.
/// </summary>
public class AnulacionFacturaResultDto
{
    public bool Exitoso { get; set; }
    public string EstadoSiat { get; set; } = string.Empty;
    public string? Cuf { get; set; }
    public string? Mensaje { get; set; }
    public string? CodigoRespuestaProveedor { get; set; }
    public string? DescripcionRespuestaProveedor { get; set; }
}
