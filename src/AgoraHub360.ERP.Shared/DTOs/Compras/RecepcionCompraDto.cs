namespace AgoraHub360.ERP.Shared.DTOs.Compras;

/// <summary>DTO de lectura para Recepción de Compra.</summary>
public record RecepcionCompraDto(
    long RecepcionCompraId,
    int EmpresaId,
    string Numero,
    long OrdenCompraId,
    string OrdenCompraNumero,
    DateTime FechaRecepcion,
    int AlmacenId,
    string AlmacenNombre,
    string? DocumentoProveedor,
    string? Observaciones,
    bool Confirmada,
    bool TieneDiferencias,
    string? TipoDiferencia,
    string? ActaDiferencias,
    string? NumeroReclamo,
    bool EnCuarentena,
    string? UbicacionCuarentena,
    string? ResultadoControlCalidad,
    List<RecepcionCompraLineaDto> Lineas);

/// <summary>DTO de lectura para línea de recepción.</summary>
public record RecepcionCompraLineaDto(
    long RecepcionCompraLineaId,
    long OrdenCompraLineaId,
    int NumeroLineaOC,
    string ProductoSku,
    string Descripcion,
    string UnidadMedida,
    decimal CantidadOrdenada,
    decimal CantidadPendiente,
    decimal CantidadRecibida,
    decimal CantidadDañada,
    decimal CantidadSobrante,
    decimal CantidadFaltante,
    decimal CantidadAceptada,
    decimal CostoUnitario,
    string? Notas);

/// <summary>DTO para crear una recepción con sus líneas.</summary>
public record CreateRecepcionCompraDto(
    long OrdenCompraId,
    DateTime FechaRecepcion,
    int? AlmacenId = null,
    string? DocumentoProveedor = null,
    string? Observaciones = null,
    bool TieneDiferencias = false,
    string? TipoDiferencia = null,
    string? ActaDiferencias = null,
    string? NumeroReclamo = null,
    bool EnCuarentena = false,
    string? UbicacionCuarentena = null,
    string? ResultadoControlCalidad = null,
    List<CreateRecepcionCompraLineaDto>? Lineas = null);

/// <summary>DTO para crear una línea de recepción.</summary>
public record CreateRecepcionCompraLineaDto(
    long OrdenCompraLineaId,
    decimal CantidadRecibida,
    decimal CantidadDañada = 0,
    decimal CantidadSobrante = 0,
    decimal CantidadFaltante = 0,
    decimal? CostoUnitario = null,
    string? Notas = null);
