using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.Ventas;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IVentaService
{
    Task<Result<PaginatedResultDto<VentaResumenDto>>> GetAllAsync(VentaFilterDto? filter = null, CancellationToken ct = default);
    Task<Result<VentaDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<VentaDto>> CreateAsync(CrearVentaRequestDto dto, CancellationToken ct = default);
    Task<Result<VentaDto>> UpdateAsync(long id, ActualizarVentaRequestDto dto, CancellationToken ct = default);
    Task<Result<VentaDto>> ConfirmarAsync(long id, ConfirmarVentaRequestDto? dto, CancellationToken ct = default);
    Task<Result<VentaDto>> CrearDesdePedidoAsync(GenerarVentaDesdePedidoRequestDto dto, CancellationToken ct = default);
    Task<Result<VentaDto>> RegistrarPagoAsync(long id, RegistrarPagoVentaRequestDto dto, CancellationToken ct = default);
    Task<Result<VentaDto>> AnularAsync(long id, AnularVentaRequestDto dto, CancellationToken ct = default);

    /// <summary>
    /// Obtiene pagos de ventas paginados y filtrados.
    /// </summary>
    Task<Result<PaginatedResultDto<VentaPagoDto>>> GetPagosPagedAsync(VentaPagoFilterDto filter, CancellationToken ct = default);

    /// <summary>
    /// Anula un pago de venta.
    /// </summary>
    Task<Result<VentaDto>> AnularPagoAsync(long id, AnularPagoVentaRequestDto dto, CancellationToken ct = default);
}
