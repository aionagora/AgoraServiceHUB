namespace AgoraHub360.ERP.Shared.DTOs.DOC;

/// <summary>DTO de respuesta al subir un archivo.</summary>
public class DocumentUploadResultDto
{
    public long DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string? Url { get; set; }
}
