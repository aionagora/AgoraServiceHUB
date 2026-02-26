namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public interface ICatalogService
{
    Task<Result<IReadOnlyList<CatalogDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<CatalogDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<CatalogDto>> CreateAsync(CreateCatalogDto dto, CancellationToken ct = default);
    Task<Result<CatalogDto>> UpdateAsync(long id, UpdateCatalogDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
