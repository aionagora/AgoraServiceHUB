using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Usuario;
using AgoraHub360.ERP.Shared.DTOs.Seguridad;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IUsuarioSucursalAccesoService
{
    Task<Result<IReadOnlyList<UsuarioSucursalAccesoDto>>> GetByUsuarioAsync(int usuarioId, CancellationToken ct = default);
    Task<Result<UsuarioSucursalAccesoDto>> UpsertAsync(int usuarioId, UpsertUsuarioSucursalAccesoDto dto, CancellationToken ct = default);
    Task<Result<bool>> SetPredeterminadaAsync(int usuarioId, int sucursalId, CancellationToken ct = default);
    Task<Result<bool>> RemoveAsync(int usuarioId, int sucursalId, CancellationToken ct = default);
    Task<Result<ValidarSucursalAccesoDto>> ValidarAccesoActualAsync(int sucursalId, bool requiereOperacion = false, CancellationToken ct = default);
}
