namespace AgoraHub360.ERP.Shared.DTOs.Compras;

/// <summary>DTO de lectura para Hoja de Importación.</summary>
public record HojaImportacionDto(
    long HojaImportacionId,
    int EmpresaId,
    string Numero,
    long? OrdenCompraId,
    string? OrdenCompraNumero,
    long? ExpedienteImportacionId,
    string? ExpedienteNumero,
    DateTime Fecha,
    string? ReferenciaAduanera,
    string? Observaciones,
    byte MetodoDistribucion,
    string MetodoDistribucionNombre,
    decimal TotalGastos,
    bool Liquidada,
    List<GastoImportacionDto> Gastos,
    List<ImportacionLineaDto> Lineas);

/// <summary>DTO de lectura para gasto de importación.</summary>
public record GastoImportacionDto(
    long GastoImportacionId,
    string TipoGasto,
    string? Descripcion,
    decimal Monto,
    string MonedaId,
    decimal TasaCambio,
    decimal MontoBase,
    string? Referencia);

/// <summary>DTO de lectura para línea de distribución.</summary>
public record ImportacionLineaDto(
    long ImportacionLineaId,
    long OrdenCompraLineaId,
    int NumeroLineaOC,
    string ProductoSku,
    string Descripcion,
    decimal CantidadRecepcionada,
    decimal CostoFobUnitario,
    decimal CostoFobTotal,
    decimal FactorDistribucion,
    decimal GastoAsignado,
    decimal CostoLandedUnitario,
    decimal CostoLandedTotal);

/// <summary>DTO para crear una hoja de importación (vinculada a OC directa).</summary>
public record CreateHojaImportacionDto(
    long? OrdenCompraId = null,
    long? ExpedienteImportacionId = null,
    DateTime Fecha = default,
    string? ReferenciaAduanera = null,
    string? Observaciones = null,
    byte MetodoDistribucion = 3,
    List<CreateGastoImportacionDto>? Gastos = null);

/// <summary>DTO para crear un gasto de importación.</summary>
public record CreateGastoImportacionDto(
    string TipoGasto,
    string? Descripcion,
    decimal Monto,
    string MonedaId = "BOB",
    decimal TasaCambio = 1,
    string? Referencia = null);

/// <summary>DTO para agregar un gasto a una hoja existente.</summary>
public record AddGastoImportacionDto(
    string TipoGasto,
    string? Descripcion,
    decimal Monto,
    string MonedaId = "BOB",
    decimal TasaCambio = 1,
    string? Referencia = null);
