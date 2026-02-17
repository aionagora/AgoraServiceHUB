namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public interface IProveedorService
{
    Task<Result<IReadOnlyList<ProveedorDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<ProveedorDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<ProveedorDto>> CreateAsync(CreateProveedorDto dto, CancellationToken ct = default);
    Task<Result<ProveedorDto>> UpdateAsync(int id, UpdateProveedorDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
