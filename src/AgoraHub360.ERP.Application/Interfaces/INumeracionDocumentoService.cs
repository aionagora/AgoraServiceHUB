namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Numeracion;

public interface INumeracionDocumentoService
{
    Task<Result<IReadOnlyList<NumeracionDocumentoDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<NumeracionDocumentoDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<NumeracionDocumentoDto>> CreateAsync(CreateNumeracionDto dto, CancellationToken ct = default);
    Task<Result<NumeracionDocumentoDto>> UpdateAsync(int id, UpdateNumeracionDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
    Task<Result<string>> GenerarSiguienteNumeroAsync(string tipoDocumento, long sucursalId, CancellationToken ct = default);
}
