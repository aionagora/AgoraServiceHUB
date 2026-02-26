namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public interface IAsientoContableService
{
    /// <summary>Lista asientos con filtros opcionales.</summary>
    Task<Result<IReadOnlyList<AsientoContableDto>>> GetAllAsync(
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        string? estado = null,
        string? origenTipo = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>Obtiene un asiento con sus líneas.</summary>
    Task<Result<AsientoContableDto>> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>Crea un asiento contable manual en estado Borrador.</summary>
    Task<Result<AsientoContableDto>> CreateAsync(CreateAsientoContableDto dto, CancellationToken ct = default);

    /// <summary>Contabiliza un asiento en Borrador: valida partida doble y actualiza saldos.</summary>
    Task<Result<AsientoContableDto>> ContabilizarAsync(long id, CancellationToken ct = default);

    /// <summary>Anula un asiento contabilizado: revierte saldos.</summary>
    Task<Result<AsientoContableDto>> AnularAsync(long id, CancellationToken ct = default);

    /// <summary>Elimina un asiento en Borrador.</summary>
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
