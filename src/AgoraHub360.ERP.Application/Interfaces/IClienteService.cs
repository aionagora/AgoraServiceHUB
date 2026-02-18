namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Cliente;

public interface IClienteService
{
    Task<Result<IReadOnlyList<ClienteDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<ClienteDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<ClienteDto>> CreateAsync(CreateClienteDto dto, CancellationToken ct = default);
    Task<Result<ClienteDto>> UpdateAsync(int id, UpdateClienteDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
