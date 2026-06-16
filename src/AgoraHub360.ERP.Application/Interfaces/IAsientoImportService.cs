using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad.Importacion;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IAsientoImportService
{
    Task<ImportValidacionDto> ValidarArchivoAsync(Stream excelStream, int empresaId, CancellationToken ct = default);
    Task<ImportResultDto> ImportarAsync(Stream excelStream, int empresaId, bool contabilizarInmediatamente = false, CancellationToken ct = default);
    Task<byte[]> GenerarPlantillaAsync(CancellationToken ct = default);
}
