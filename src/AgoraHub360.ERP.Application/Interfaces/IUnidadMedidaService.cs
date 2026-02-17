namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public interface IUnidadMedidaService
{
    Task<Result<IReadOnlyList<UnidadMedidaDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<UnidadMedidaDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<UnidadMedidaDto>> CreateAsync(CreateUnidadMedidaDto dto, CancellationToken ct = default);
    Task<Result<UnidadMedidaDto>> UpdateAsync(int id, UpdateUnidadMedidaDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
