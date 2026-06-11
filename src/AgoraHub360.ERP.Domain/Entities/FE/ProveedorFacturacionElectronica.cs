using AgoraHub360.ERP.Domain.Common;

namespace AgoraHub360.ERP.Domain.Entities.FE;

/// <summary>
/// Catálogo global de proveedores de Facturación Electrónica.
/// No es tenant-aware — cada empresa selecciona su proveedor desde ConfiguracionFacturacionElectronica.
/// Ejemplos futuros en BD: CIRRUS, AGORAFC, SIAT_DIRECTO.
/// </summary>
public class ProveedorFacturacionElectronica : AuditableEntity
{
    public int Id { get; set; }

    /// <summary>Identificador funcional único del proveedor. Ej: CIRRUS, AGORAFC.</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Nombre comercial del proveedor. Ej: Cirrus, AgoraFC.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Descripción opcional del proveedor y sus capacidades.</summary>
    public string? Descripcion { get; set; }

    /// <summary>Indica si este proveedor requiere PosToken para operar.</summary>
    public bool RequierePosToken { get; set; }

    /// <summary>Indica si el proveedor soporta anulación de facturas.</summary>
    public bool SoportaAnulacion { get; set; }

    /// <summary>Indica si el proveedor permite consultar estado de una factura.</summary>
    public bool SoportaConsultaEstado { get; set; }

    /// <summary>Indica si el proveedor soporta emisión en modo offline.</summary>
    public bool SoportaModoOffline { get; set; }

    /// <summary>
    /// Nombre completo de la clase provider en Infrastructure para resolución por DI.
    /// Ej: AgoraHub360.ERP.Infrastructure.Services.FE.CirrusFacturacionProvider.
    /// Es opcional y solo sirve como referencia técnica.
    /// </summary>
    public string? ClaseProvider { get; set; }

    /// <summary>Configuraciones FE que usan este proveedor.</summary>
    public ICollection<ConfiguracionFacturacionElectronica> Configuraciones { get; set; }
        = new List<ConfiguracionFacturacionElectronica>();
}
