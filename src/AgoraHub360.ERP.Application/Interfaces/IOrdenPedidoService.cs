namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Compras;

public interface IOrdenPedidoService
{
    /// <summary>Lista todas las OP de la empresa con filtros opcionales.</summary>
    Task<Result<IReadOnlyList<OrdenPedidoDto>>> GetAllAsync(
        string? estado = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        CancellationToken ct = default);

    /// <summary>Obtiene una OP con sus líneas.</summary>
    Task<Result<OrdenPedidoDto>> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>Crea una OP en estado Borrador.</summary>
    Task<Result<OrdenPedidoDto>> CreateAsync(CreateOrdenPedidoDto dto, CancellationToken ct = default);

    /// <summary>Actualiza la cabecera de una OP en Borrador.</summary>
    Task<Result<OrdenPedidoDto>> UpdateAsync(long id, UpdateOrdenPedidoDto dto, CancellationToken ct = default);

    /// <summary>
    /// Envía la OP a Compras Central (Borrador ? EnRevision).
    /// </summary>
    Task<Result<OrdenPedidoDto>> EnviarARevisionAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Registra la revisión de stock: valida si el stock cubre la demanda.
    /// Si cubre ? AbastecidoConStock. Si no ? PendienteAprobacion.
    /// </summary>
    Task<Result<OrdenPedidoDto>> RevisarStockAsync(long id, RevisarStockOrdenPedidoDto dto, CancellationToken ct = default);

    /// <summary>
    /// Aprueba o rechaza la OP (gate de Finanzas/Dirección).
    /// Aprobado ? Aprobado | Rechazado ? Rechazado.
    /// </summary>
    Task<Result<OrdenPedidoDto>> AprobarRechazarAsync(long id, AprobarRechazarOrdenPedidoDto dto, CancellationToken ct = default);

    /// <summary>Anula la OP.</summary>
    Task<Result<OrdenPedidoDto>> AnularAsync(long id, string? motivo, CancellationToken ct = default);

    /// <summary>Elimina (soft-delete) una OP en Borrador.</summary>
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
