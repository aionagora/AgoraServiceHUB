namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Logistica;

public interface IHojaRutaService
{
    Task<Result<IReadOnlyList<HojaRutaDto>>> GetAllAsync(
        string? estado = null,
        string? subEstado = null,
        string? tipoOP = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        string? search = null,
        CancellationToken ct = default);

    Task<Result<HojaRutaDto>> GetByIdAsync(long id, CancellationToken ct = default);

    Task<Result<HojaRutaDto>> CreateAsync(CreateHojaRutaDto dto, CancellationToken ct = default);

    Task<Result<HojaRutaDto>> UpdateAsync(long id, UpdateHojaRutaDto dto, CancellationToken ct = default);

    Task<Result<HojaRutaDto>> CambiarEstadoAsync(long id, CambiarEstadoHojaRutaDto dto, CancellationToken ct = default);

    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);

    /// <summary>Devuelve la(s) hoja(s) de ruta vinculadas a una OrdenPedido.</summary>
    Task<Result<List<HojaRutaDto>>> GetByOrdenPedidoAsync(long ordenPedidoId, CancellationToken ct = default);
}
