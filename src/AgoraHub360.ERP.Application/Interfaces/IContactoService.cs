using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IContactoService
{
    Task<Result<IReadOnlyList<ContactoDto>>> GetAllByClienteAsync(int clienteId, CancellationToken ct = default);
    Task<Result<ContactoDto>> GetByIdAsync(int clienteId, int id, CancellationToken ct = default);
    Task<Result<ContactoDto>> CreateAsync(int clienteId, CreateContactoDto dto, CancellationToken ct = default);
    Task<Result<ContactoDto>> UpdateAsync(int clienteId, int id, UpdateContactoDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int clienteId, int id, CancellationToken ct = default);
}
