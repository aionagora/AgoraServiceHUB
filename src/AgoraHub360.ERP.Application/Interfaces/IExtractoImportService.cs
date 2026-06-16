using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Bancario;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IExtractoImportService
{
    Task<Result<ImportExtractoResultDto>> ImportarAsync(int empresaId, int cuentaId, Stream archivo, string formato, CancellationToken ct = default);
}
