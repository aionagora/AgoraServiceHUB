namespace AgoraHub360.ERP.Domain.Entities.CMP;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;

/// <summary>
/// Recepción de mercadería vinculada a una Orden de Compra.
/// Permite recepciones parciales: cada recepción puede cubrir una fracción de las líneas.
/// Al confirmar, genera automáticamente movimientos de inventario tipo Receipt.
/// </summary>
public class RecepcionCompra : TenantEntity
{
    public long RecepcionCompraId { get; set; }

    /// <summary>Número único generado desde NumeracionDocumento (ej: REC-000001).</summary>
    public string Numero { get; set; } = string.Empty;

    public long OrdenCompraId { get; set; }
    public OrdenCompra? OrdenCompra { get; set; }

    public DateTime FechaRecepcion { get; set; }

    /// <summary>Almacén donde se recibe la mercadería (copiado de la OC, puede cambiarse).</summary>
    public int AlmacenId { get; set; }
    public Almacen? Almacen { get; set; }

    /// <summary>Documento de referencia del proveedor (guía, factura, remisión).</summary>
    public string? DocumentoProveedor { get; set; }

    public string? Observaciones { get; set; }

    /// <summary>True si ya se confirmó y generó movimientos de inventario.</summary>
    public bool Confirmada { get; set; }

    // Navegación
    public ICollection<RecepcionCompraLinea> Lineas { get; set; } = new List<RecepcionCompraLinea>();
}
