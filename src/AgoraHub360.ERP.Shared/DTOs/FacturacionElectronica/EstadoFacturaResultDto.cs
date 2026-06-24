namespace AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

/// <summary>
/// Resultado de una consulta de estado de factura contra el proveedor FE.
/// </summary>
public class EstadoFacturaResultDto
{
    public bool Exitoso { get; set; }
    public string EstadoSiat { get; set; } = string.Empty;
    public string? Cuf { get; set; }
    public string? Mensaje { get; set; }
    public string? CodigoRespuestaProveedor { get; set; }
    public string? DescripcionRespuestaProveedor { get; set; }
    public DateTime FechaConsultaUtc { get; set; } = DateTime.UtcNow;
}
