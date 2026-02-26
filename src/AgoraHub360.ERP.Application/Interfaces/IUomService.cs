namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public interface IUomService
{
    Task<Result<IReadOnlyList<UomDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<UomDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<UomDto>> CreateAsync(CreateUomDto dto, CancellationToken ct = default);
    Task<Result<UomDto>> UpdateAsync(int id, UpdateUomDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
