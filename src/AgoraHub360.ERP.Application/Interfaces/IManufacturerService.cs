namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public interface IManufacturerService
{
    Task<Result<IReadOnlyList<ManufacturerDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<ManufacturerDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<ManufacturerDto>> CreateAsync(CreateManufacturerDto dto, CancellationToken ct = default);
    Task<Result<ManufacturerDto>> UpdateAsync(long id, UpdateManufacturerDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
