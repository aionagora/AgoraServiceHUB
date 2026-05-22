namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Inventario;

public interface IMovimientoInventarioService
{
    /// <summary>Obtiene todos los movimientos de la empresa activa, con filtros opcionales.</summary>
    Task<Result<IReadOnlyList<MovimientoInventarioDto>>> GetAllAsync(
        int? productoId = null,
        int? almacenId = null,
        string? tipoMovimiento = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        CancellationToken ct = default);

    /// <summary>Obtiene un movimiento por Id.</summary>
    Task<Result<MovimientoInventarioDto>> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Registra un nuevo movimiento y actualiza el stock.</summary>
    Task<Result<MovimientoInventarioDto>> CreateAsync(CreateMovimientoInventarioDto dto, CancellationToken ct = default);

    /// <summary>Obtiene el kardex (movimientos con saldo acumulado) de un producto.</summary>
    Task<Result<KardexDto>> GetKardexAsync(
        long companyProductId,
        int? almacenId = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        CancellationToken ct = default);

    /// <summary>Obtiene el stock actual de todos los productos de la empresa.</summary>
    Task<Result<IReadOnlyList<StockProductoDto>>> GetStockAsync(
        int? almacenId = null,
        CancellationToken ct = default);
}
