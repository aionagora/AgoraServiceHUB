using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Sucursal;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface ISucursalService
{
    Task<Result<IReadOnlyList<SucursalListadoDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<SucursalDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<SucursalDto>> CreateAsync(CrearSucursalDto dto, CancellationToken ct = default);
    Task<Result<SucursalDto>> UpdateAsync(int id, ActualizarSucursalDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
    Task<Result<bool>> CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default);
    Task<Result<bool>> EstablecerCentralAsync(int id, CancellationToken ct = default);
}
