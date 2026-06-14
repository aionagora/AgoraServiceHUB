using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Seguridad;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface ISeguridadDinamicaService
{
    Task<Result<SesionContextoDto>> GetSesionContextAsync(int? usuarioId = null, int? empresaId = null, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ModuloSistemaDto>>> GetMenuUsuarioAsync(int? usuarioId = null, int? empresaId = null, CancellationToken ct = default);
    Task<Result<bool>> HasPermissionAsync(string formularioCodigo, string? accionCodigo = null, int? usuarioId = null, int? empresaId = null, CancellationToken ct = default);
    Task<Result<PerfilAccesoDto>> CreatePerfilAsync(UpsertPerfilAccesoDto dto, CancellationToken ct = default);
    Task<Result<bool>> SetPermisoPerfilAsync(UpsertPerfilPermisoDto dto, CancellationToken ct = default);
    Task<Result<bool>> AsignarPerfilUsuarioAsync(AsignarUsuarioPerfilDto dto, CancellationToken ct = default);
}
