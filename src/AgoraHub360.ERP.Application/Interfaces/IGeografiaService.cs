using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.Common;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IGeografiaService
{
    Task<Result<IReadOnlyList<GeoCatalogItemDto>>> GetPaisesAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<GeoCatalogItemDto>>> GetDepartamentosByPaisAsync(Guid paisId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<GeoCatalogItemDto>>> GetProvinciasByDepartamentoAsync(Guid departamentoId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<GeoCatalogItemDto>>> GetCiudadesByProvinciaAsync(Guid provinciaId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<GeoCatalogItemDto>>> GetZonasByCiudadAsync(Guid ciudadId, CancellationToken ct = default);
}
