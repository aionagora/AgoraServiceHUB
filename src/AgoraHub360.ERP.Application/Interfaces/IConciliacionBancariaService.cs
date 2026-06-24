using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Bancario;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IConciliacionBancariaService
{
    Task<Result<ImportExtractoResultDto>> ImportarExtractoAsync(int cuentaId, int periodoId, Stream archivo, string formato, CancellationToken ct = default);
    Task<Result<IEnumerable<SugerenciaConciliacionDto>>> GetSugerenciasAsync(int cuentaId, int periodoId, CancellationToken ct = default);
    Task<Result<bool>> ConciliarAsync(long extractoId, long asientoLineaId, CancellationToken ct = default);
    Task<Result<bool>> DesconciliarAsync(long extractoId, CancellationToken ct = default);
    Task<Result<ResumenConciliacionDto>> GetResumenAsync(int cuentaId, int periodoId, CancellationToken ct = default);
    Task<Result<bool>> AprobarAsync(int conciliacionId, CancellationToken ct = default);
}
