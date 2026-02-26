namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Shared.DTOs.PRC;

public interface IPriceListService
{
    Task<Result<IReadOnlyList<PriceListDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<PriceListDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<PriceListDto>> CreateAsync(CreatePriceListDto dto, CancellationToken ct = default);
    Task<Result<PriceListDto>> UpdateAsync(long id, UpdatePriceListDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken ct = default);

    Task<Result<IReadOnlyList<PriceListItemDto>>> GetItemsAsync(long priceListId, CancellationToken ct = default);
    Task<Result<PriceListItemDto>> AddItemAsync(CreatePriceListItemDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteItemAsync(long itemId, CancellationToken ct = default);
}
