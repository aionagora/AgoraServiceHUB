namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Compras;

public interface IHojaImportacionService
{
    /// <summary>Lista hojas de importación con filtros opcionales.</summary>
    Task<Result<IReadOnlyList<HojaImportacionDto>>> GetAllAsync(
        long? ordenCompraId = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        CancellationToken ct = default);

    /// <summary>Obtiene una hoja con gastos y líneas de distribución.</summary>
    Task<Result<HojaImportacionDto>> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>Crea una hoja de importación con gastos iniciales (sin liquidar).</summary>
    Task<Result<HojaImportacionDto>> CreateAsync(CreateHojaImportacionDto dto, CancellationToken ct = default);

    /// <summary>Agrega un gasto a una hoja no liquidada.</summary>
    Task<Result<HojaImportacionDto>> AddGastoAsync(long hojaId, AddGastoImportacionDto dto, CancellationToken ct = default);

    /// <summary>Elimina un gasto de una hoja no liquidada.</summary>
    Task<Result<HojaImportacionDto>> RemoveGastoAsync(long hojaId, long gastoId, CancellationToken ct = default);

    /// <summary>
    /// Liquida la hoja: distribuye gastos sobre líneas de OC según método,
    /// recalcula Landed Cost unitario y actualiza StockProducto.AverageCost.
    /// </summary>
    Task<Result<HojaImportacionDto>> LiquidarAsync(long id, CancellationToken ct = default);

    /// <summary>Elimina una hoja no liquidada (soft-delete).</summary>
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
