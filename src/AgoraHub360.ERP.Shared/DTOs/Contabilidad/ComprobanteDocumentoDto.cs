namespace AgoraHub360.ERP.Shared.DTOs.Contabilidad;

/// <summary>DTO de lectura para un documento adjunto a un comprobante.</summary>
public class ComprobanteDocumentoDto
{
    public int Id { get; set; }
    public long ComprobanteId { get; set; }
    public long DocumentId { get; set; }
    public string? Descripcion { get; set; }
    public int EmpresaId { get; set; }

    // Datos del documento adjunto (desnormalizados para la vista)
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string? Url { get; set; }
    public long SizeBytes { get; set; }
}

/// <summary>DTO para adjuntar un documento a un comprobante.</summary>
public class AdjuntarDocumentoDto
{
    public long DocumentId { get; set; }
    public string? Descripcion { get; set; }
}
