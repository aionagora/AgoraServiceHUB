namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Parametro;

public interface IParametroSistemaService
{
    Task<Result<IReadOnlyList<ParametroSistemaDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<ParametroSistemaDto>>> GetByCategoriaAsync(string categoria, CancellationToken ct = default);
    Task<Result<ParametroSistemaDto>> GetByClaveAsync(string clave, CancellationToken ct = default);
    Task<Result<ParametroSistemaDto>> UpsertAsync(UpsertParametroDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
