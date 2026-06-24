using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IContactoUsuarioAccesoService
{
    Task<Result<ContactoUsuarioAccesoDto>> GetByContactoAsync(int contactoId, CancellationToken ct = default);
    Task<Result<ContactoUsuarioAccesoDto>> UpsertAsync(int contactoId, UpsertContactoUsuarioAccesoDto dto, CancellationToken ct = default);
    Task<Result<bool>> SetActivoAsync(int contactoId, bool activo, CancellationToken ct = default);
}
