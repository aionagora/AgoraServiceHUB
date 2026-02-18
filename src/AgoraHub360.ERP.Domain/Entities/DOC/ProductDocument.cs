namespace AgoraHub360.ERP.Domain.Entities.DOC;

/// <summary>
/// Relación entre producto (o CompanyProduct) y un documento multimedia.
/// DocType: 1=FotoPrincipal, 2=Galeria, 3=FichaTecnica, 4=MSDS, 5=Certificado
/// </summary>
public class ProductDocument
{
    public long ProductId { get; set; }
    public long DocumentId { get; set; }

    /// <summary>1=FotoPrincipal, 2=Galeria, 3=FichaTecnica, 4=MSDS, 5=Certificado</summary>
    public byte DocType { get; set; } = 1;

    public int SortOrder { get; set; }

    // Navegación
    public Document Document { get; set; } = null!;
}
