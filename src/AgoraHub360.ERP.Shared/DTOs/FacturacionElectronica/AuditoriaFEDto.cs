namespace AgoraHub360.ERP.Shared.DTOs.FacturacionElectronica;

public class AuditoriaFEDto
{
    public long Id { get; set; }

    public int EmpresaId { get; set; }

    public long FacturaVentaId { get; set; }

    public string ProveedorCodigo { get; set; } = string.Empty;

    public string ProveedorNombre { get; set; } = string.Empty;

    public string AmbienteCodigo { get; set; } = string.Empty;

    public string AmbienteNombre { get; set; } = string.Empty;

    public string BillUuid { get; set; } = string.Empty;

    public string? Cuf { get; set; }

    public string EstadoSiat { get; set; } = string.Empty;

    public string? MensajeError { get; set; }

    public string UsuarioId { get; set; } = string.Empty;

    public DateTime FechaHora { get; set; }

    public long TiempoRespuestaMs { get; set; }

    public string? CodigoRespuestaProveedor { get; set; }

    public string? DescripcionRespuestaProveedor { get; set; }

    public bool Exitoso { get; set; }
}
