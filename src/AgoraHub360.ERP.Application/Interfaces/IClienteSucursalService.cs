using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IClienteSucursalService
{
    Task<Result<IReadOnlyList<ClienteSucursalDto>>> GetAllByClienteAsync(int clienteId, CancellationToken ct = default);
    Task<Result<ClienteSucursalDto>> GetByIdAsync(int clienteId, int id, CancellationToken ct = default);
    Task<Result<ClienteSucursalDto>> CreateAsync(int clienteId, CreateClienteSucursalDto dto, CancellationToken ct = default);
    Task<Result<ClienteSucursalDto>> UpdateAsync(int clienteId, int id, UpdateClienteSucursalDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int clienteId, int id, CancellationToken ct = default);
    Task<Result<bool>> SetPrincipalAsync(int clienteId, int id, CancellationToken ct = default);
}
