namespace AgoraHub360.ERP.Domain.Entities.CMP;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Enums;

/// <summary>
/// Orden de Compra (Purchase Order).
/// Documento maestro con cabecera + líneas de detalle.
/// Flujo: Borrador ? Confirmado ? Aprobado ? (Recepcionado parcial/total) ? Cerrado.
/// </summary>
public class OrdenCompra : TenantEntity
{
    public long OrdenCompraId { get; set; }

    /// <summary>Número único generado desde NumeracionDocumento (ej: OC-000001).</summary>
    public string Numero { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; }

    /// <summary>Fecha estimada de entrega del proveedor.</summary>
    public DateTime? FechaEntregaEstimada { get; set; }

    public int ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }

    /// <summary>Almacén destino donde se recibirá la mercadería.</summary>
    public int AlmacenDestinoId { get; set; }
    public Almacen? AlmacenDestino { get; set; }

    /// <summary>Moneda de la OC (ISO 4217).</summary>
    public string MonedaId { get; set; } = "BOB";

    /// <summary>Tasa de cambio respecto a la moneda base de la empresa.</summary>
    public decimal TasaCambio { get; set; } = 1;

    /// <summary>Estado del documento.</summary>
    public EstadoDocumento Estado { get; set; } = EstadoDocumento.Borrador;

    /// <summary>Condición de pago (ej: Contado, 30 días, etc.).</summary>
    public string? CondicionPago { get; set; }

    /// <summary>Observaciones generales de la OC.</summary>
    public string? Observaciones { get; set; }

    /// <summary>Referencia externa (ej: Nro. cotización del proveedor).</summary>
    public string? ReferenciaExterna { get; set; }

    // ?? Totales (calculados desde las líneas) ??
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }

    // Navegación
    public ICollection<OrdenCompraLinea> Lineas { get; set; } = new List<OrdenCompraLinea>();

    /// <summary>Recalcula los totales a partir de las líneas.</summary>
    public void RecalcularTotales()
    {
        Subtotal = Lineas.Sum(l => l.Subtotal);
        Descuento = Lineas.Sum(l => l.MontoDescuento);
        Impuesto = Lineas.Sum(l => l.MontoImpuesto);
        Total = Subtotal - Descuento + Impuesto;
    }
}
