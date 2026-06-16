using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;
using DomainResult = AgoraHub360.ERP.Domain.Common.Result;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IGestionContableContextService
{
    Task<Result<IEnumerable<GestionContableDto>>> GetGestionesDisponiblesAsync(CancellationToken ct = default);
    Task<Result<GestionContableDto?>> GetGestionSugeridaAsync(CancellationToken ct = default);
    Task<DomainResult> ValidarGestionAsync(int anio, CancellationToken ct = default);
}
