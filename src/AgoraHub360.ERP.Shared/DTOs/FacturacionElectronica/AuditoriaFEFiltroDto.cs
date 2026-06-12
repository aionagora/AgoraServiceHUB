namespace AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

public class AuditoriaFEFiltroDto
{
    public DateTime? Desde { get; set; }

    public DateTime? Hasta { get; set; }

    public string? EstadoSiat { get; set; }

    public string? ProveedorCodigo { get; set; }

    public long? FacturaVentaId { get; set; }
}
