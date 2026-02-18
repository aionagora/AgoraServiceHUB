namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public interface IAlmacenService
{
    Task<Result<IReadOnlyList<AlmacenDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<AlmacenDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<AlmacenDto>> CreateAsync(CreateAlmacenDto dto, CancellationToken ct = default);
    Task<Result<AlmacenDto>> UpdateAsync(int id, UpdateAlmacenDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
