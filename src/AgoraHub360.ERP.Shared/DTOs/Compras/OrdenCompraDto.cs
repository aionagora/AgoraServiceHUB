namespace AgoraHub360.ERP.Shared.DTOs.Compras;

/// <summary>DTO de lectura para Orden de Compra.</summary>
public record OrdenCompraDto(
    long OrdenCompraId,
    int EmpresaId,
    string Numero,
    DateTime FechaEmision,
    DateTime? FechaEntregaEstimada,
    int ProveedorId,
    string ProveedorNombre,
    int AlmacenDestinoId,
    string AlmacenDestinoNombre,
    string MonedaId,
    decimal TasaCambio,
    string Estado,
    string? CondicionPago,
    string? Observaciones,
    string? ReferenciaExterna,
    decimal Subtotal,
    decimal Descuento,
    decimal Impuesto,
    decimal Total,
    bool Activo,
    List<OrdenCompraLineaDto> Lineas);

/// <summary>DTO de lectura para línea de OC.</summary>
public record OrdenCompraLineaDto(
    long OrdenCompraLineaId,
    int NumeroLinea,
    long CompanyProductId,
    string ProductoSku,
    string Descripcion,
    string UnidadMedida,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal PorcentajeDescuento,
    decimal MontoDescuento,
    decimal Subtotal,
    decimal PorcentajeImpuesto,
    decimal MontoImpuesto,
    decimal TotalLinea,
    decimal CantidadRecepcionada,
    decimal CantidadPendiente);

/// <summary>DTO para crear una OC con sus líneas.</summary>
public record CreateOrdenCompraDto(
    DateTime FechaEmision,
    DateTime? FechaEntregaEstimada,
    int ProveedorId,
    int AlmacenDestinoId,
    string MonedaId = "BOB",
    decimal TasaCambio = 1,
    string? CondicionPago = null,
    string? Observaciones = null,
    string? ReferenciaExterna = null,
    List<CreateOrdenCompraLineaDto>? Lineas = null);

/// <summary>DTO para crear una línea de OC.</summary>
public record CreateOrdenCompraLineaDto(
    long CompanyProductId,
    string Descripcion,
    string UnidadMedida,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal PorcentajeDescuento = 0,
    decimal PorcentajeImpuesto = 0);

/// <summary>DTO para actualizar la cabecera de una OC (solo en Borrador).</summary>
public record UpdateOrdenCompraDto(
    DateTime FechaEmision,
    DateTime? FechaEntregaEstimada,
    int ProveedorId,
    int AlmacenDestinoId,
    string MonedaId,
    decimal TasaCambio,
    string? CondicionPago,
    string? Observaciones,
    string? ReferenciaExterna);

/// <summary>DTO para agregar una línea a una OC existente.</summary>
public record AddOrdenCompraLineaDto(
    long CompanyProductId,
    string Descripcion,
    string UnidadMedida,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal PorcentajeDescuento = 0,
    decimal PorcentajeImpuesto = 0);

/// <summary>DTO para actualizar una línea existente.</summary>
public record UpdateOrdenCompraLineaDto(
    string Descripcion,
    string UnidadMedida,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal PorcentajeDescuento,
    decimal PorcentajeImpuesto);
