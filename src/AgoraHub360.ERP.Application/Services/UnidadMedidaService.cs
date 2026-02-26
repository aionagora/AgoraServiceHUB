namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

/// <summary>
/// Adapter: delegates to the unified UomService (Uom entity).
/// Keeps IUnidadMedidaService contract for backward compatibility with legacy controllers/pages.
/// </summary>
public class UnidadMedidaService : IUnidadMedidaService
{
    private readonly IUomService _uomService;

    public UnidadMedidaService(IUomService uomService)
    {
        _uomService = uomService;
    }

    public Task<Result<IReadOnlyList<UomDto>>> GetAllAsync(CancellationToken ct = default)
        => _uomService.GetAllAsync(ct);

    public Task<Result<UomDto>> GetByIdAsync(int id, CancellationToken ct = default)
        => _uomService.GetByIdAsync(id, ct);

    public Task<Result<UomDto>> CreateAsync(CreateUomDto dto, CancellationToken ct = default)
        => _uomService.CreateAsync(dto, ct);

    public Task<Result<UomDto>> UpdateAsync(int id, UpdateUomDto dto, CancellationToken ct = default)
        => _uomService.UpdateAsync(id, dto, ct);

    public Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
        => _uomService.DeleteAsync(id, ct);
}
