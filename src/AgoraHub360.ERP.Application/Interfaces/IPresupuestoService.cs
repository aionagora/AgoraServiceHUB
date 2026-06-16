using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Presupuestos;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IPresupuestoService
{
    Task<Result<PresupuestoContableDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<IEnumerable<PresupuestoVsRealDto>>> GetComparativoAsync(int gestion, int? mes, int? cuentaId, int? centroCostoId, CancellationToken ct = default);
    Task<Result<PresupuestoContableDto>> CreateAsync(CreatePresupuestoDto dto, CancellationToken ct = default);
    Task<Result<bool>> AprobarAsync(int id, CancellationToken ct = default);
    Task<Result<CargaMasivaResultDto>> ImportarDesdeExcelAsync(int gestion, Stream excelStream, CancellationToken ct = default);
}
