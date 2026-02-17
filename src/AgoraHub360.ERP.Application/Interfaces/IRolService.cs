namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Rol;

public interface IRolService
{
    Task<Result<IReadOnlyList<RolDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<RolDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<RolDto>> CreateAsync(CreateRolDto dto, CancellationToken ct = default);
    Task<Result<RolDto>> UpdateAsync(int id, UpdateRolDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
