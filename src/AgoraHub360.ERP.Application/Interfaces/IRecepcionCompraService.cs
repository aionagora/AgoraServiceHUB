namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Compras;

public interface IRecepcionCompraService
{
    /// <summary>Lista recepciones de la empresa activa con filtros opcionales.</summary>
    Task<Result<IReadOnlyList<RecepcionCompraDto>>> GetAllAsync(
        long? ordenCompraId = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        CancellationToken ct = default);

    /// <summary>Obtiene una recepción con sus líneas por Id.</summary>
    Task<Result<RecepcionCompraDto>> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Crea una recepción, actualiza CantidadRecepcionada en las líneas de OC,
    /// genera movimientos de inventario Receipt y actualiza stock/WAC.
    /// </summary>
    Task<Result<RecepcionCompraDto>> CreateAsync(CreateRecepcionCompraDto dto, CancellationToken ct = default);

    /// <summary>Anula una recepción no confirmada (soft-delete).</summary>
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
