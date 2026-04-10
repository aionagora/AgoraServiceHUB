namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Compras;

public interface IExpedienteImportacionService
{
    /// <summary>Lista expedientes con filtros opcionales.</summary>
    Task<Result<IReadOnlyList<ExpedienteImportacionDto>>> GetAllAsync(
        string? estado = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        CancellationToken ct = default);

    /// <summary>Obtiene un expediente con sus hitos y OCs vinculadas.</summary>
    Task<Result<ExpedienteImportacionDto>> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Crea un expediente vinculando 1..N OCs (deben estar en ConfirmadaProveedor o PagoProgramado).
    /// Estado inicial: Borrador.
    /// </summary>
    Task<Result<ExpedienteImportacionDto>> CreateAsync(CreateExpedienteImportacionDto dto, CancellationToken ct = default);

    /// <summary>Actualiza datos de embarque (forwarder, BL/AWB, ETD, ETA, etc.).</summary>
    Task<Result<ExpedienteImportacionDto>> UpdateEmbarqueAsync(long id, UpdateExpedienteEmbarqueDto dto, CancellationToken ct = default);

    /// <summary>
    /// Confirma el despacho/salida: Borrador ? EnTransito.
    /// Actualiza ATD y genera hito.
    /// </summary>
    Task<Result<ExpedienteImportacionDto>> ConfirmarSalidaAsync(long id, DateTime fechaSalida, CancellationToken ct = default);

    /// <summary>
    /// Registra el arribo: EnTransito ? Arribado.
    /// Actualiza ATA y genera hito.
    /// </summary>
    Task<Result<ExpedienteImportacionDto>> RegistrarArriboAsync(long id, RegistrarArriboDto dto, CancellationToken ct = default);

    /// <summary>
    /// Inicia despacho aduanero: Arribado ? EnAduana.
    /// Registra DUI/DIM, despachante y tributos.
    /// </summary>
    Task<Result<ExpedienteImportacionDto>> IniciarDespachoAduaneroAsync(long id, RegistrarDespachoAduaneroDto dto, CancellationToken ct = default);

    /// <summary>
    /// Registra observación/aforo aduanero: EnAduana ? ObservacionAduana.
    /// </summary>
    Task<Result<ExpedienteImportacionDto>> RegistrarObservacionAduanaAsync(long id, RegistrarObservacionAduanaDto dto, CancellationToken ct = default);

    /// <summary>
    /// Subsana observación y vuelve a despacho: ObservacionAduana ? EnAduana.
    /// </summary>
    Task<Result<ExpedienteImportacionDto>> SubsanarObservacionAsync(long id, string? observaciones, CancellationToken ct = default);

    /// <summary>
    /// Registra levante/liberación aduanera: EnAduana ? Liberado.
    /// </summary>
    Task<Result<ExpedienteImportacionDto>> RegistrarLevanteAsync(long id, RegistrarLevanteDto dto, CancellationToken ct = default);

    /// <summary>Agrega un hito de tracking al expediente.</summary>
    Task<Result<ExpedienteImportacionDto>> AddHitoAsync(long id, AddHitoExpedienteDto dto, CancellationToken ct = default);

    /// <summary>Cierra el expediente (Liberado ? Cerrado) cuando todas las OCs están cerradas.</summary>
    Task<Result<ExpedienteImportacionDto>> CerrarAsync(long id, CancellationToken ct = default);

    /// <summary>Elimina (soft-delete) un expediente en Borrador.</summary>
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
