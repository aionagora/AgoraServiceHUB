namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Compras;

public interface IOrdenCompraService
{
    /// <summary>Lista todas las OC de la empresa activa con filtros opcionales.</summary>
    Task<Result<IReadOnlyList<OrdenCompraDto>>> GetAllAsync(
        int? proveedorId = null,
        string? estado = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        CancellationToken ct = default);

    /// <summary>Obtiene una OC con sus líneas por Id.</summary>
    Task<Result<OrdenCompraDto>> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>Crea una OC en estado Borrador con sus líneas.</summary>
    Task<Result<OrdenCompraDto>> CreateAsync(CreateOrdenCompraDto dto, CancellationToken ct = default);

    /// <summary>Actualiza la cabecera de una OC en Borrador.</summary>
    Task<Result<OrdenCompraDto>> UpdateAsync(long id, UpdateOrdenCompraDto dto, CancellationToken ct = default);

    /// <summary>Agrega una línea a una OC en Borrador.</summary>
    Task<Result<OrdenCompraDto>> AddLineaAsync(long ordenId, AddOrdenCompraLineaDto dto, CancellationToken ct = default);

    /// <summary>Actualiza una línea de una OC en Borrador.</summary>
    Task<Result<OrdenCompraDto>> UpdateLineaAsync(long ordenId, long lineaId, UpdateOrdenCompraLineaDto dto, CancellationToken ct = default);

    /// <summary>Elimina una línea de una OC en Borrador.</summary>
    Task<Result<OrdenCompraDto>> RemoveLineaAsync(long ordenId, long lineaId, CancellationToken ct = default);

    /// <summary>
    /// Cambia el estado de la OC según la máquina de estados del flujo de importación.
    /// Borrador?Confirmado?PendienteAprobacion?Aprobado?EnviadaProveedor
    /// ?(EnNegociacion)?ConfirmadaProveedor?PagoProgramado?EnTransito / Anulado / Rechazado.
    /// </summary>
    Task<Result<OrdenCompraDto>> CambiarEstadoAsync(long id, string nuevoEstado, CancellationToken ct = default);

    /// <summary>
    /// Gate de aprobación [Finanzas/Dirección]: aprueba o rechaza la OC.
    /// PendienteAprobacion ? Aprobado | Rechazado.
    /// </summary>
    Task<Result<OrdenCompraDto>> AprobarRechazarAsync(long id, AprobarRechazarOrdenCompraDto dto, CancellationToken ct = default);

    /// <summary>
    /// Gate [4]: registra la confirmación o negociación del proveedor (PI/Proforma).
    /// EnviadaProveedor|EnNegociacion ? CondicionesOK=true ? ConfirmadaProveedor
    ///                                ? CondicionesOK=false ? EnNegociacion.
    /// </summary>
    Task<Result<ConfirmacionProveedorDto>> RegistrarConfirmacionProveedorAsync(
        long id, RegistrarConfirmacionProveedorDto dto, CancellationToken ct = default);

    /// <summary>Obtiene el historial de confirmaciones/negociaciones del proveedor para una OC.</summary>
    Task<Result<IReadOnlyList<ConfirmacionProveedorDto>>> GetConfirmacionesAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Gate [5]: programa un pago (anticipo o saldo) para la OC.
    /// ConfirmadaProveedor ? PagoProgramado (primer pago programado).
    /// </summary>
    Task<Result<PagoOrdenCompraDto>> ProgramarPagoAsync(long id, ProgramarPagoDto dto, CancellationToken ct = default);

    /// <summary>Registra la ejecución real de un pago previamente programado.</summary>
    Task<Result<PagoOrdenCompraDto>> EjecutarPagoAsync(long id, long pagoId, EjecutarPagoDto dto, CancellationToken ct = default);

    /// <summary>Lista todos los pagos programados/ejecutados de una OC.</summary>
    Task<Result<IReadOnlyList<PagoOrdenCompraDto>>> GetPagosAsync(long id, CancellationToken ct = default);

    /// <summary>Elimina (soft-delete) una OC en Borrador.</summary>
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
