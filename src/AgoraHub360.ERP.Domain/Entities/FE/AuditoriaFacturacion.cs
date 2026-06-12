using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.VTA;
using AgoraHub360.ERP.Domain.Enums;

namespace AgoraHub360.ERP.Domain.Entities.FE;

/// <summary>
/// Auditoría de Facturación Electrónica (tenant-aware).
/// Registra cada intento de emisión, anulación o consulta contra el proveedor externo.
/// Guarda snapshot textual del proveedor y ambiente para conservar el histórico
/// aunque los catálogos cambien.
/// </summary>
public class AuditoriaFacturacion : TenantEntity
{
    public long Id { get; set; }

    // ── Factura asociada ──────────────────────────────────────────────────

    /// <summary>FK a la factura de venta emitida o intentada.</summary>
    public long FacturaVentaId { get; set; }

    /// <summary>Factura de venta asociada.</summary>
    public FacturaVenta? FacturaVenta { get; set; }

    // ── Snapshots textuales del proveedor y ambiente ───────────────────────
    // No son FK a los catálogos para preservar el histórico aunque cambien.

    /// <summary>Código del proveedor al momento de la operación. Ej: CIRRUS.</summary>
    public string ProveedorCodigo { get; set; } = string.Empty;

    /// <summary>Nombre del proveedor al momento de la operación. Ej: Cirrus.</summary>
    public string ProveedorNombre { get; set; } = string.Empty;

    /// <summary>Código del ambiente al momento de la operación. Ej: TEST, PRODUCCION.</summary>
    public string AmbienteCodigo { get; set; } = string.Empty;

    /// <summary>Nombre del ambiente al momento de la operación. Ej: Pruebas, Producción.</summary>
    public string AmbienteNombre { get; set; } = string.Empty;

    // ── Datos de la operación ─────────────────────────────────────────────

    /// <summary>BillUuid devuelto por el proveedor.</summary>
    public string BillUuid { get; set; } = string.Empty;

    /// <summary>CUF generado (puede ser null si la operación falló antes de generarlo).</summary>
    public string? Cuf { get; set; }

    /// <summary>Estado SIAT al momento del registro.</summary>
    public EstadoSiatFactura EstadoSiat { get; set; }

    /// <summary>Mensaje de error si la operación falló (sin exponer secretos ni response body completo).</summary>
    public string? MensajeError { get; set; }

    /// <summary>ID del usuario que ejecutó la operación.</summary>
    public string UsuarioId { get; set; } = string.Empty;

    /// <summary>Fecha y hora UTC de la operación.</summary>
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;

    /// <summary>Tiempo de respuesta del proveedor en milisegundos.</summary>
    public long TiempoRespuestaMs { get; set; }

    /// <summary>Código de respuesta devuelto por el proveedor (sin exponer el body completo).</summary>
    public string? CodigoRespuestaProveedor { get; set; }

    /// <summary>Descripción textual de la respuesta del proveedor.</summary>
    public string? DescripcionRespuestaProveedor { get; set; }

    /// <summary>Indica si la operación fue exitosa.</summary>
    public bool Exitoso { get; set; }
}
