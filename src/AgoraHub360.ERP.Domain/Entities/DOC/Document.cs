namespace AgoraHub360.ERP.Domain.Entities.DOC;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Documento multimedia almacenado en distintos proveedores de storage.
/// StorageProvider: 1=DB, 2=FileSystem, 3=S3, 4=Azure
/// </summary>
public class Document : TenantEntity
{
    public long DocumentId { get; set; }

    /// <summary>1=DB, 2=FileSystem, 3=S3, 4=Azure</summary>
    public byte StorageProvider { get; set; } = 2;

    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Hash { get; set; }
    public long SizeBytes { get; set; }

    public ICollection<ProductDocument> ProductDocuments { get; set; } = new List<ProductDocument>();
}
