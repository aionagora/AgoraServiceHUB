using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Ventas;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface ISiatMetodoPagoService
{
    Task<Result<IReadOnlyList<SiatMetodoPagoDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<SiatMetodoPagoDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<SiatMetodoPagoDto>> CreateAsync(CrearSiatMetodoPagoRequestDto dto, CancellationToken ct = default);
    Task<Result<SiatMetodoPagoDto>> UpdateAsync(long id, ActualizarSiatMetodoPagoRequestDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<SiatMetodoPagoDto>>> SeedDefaultAsync(CancellationToken ct = default);
}
