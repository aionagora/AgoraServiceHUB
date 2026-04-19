namespace AgoraHub360.ERP.Shared.DTOs.Compras;

/// <summary>DTO de lectura para Orden de Pedido.</summary>
public record OrdenPedidoDto(
    long OrdenPedidoId,
    int EmpresaId,
    string EmpresaNombre,
    int? SucursalId,
    string? SucursalNombre,
    int? ClienteId,
    string? ClienteNombre,
    int? ClienteSucursalId,
    string? ClienteSucursalNombre,
    string Numero,
    DateTime FechaEmision,
    DateTime? FechaRequerida,
    int SolicitanteId,
    string SolicitanteNombre,
    string? CentroCosto,
    string Urgencia,
    int AlmacenDestinoId,
    string AlmacenDestinoNombre,
    string Estado,
    string? DireccionEntrega,
    string? ReferenciaEntrega,
    string? ContactoRelacionado,
    string? Observaciones,
    string? MotivoRechazo,
    bool? StockCubre,
    string? ObservacionesRevisionStock,
    bool Activo,
    List<OrdenPedidoLineaDto> Lineas);

/// <summary>DTO de lectura para línea de Orden de Pedido.</summary>
public record OrdenPedidoLineaDto(
    long OrdenPedidoLineaId,
    int NumeroLinea,
    long CompanyProductId,
    string ProductoSku,
    string Descripcion,
    string UnidadMedida,
    decimal CantidadSolicitada,
    decimal? CantidadStockDisponible,
    decimal? CantidadEnTransito,
    decimal? CantidadAComprar,
    string? Notas);

/// <summary>DTO para crear una Orden de Pedido.</summary>
public record CreateOrdenPedidoDto(
    DateTime FechaEmision,
    DateTime? FechaRequerida,
    string? CentroCosto,
    /// <summary>Normal | Urgente | Critico</summary>
    string Urgencia,
    int AlmacenDestinoId,
    int? SucursalId = null,
    int? ClienteId = null,
    int? ClienteSucursalId = null,
    string? Observaciones = null,
    List<CreateOrdenPedidoLineaDto>? Lineas = null);

/// <summary>DTO para crear una línea de Orden de Pedido.</summary>
public record CreateOrdenPedidoLineaDto(
    long CompanyProductId,
    string Descripcion,
    string UnidadMedida,
    decimal CantidadSolicitada,
    string? Notas = null);

/// <summary>DTO para actualizar una Orden de Pedido en Borrador.</summary>
public record UpdateOrdenPedidoDto(
    DateTime FechaEmision,
    DateTime? FechaRequerida,
    string? CentroCosto,
    /// <summary>Normal | Urgente | Critico</summary>
    string Urgencia,
    int AlmacenDestinoId,
    string? Observaciones,
    int? SucursalId = null,
    int? ClienteId = null,
    int? ClienteSucursalId = null);

/// <summary>
/// DTO para registrar la revisión de stock por Compras Central.
/// </summary>
public record RevisarStockOrdenPedidoDto(
    bool StockCubre,
    string? ObservacionesRevisionStock,
    List<RevisionStockLineaDto>? Lineas = null);

/// <summary>DTO para registrar disponibilidad de stock en una línea de OP.</summary>
public record RevisionStockLineaDto(
    long OrdenPedidoLineaId,
    decimal CantidadStockDisponible,
    decimal CantidadEnTransito);

/// <summary>DTO para aprobar o rechazar la Orden de Pedido.</summary>
public record AprobarRechazarOrdenPedidoDto(
    bool Aprobado,
    string? MotivoRechazo = null);
