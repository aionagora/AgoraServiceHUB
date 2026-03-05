namespace AgoraHub360.ERP.Shared.DTOs.Compras;

/// <summary>DTO de lectura para Expediente de Importación.</summary>
public record ExpedienteImportacionDto(
    long ExpedienteImportacionId,
    int EmpresaId,
    string Numero,
    string Estado,
    string? Incoterm,
    string? ModalidadTransporte,
    string? PaisOrigen,
    string? PuertoOrigen,
    string? PuertoDestino,
    string? Forwarder,
    string? Aseguradora,
    string? NumeroPólizaSeguro,
    string? NumeroBLAWB,
    DateTime? ETD,
    DateTime? ATD,
    DateTime? ETA,
    DateTime? ATA,
    string? NumeroDUIDIM,
    string? Despachante,
    DateTime? FechaPresentacionAduana,
    decimal? TotalTributos,
    bool TuvoObservacionAduana,
    string? DetalleObservacionAduana,
    DateTime? FechaLevante,
    string? Observaciones,
    bool Activo,
    List<long> OrdenesCompraIds,
    List<string> OrdenesCompraNumeros,
    List<HitoExpedienteDto> Hitos);

/// <summary>DTO de lectura para un hito de tracking.</summary>
public record HitoExpedienteDto(
    long HitoExpedienteId,
    string TipoHito,
    DateTime FechaHito,
    string? Descripcion,
    string? ReferenciaDocumento);

/// <summary>DTO para crear un Expediente de Importación.</summary>
public record CreateExpedienteImportacionDto(
    List<long> OrdenesCompraIds,
    string? Incoterm = null,
    string? ModalidadTransporte = null,
    string? PaisOrigen = null,
    string? PuertoOrigen = null,
    string? PuertoDestino = null,
    string? Forwarder = null,
    string? Aseguradora = null,
    string? NumeroPólizaSeguro = null,
    string? Observaciones = null);

/// <summary>DTO para actualizar datos de embarque del expediente.</summary>
public record UpdateExpedienteEmbarqueDto(
    string? Incoterm,
    string? ModalidadTransporte,
    string? PaisOrigen,
    string? PuertoOrigen,
    string? PuertoDestino,
    string? Forwarder,
    string? Aseguradora,
    string? NumeroPólizaSeguro,
    string? NumeroBLAWB,
    DateTime? ETD,
    DateTime? ETA,
    string? Observaciones);

/// <summary>DTO para registrar el arribo y actualizar ATA/ATD.</summary>
public record RegistrarArriboDto(
    DateTime FechaArribo,
    string? Observaciones = null);

/// <summary>DTO para registrar el despacho aduanero.</summary>
public record RegistrarDespachoAduaneroDto(
    string NumeroDUIDIM,
    string Despachante,
    DateTime FechaPresentacion,
    decimal TotalTributos,
    string? Observaciones = null);

/// <summary>DTO para registrar una observación/aforo aduanero.</summary>
public record RegistrarObservacionAduanaDto(
    string DetalleObservacion,
    string? Observaciones = null);

/// <summary>DTO para registrar el levante/liberación aduanera.</summary>
public record RegistrarLevanteDto(
    DateTime FechaLevante,
    string? Observaciones = null);

/// <summary>DTO para agregar un hito de tracking al expediente.</summary>
public record AddHitoExpedienteDto(
    string TipoHito,
    DateTime FechaHito,
    string? Descripcion = null,
    string? ReferenciaDocumento = null);
