namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public interface IAsientoContableService
{
    // ?? Comprobantes ??
    /// <summary>Lista asientos con filtros opcionales.</summary>
    Task<Result<IReadOnlyList<AsientoContableDto>>> GetAllAsync(
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        string? estado = null,
        int? tipoComprobanteId = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>Obtiene un asiento con sus líneas.</summary>
    Task<Result<AsientoContableDto>> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>Crea un asiento contable manual en estado Borrador.</summary>
    Task<Result<AsientoContableDto>> CreateAsync(CreateAsientoContableDto dto, CancellationToken ct = default);

    /// <summary>Actualiza un asiento contable en Borrador.</summary>
    Task<Result<AsientoContableDto>> UpdateAsync(long id, UpdateAsientoContableDto dto, CancellationToken ct = default);

    /// <summary>Contabiliza un asiento en Borrador: valida partida doble y actualiza saldos.</summary>
    Task<Result<AsientoContableDto>> ContabilizarAsync(long id, CancellationToken ct = default);

    /// <summary>Anula un asiento contabilizado: revierte saldos.</summary>
    Task<Result<AsientoContableDto>> AnularAsync(long id, CancellationToken ct = default);

    /// <summary>Elimina un asiento en Borrador.</summary>
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);

    /// <summary>Realiza una copia de un asiento contable.</summary>
    Task<Result<AsientoContableDto>> CopiarAsync(long id, CancellationToken ct = default);

    // ?? Tablas de soporte ??
    Task<Result<IReadOnlyList<TipoComprobanteDto>>> GetTiposComprobanteAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<TipoCambioDto>>> GetTiposCambioAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<TipoPagoDto>>> GetTiposPagoAsync(CancellationToken ct = default);
    Task<Result<int>> SeedCatalogosAsync(CancellationToken ct = default);
}
