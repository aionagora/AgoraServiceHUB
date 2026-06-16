using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IPedidoVentaService
{
    Task<Result<IReadOnlyList<PedidoVentaDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<PaginatedResultDto<PedidoVentaDto>>> GetPagedAsync(PedidoVentaFilterDto filter, CancellationToken ct = default);
    Task<Result<PedidoVentaDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<PedidoVentaDto>> CreateAsync(CreatePedidoVentaDto dto, CancellationToken ct = default);
    Task<Result<PedidoVentaDto>> UpdateAsync(long id, UpdatePedidoVentaDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result<bool>> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result<bool>> DispatchAsync(long id, CancellationToken ct = default);
    Task<Result<bool>> CancelAsync(long id, CancellationToken ct = default);
    Task<Result<bool>> MarkDeliveredAsync(long id, CancellationToken ct = default);
}
