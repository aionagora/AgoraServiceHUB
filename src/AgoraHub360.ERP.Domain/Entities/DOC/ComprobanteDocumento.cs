namespace AgoraHub360.ERP.Domain.Entities.DOC;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.ACC;

/// <summary>
/// Documento adjunto a un comprobante contable (asiento).
/// Permite asociar facturas, contratos y otros archivos a un asiento.
/// </summary>
public class ComprobanteDocumento : TenantEntity
{
    public int Id { get; set; }

    /// <summary>Asiento contable al que pertenece el documento.</summary>
    public long ComprobanteId { get; set; }
    public virtual AsientoContable? Comprobante { get; set; }

    /// <summary>Documento multimedia en la tabla doc.Documents.</summary>
    public long DocumentId { get; set; }
    public virtual Document? Document { get; set; }

    /// <summary>Descripción opcional del adjunto (ej: "Factura proveedor China").</summary>
    public string? Descripcion { get; set; }
}
