namespace AgoraHub360.ERP.Domain.Entities.CMP;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Enums;

/// <summary>
/// Confirmación del Proveedor – Proforma Invoice (PI) o aceptación de condiciones.
/// Representa el paso [4] del flujo: proveedor acepta o negocia la OC.
/// Flujo: Borrador ? CondicionesOK (? OC ConfirmadaProveedor)
///                 | EnNegociacion (? ajuste OC y reenvío).
/// </summary>
public class ConfirmacionProveedor : TenantEntity
{
    public long ConfirmacionProveedorId { get; set; }

    public long OrdenCompraId { get; set; }
    public OrdenCompra? OrdenCompra { get; set; }

    /// <summary>Número de Proforma Invoice del proveedor.</summary>
    public string? NumeroProforma { get; set; }

    public DateTime FechaConfirmacion { get; set; }

    /// <summary>¿El proveedor confirmó las condiciones tal como estaban en la OC?</summary>
    public bool CondicionesOK { get; set; }

    /// <summary>Observaciones del proveedor (motivos de negociación, ajustes propuestos).</summary>
    public string? ObservacionesProveedor { get; set; }

    /// <summary>Fecha de entrega comprometida por el proveedor en la confirmación.</summary>
    public DateTime? FechaEntregaComprometida { get; set; }

    /// <summary>Estado resultante: ConfirmadaProveedor o EnNegociacion.</summary>
    public EstadoDocumento Estado { get; set; } = EstadoDocumento.EnviadaProveedor;

    public string? Observaciones { get; set; }

    /// <summary>Número de iteración de negociación (1 = primera respuesta, 2 = tras primera renegociación, etc.).</summary>
    public int IteracionNegociacion { get; set; } = 1;
}
