using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IFacturaVentaService
{
    Task<Result<PaginatedResultDto<FacturaVentaResumenDto>>> GetAllAsync(FacturaVentaFilterDto? filter = null, CancellationToken ct = default);
    Task<Result<FacturaVentaDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<FacturaVentaDto>> GetByVentaIdAsync(long ventaId, CancellationToken ct = default);
    Task<Result<FacturaVentaDto>> GenerarDesdeVentaAsync(GenerarFacturaVentaRequestDto dto, CancellationToken ct = default);
    Task<Result<FacturaVentaDto>> AnularAsync(long id, AnularFacturaVentaRequestDto dto, CancellationToken ct = default);
}
