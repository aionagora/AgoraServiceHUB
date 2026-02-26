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

    /// <summary>Cambia el estado de la OC: Borrador?Confirmado?Aprobado / Anulado.</summary>
    Task<Result<OrdenCompraDto>> CambiarEstadoAsync(long id, string nuevoEstado, CancellationToken ct = default);

    /// <summary>Elimina (soft-delete) una OC en Borrador.</summary>
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
