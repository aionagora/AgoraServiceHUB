namespace AgoraHub360.ERP.Domain.Entities.LOG;

using AgoraHub360.ERP.Domain.Common;

/// <summary>
/// Hoja de Ruta para control de procesos logísticos.
/// Dos grandes categorías:
///   INTERNA  ? operaciones entre almacenes/importación (flujo aduanero).
///   CLIENTE  ? entregas, recojos, devoluciones al cliente.
/// </summary>
public class HojaRuta : TenantEntity
{
    public long HojaRutaId { get; set; }

    /// <summary>Número autogenerado desde NumeracionDocumento (ej: HR-000001).</summary>
    public string NumeroHojaRuta { get; set; } = string.Empty;

    /// <summary>Categoría principal: INTERNA | CLIENTE.</summary>
    public string TipoOP { get; set; } = "INTERNA";

    /// <summary>
    /// Sub-tipo según categoría.
    /// INTERNA  ? IMPORTACION | TRASPASO_INTERNO | DEVOLUCION | AJUSTE
    /// CLIENTE  ? ENTREGA | RECOJO | DEVOLUCION_CLI
    /// </summary>
    public string SubTipo { get; set; } = string.Empty;

    /// <summary>Almacén de origen (obligatorio para INTERNA).</summary>
    public int AlmacenOrigenId { get; set; }

    /// <summary>Almacén destino (opcional, TRASPASO_INTERNO).</summary>
    public int? AlmacenDestinoId { get; set; }

    /// <summary>Nombre del proveedor (INTERNA) o cliente (CLIENTE).</summary>
    public string? ProveedorCliente { get; set; }

    /// <summary>Dirección de entrega (CLIENTE - ENTREGA).</summary>
    public string? DireccionEntrega { get; set; }

    /// <summary>Contacto del cliente: teléfono o email (CLIENTE).</summary>
    public string? ContactoCliente { get; set; }

    /// <summary>Usuario responsable (readonly desde sesión).</summary>
    public string ResponsableUsuario { get; set; } = string.Empty;

    /// <summary>Fecha de registro automática.</summary>
    public DateTime FechaRegistro { get; set; }

    /// <summary>Fecha del documento (obligatoria).</summary>
    public DateTime FechaDocumento { get; set; }

    /// <summary>
    /// ETA (estimado arribo) para INTERNA,
    /// Fecha de entrega estimada para CLIENTE.
    /// </summary>
    public DateTime? ETA { get; set; }

    /// <summary>BORRADOR, EN_PROCESO, COMPLETADO, ANULADO.</summary>
    public string Estado { get; set; } = "BORRADOR";

    /// <summary>
    /// INTERNA : INICIADO ? EMBARCADO ? EN_ADUANA ? AFORO ? LEVANTE ? EN_RECEPCION ? CERRADO
    /// CLIENTE : INICIADO ? PREPARANDO ? EN_RUTA ? ENTREGADO | NO_ENTREGADO ? CERRADO
    /// </summary>
    public string SubEstado { get; set; } = "INICIADO";

    public string? Observaciones { get; set; }

    /// <summary>
    /// OP que originó esta Hoja de Ruta (opcional).
    /// Permite navegar desde una OrdenPedido a su HR de seguimiento.
    /// </summary>
    public long? OrdenPedidoId { get; set; }

    // Navegación
    public ICollection<HojaRutaHistorial> Historial { get; set; } = new List<HojaRutaHistorial>();
}
