namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public interface IComprobanteDocumentoService
{
    /// <summary>Lista todos los documentos adjuntos a un comprobante.</summary>
    Task<Result<IReadOnlyList<ComprobanteDocumentoDto>>> GetByComprobanteAsync(
        long comprobanteId, CancellationToken ct = default);

    /// <summary>Adjunta un documento existente a un comprobante.</summary>
    Task<Result<ComprobanteDocumentoDto>> AdjuntarAsync(
        long comprobanteId, AdjuntarDocumentoDto dto, CancellationToken ct = default);

    /// <summary>Elimina el vínculo entre un comprobante y un documento.</summary>
    Task<Result<bool>> RemoverAsync(
        long comprobanteId, int docId, CancellationToken ct = default);
}
