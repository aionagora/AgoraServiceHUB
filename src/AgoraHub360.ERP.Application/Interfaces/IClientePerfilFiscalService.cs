using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IClientePerfilFiscalService
{
    Task<Result<IReadOnlyList<ClientePerfilFiscalDto>>> GetAllByClienteAsync(int clienteId, CancellationToken ct = default);
    Task<Result<ClientePerfilFiscalDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<ClientePerfilFiscalDto>> CreateAsync(int clienteId, CreateClientePerfilFiscalDto dto, CancellationToken ct = default);
    Task<Result<ClientePerfilFiscalDto>> UpdateAsync(long id, UpdateClientePerfilFiscalDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result<bool>> SetPredeterminadoAsync(long id, CancellationToken ct = default);
}
