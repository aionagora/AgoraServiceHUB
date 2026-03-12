namespace AgoraHub360.ERP.Domain.Entities.CMP;

using AgoraHub360.ERP.Domain.Common;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Enums;

/// <summary>
/// Orden de Pedido (OP) — Demanda interna de una sucursal o área.
/// Flujo: Borrador ? EnRevision ? (AbastecidoConStock | PendienteAprobacion ? Aprobado ? [OC])
///        | Anulado | Rechazado.
/// </summary>
public class OrdenPedido : TenantEntity
{
    public long OrdenPedidoId { get; set; }

    /// <summary>Número único generado desde NumeracionDocumento (ej: OP-000001).</summary>
    public string Numero { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; }

    /// <summary>Fecha en la que el solicitante necesita los ítems.</summary>
    public DateTime? FechaRequerida { get; set; }

    /// <summary>Usuario que origina la solicitud (FK a Usuarios).</summary>
    public int SolicitanteId { get; set; }
    public Usuario? Solicitante { get; set; }

    /// <summary>Centro de costo al que se imputa la solicitud.</summary>
    public string? CentroCosto { get; set; }

    /// <summary>Nivel de urgencia: Normal, Urgente, Crítico.</summary>
    public NivelUrgencia Urgencia { get; set; } = NivelUrgencia.Normal;

    /// <summary>Almacén destino donde se requiere el stock.</summary>
    public int AlmacenDestinoId { get; set; }
    public Almacen? AlmacenDestino { get; set; }

    public EstadoDocumento Estado { get; set; } = EstadoDocumento.Borrador;

    public string? Observaciones { get; set; }

    /// <summary>Motivo cuando se rechaza o anula la OP.</summary>
    public string? MotivoRechazo { get; set; }

    /// <summary>
    /// Resultado de la validación de stock.
    /// True = stock/tránsito cubre la demanda ? se cierra con AbastecidoConStock.
    /// False = requiere compra ? pasa a Compras Central.
    /// </summary>
    public bool? StockCubre { get; set; }

    /// <summary>Observaciones de la revisión de stock realizada por Compras.</summary>
    public string? ObservacionesRevisionStock { get; set; }

    // Navegación
    public ICollection<OrdenPedidoLinea> Lineas { get; set; } = new List<OrdenPedidoLinea>();

    /// <summary>OCs generadas a partir de esta OP.</summary>
    public ICollection<OrdenCompra> OrdenesCompra { get; set; } = new List<OrdenCompra>();
}
