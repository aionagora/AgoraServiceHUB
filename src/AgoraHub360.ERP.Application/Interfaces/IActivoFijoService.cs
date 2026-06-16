using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.ActivosFijos;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IActivoFijoService
{
    Task<Result<ActivoFijoDto>> RegistrarActivoAsync(CreateActivoFijoDto dto, CancellationToken ct = default);
    Task<Result<DepreciacionMensualResultDto>> EjecutarDepreciacionMensualAsync(int periodoId, CancellationToken ct = default);
    Task<Result<IEnumerable<DepreciacionReporteDto>>> GetReporteDepreciacionAsync(int anio, CancellationToken ct = default);
    Task<Result<ActivoFijoDto>> DarDeBajaAsync(long activoId, string motivo, CancellationToken ct = default);
}
