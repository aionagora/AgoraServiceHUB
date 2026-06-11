using AgoraHub360.ERP.Domain.Common;

namespace AgoraHub360.ERP.Domain.Entities.FE;

/// <summary>
/// Catálogo global de ambientes de Facturación Electrónica.
/// No es tenant-aware — cada configuración FE selecciona su ambiente.
/// Ejemplos futuros en BD: TEST, PRODUCCION.
/// </summary>
public class AmbienteFacturacionElectronica : AuditableEntity
{
    public int Id { get; set; }

    /// <summary>Identificador funcional único del ambiente. Ej: TEST, PRODUCCION.</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Nombre descriptivo del ambiente. Ej: Pruebas, Producción.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Descripción opcional del ambiente.</summary>
    public string? Descripcion { get; set; }

    /// <summary>Indica si este ambiente es de producción (true) o pruebas (false).</summary>
    public bool EsProduccion { get; set; }

    /// <summary>Configuraciones FE que usan este ambiente.</summary>
    public ICollection<ConfiguracionFacturacionElectronica> Configuraciones { get; set; }
        = new List<ConfiguracionFacturacionElectronica>();
}
