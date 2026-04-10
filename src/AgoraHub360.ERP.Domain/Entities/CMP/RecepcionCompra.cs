namespace AgoraHub360.ERP.Domain.Entities.CMP;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Enums;

/// <summary>
/// Recepción de mercadería vinculada a una Orden de Compra.
/// Permite recepciones parciales: cada recepción puede cubrir una fracción de las líneas.
/// Paso [9] del flujo: descarga + conteo vs Packing/OC, control de calidad, diferencias.
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

    // ?? Control de diferencias [9] ??????????????????????????????????????????

    /// <summary>True si se detectaron diferencias (faltantes, daños o sobrantes) durante la recepción.</summary>
    public bool TieneDiferencias { get; set; }

    /// <summary>Tipo de diferencia principal detectada: Faltante, Daño, Sobrante, Mixto.</summary>
    public string? TipoDiferencia { get; set; }

    /// <summary>Descripción del acta de diferencias levantada.</summary>
    public string? ActaDiferencias { get; set; }

    /// <summary>Número de acta/reclamo generado al proveedor o forwarder.</summary>
    public string? NumeroReclamo { get; set; }

    /// <summary>True si las unidades con diferencias se enviaron a cuarentena.</summary>
    public bool EnCuarentena { get; set; }

    /// <summary>Ubicación de cuarentena donde están depositadas las unidades en observación.</summary>
    public string? UbicacionCuarentena { get; set; }

    /// <summary>Resultado del control de calidad (Aprobado, Rechazado, Observado).</summary>
    public string? ResultadoControlCalidad { get; set; }

    // Navegación
    public ICollection<RecepcionCompraLinea> Lineas { get; set; } = new List<RecepcionCompraLinea>();
}
